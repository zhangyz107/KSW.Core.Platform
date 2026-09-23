using KSW.Localization;
using KSW.Ui;
using KSW.UI.WPF.Language;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace KSW.UI.WPF.ViewModels
{
    public class LogViewerDialogViewModel : ViewModelBase, IDialogAware
    {
        private readonly InMemoryLogSink _sink;
        private readonly DispatcherTimer _timer;
        private const int recentCount = 5000;       //显示最近5000条日志
        private LogEventLevel? _minLevel;
        private string _filterKeyword = "";
        private int _autoScrollPosition;
        private LogEntry _selectedLog;
        private string _selectedLogLevel = LanguageHelper.Manager["All"];
        private ObservableCollection<LogEntry> _displayedEntries = new();
        private string _statusText;

        #region Properties
        public string Title => LanguageHelper.Manager["LogViewer"];

        public DialogCloseListener RequestClose { get; }

        public ListCollectionView DisplayedEntriesView { get; private set; }

        public ObservableCollection<LogEntry> DisplayedEntries
        {
            get => _displayedEntries;
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string FilterKeyword
        {
            get => _filterKeyword;
            set
            {
                if (SetProperty(ref _filterKeyword, value))
                    DisplayedEntriesView?.Refresh();
            }
        }

        public string SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                if (SetProperty(ref _selectedLogLevel, value))
                {
                    if (Enum.TryParse(_selectedLogLevel, out LogEventLevel logLevel))
                        SetLevelFilter(logLevel);
                    else
                        SetLevelFilter(null);

                    DisplayedEntriesView?.Refresh();
                }
            }
        }

        public LogEntry SelectedLog
        {
            get => _selectedLog;
            set => SetProperty(ref _selectedLog, value);
        }
        #endregion

        #region Commands
        private DelegateCommand _copyCommand;
        public DelegateCommand CopyCommand =>
            _copyCommand ?? (_copyCommand = new DelegateCommand(ExecuteCopyCommand));

        private DelegateCommand _clearLogsCommand;
        public DelegateCommand ClearLogsCommand =>
            _clearLogsCommand ?? (_clearLogsCommand = new DelegateCommand(ExecuteClearLogsCommand));

        private DelegateCommand _exportLogsCommand;
        public DelegateCommand ExportLogsCommand =>
            _exportLogsCommand ?? (_exportLogsCommand = new DelegateCommand(ExecuteExportLogsCommand));
        #endregion

        #region WinAPI

        [DllImport("User32")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32")]
        public static extern bool CloseClipboard();

        [DllImport("User32")]
        public static extern bool EmptyClipboard();

        [DllImport("User32")]
        public static extern bool IsClipboardFormatAvailable(int format);

        [DllImport("User32")]
        public static extern IntPtr GetClipboardData(int uFormat);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);
        #endregion

        public LogViewerDialogViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            _sink = containerProvider?.Resolve<InMemoryLogSink>();
            if (_sink != null)
                _sink.NewLogEntry += OnNewLogEntry;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            DisplayedEntriesView = CollectionViewSource.GetDefaultView(_displayedEntries) as ListCollectionView;
            if (DisplayedEntriesView != null)
                DisplayedEntriesView.Filter = new Predicate<object>(OnFilterDisplay);
        }

        private bool OnFilterDisplay(object obj)
        {
            if (obj is LogEntry entry)
            {
                var hasKeyword = true;
                var hasLevel = true;

                if (_filterKeyword.IsEmpty())
                    hasKeyword = true;
                else
                    hasKeyword = entry.Message.Contains(_filterKeyword, StringComparison.OrdinalIgnoreCase);

                if (_selectedLogLevel == LanguageHelper.Manager["All"])
                    hasLevel = true;
                else if (Enum.TryParse(_selectedLogLevel, out LogEventLevel logLevel))
                    hasLevel = entry.LevelEnum == logLevel;

                return hasKeyword && hasLevel;
            }
            return false;
        }

        private void OnNewLogEntry()
        {
            RefreshEntries();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            RefreshEntries();
        }

        private void RefreshEntries()
        {
            var filtered = _sink.GetFiltered(_minLevel, _filterKeyword).ToList();

            // 计算差异，只更新新增条目
            var existingIds = new HashSet<string>(
                DisplayedEntries.Select(e => $"{e.Timestamp}_{e.Message}"));

            foreach (var entry in filtered.Where(e => !existingIds.Contains($"{e.Timestamp}_{e.Message}")))
            {
                DisplayedEntries.Add(entry);
            }

            // 限制显示条目数
            while (DisplayedEntries.Count > 1000)
                DisplayedEntries.RemoveAt(0);

            StatusText = string.Format(LanguageHelper.Manager["StatusInfo"], DisplayedEntries.Count, DisplayedEntriesView.Count);
        }

        private void ExecuteCopyCommand()
        {
            if (_selectedLog != null)
            {
                var text = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} {1} {2}",
                    _selectedLog.Timestamp,
                    _selectedLog.Level,
                    _selectedLog.Message);

                SetText(text);
            }
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void ExecuteClearLogsCommand()
        {
            _sink.Clear();
            DisplayedEntries.Clear();
        }

        private void ExecuteExportLogsCommand()
        {
            var openDirDialog = new OpenFolderDialog
            {
                Title = LanguageHelper.Manager["LogDirSelect"],
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };

            if (openDirDialog.ShowDialog() == true)
            {
                var dir = openDirDialog.FolderName;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var fileName = $"log_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(dir, fileName);
                var entries = _sink.GetRecent(recentCount);
                var lines = entries.Select(e =>
                    $"[{e.Timestamp:HH:mm:ss.fff}] [{e.Level}] {(e.SourceContext != null ? $"[{e.SourceContext}]" : "")} {e.Message}" +
                    (e.Exception != null ? $"\n{e.Exception}" : ""));

                File.WriteAllLines(filePath, lines);
                StatusText = string.Format(LanguageHelper.Manager["InformationExported"], entries.Count(), filePath);
            }


        }

        /// <summary>
        /// 设置日志级别过滤
        /// </summary>
        public void SetLevelFilter(LogEventLevel? level)
        {
            _minLevel = level;
            RefreshEntries();
        }

        /// <summary>
        /// 向剪贴板中添加文本
        /// </summary>
        /// <param name="text">文本</param>
        public void SetText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                SetText(text);
                return;
            }
            EmptyClipboard();
            SetClipboardData(13, Marshal.StringToHGlobalUni(text));
            CloseClipboard();
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }
    }
}

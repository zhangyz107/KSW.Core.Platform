using KSW.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KSW.UI.WPF.Views
{
    /// <summary>
    /// LogViewerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerDialog : IView
    {
        public LogViewerDialog()
        {
            InitializeComponent();
        }

        private void LevelFilter_Changed(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}

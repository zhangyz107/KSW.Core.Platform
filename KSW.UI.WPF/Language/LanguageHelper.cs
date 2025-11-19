using KSW.Localization;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace KSW.UI.WPF.Language
{
    /// <summary>
    /// 多语言管理
    /// </summary>
    public class LanguageHelper
    {
        /// <summary>
        /// 多语言资源命名空间
        /// </summary>
        public string ResourceName
        {
            get => "KSW.UI.WPF.Properties.Resources";
        }
        private readonly ILanguageManager _manager;

        private static readonly Lazy<LanguageHelper> _lazy = new Lazy<LanguageHelper>(() => new LanguageHelper());

        public static ILanguageManager Manager { get { return _lazy?.Value?._manager; } }

        public LanguageHelper()
        {
            _manager = LanguageManagerFactory.CreateManager(ResourceName, GetType().Assembly);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Localization
{
    public class LanguageManagerBase : ILanguageManager
    {
        private readonly ResourceManager _resourceManager;
        private static CultureInfo _currentCultureInfo;
        #region Properties
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        public LanguageManagerBase(string resourceName, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }
            //assembly ?? = Assembly.GetAssembly(GetType());
            _resourceManager = new ResourceManager(resourceName, assembly);
            //CultureManager.CurrentCultureChanged += CultureManager_CurrentCultureChanged;
        }

        public string this[string name]
        {
            get
            {
                if (name == null)
                    throw new NotImplementedException();
                var cultureInfo = _currentCultureInfo ?? CultureManager.CurrentCulture;
                var result = _resourceManager.GetString(name, cultureInfo);
                return result;
            }
        }

        /// <summary>
        /// 切换使用语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            _currentCultureInfo=  cultureInfo;
            RefreshLanguage();
        }

        private void CultureManager_CurrentCultureChanged(object? sender, CultureInfo e)
        {
            RefreshLanguage();
        }

        public void RefreshLanguage()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));
        }
    }
}

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

        #region Properties
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        public LanguageManagerBase(string resourceName, Assembly assembly)
        {
            _resourceManager = new ResourceManager(resourceName, assembly);
            CultureManager.CurrentCultureChanged += CultureManager_CurrentCultureChanged;
        }

        public string this[string name]
        {
            get
            {
                if (name == null)
                    throw new NotImplementedException();
                return _resourceManager.GetString(name);
            }
        }

        /// <summary>
        /// 切换使用语言
        /// </summary>
        /// <param name="cultureInfo"></param>
        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureManager.CurrentCulture = cultureInfo;
        }

        private void CultureManager_CurrentCultureChanged(object? sender, CultureInfo e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("item[]"));
        }
    }
}

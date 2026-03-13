using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSW.Localization
{
    public class LanguageManagerFactory
    {
        private static Dictionary<string, ILanguageManager> _languageManagerStore = new Dictionary<string, ILanguageManager>();

        static LanguageManagerFactory()
        {

        }

        public static ILanguageManager CreateManager(string resourceName, Assembly assembly)
        {
            if (!_languageManagerStore.ContainsKey(resourceName))
            {
                var manager = new LanguageManagerBase(resourceName, assembly);
                _languageManagerStore.Add(resourceName, manager);
                return manager;
            }
            else
            {
                var manager = new LanguageManagerBase(resourceName, assembly);
                _languageManagerStore[resourceName] = manager;
                return manager;
            }
        }

        public static ILanguageManager GetManager(string resourceName)
        {
            if (_languageManagerStore.ContainsKey(resourceName))
            {
                return _languageManagerStore[resourceName];
            }
            else
            {
                return null;
            }
        }

        public static void ChangeLanguage(CultureInfo cultureInfo)
        {
            foreach (var managerKeyValue in _languageManagerStore)
            {
                var manager = managerKeyValue.Value;
                manager?.ChangeLanguage(cultureInfo);
            }
        }
    }
}

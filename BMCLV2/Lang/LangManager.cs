using System;
using System.Windows;
using System.Collections;

namespace BMCLV2.Lang
{
    static class LangManager
    {
        static private readonly Hashtable Language = new Hashtable();
        static private readonly ResourceDictionary DefaultLanguage = LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
        static public void Add(string languageName,string languageUrl)
        {
            if (Language.ContainsKey(languageName))
            {
                Language[languageName] = new LangType(languageName,languageUrl);
                return;
            }
            Language.Add(languageName, new LangType(languageName, languageUrl));

        }
        static public string GetLangFromResource(string key)
        {
            if (Application.Current.Resources.Contains(key))
                return Application.Current.Resources[key] as string;
            if (DefaultLanguage.Contains(key))
                return DefaultLanguage[key] as string;
            return key;
        }

        static public ResourceDictionary LoadLangFromResource(string path)
        {
            var lang = new ResourceDictionary();
            lang.Source = new Uri(path);
            return lang;
        }
        static public void UseLanguage(string languageName)
        {
            if (!Language.ContainsKey(languageName))
            {
                Application.Current.Resources = DefaultLanguage;
                return;
            }
            var langType = Language[languageName] as LangType;
            if (langType != null)
                Application.Current.Resources = langType.Language;
        }
    }
}

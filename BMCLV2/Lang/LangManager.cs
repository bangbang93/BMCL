using System;
using System.Collections.Generic;
using System.Windows;
using System.Collections;

namespace BMCLV2.Lang
{
    static class LangManager
    {
        private static readonly Dictionary<string, LangType> Languages = new Dictionary<string, LangType>();
        private static readonly Dictionary<string, string> DisplayToName = new Dictionary<string, string>(); 
        static private readonly ResourceDictionary DefaultLanguage = LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
        static public void Add(string languageName,string languageUrl)
        {
            if (Languages.ContainsKey(languageName))
            {
                Languages[languageName] = new LangType(languageName,languageUrl);
                return;
            }
            Languages.Add(languageName, new LangType(languageName, languageUrl));

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
            if (!Languages.ContainsKey(languageName))
            {
                Application.Current.Resources = DefaultLanguage;
                return;
            }
            var langType = Languages[languageName];
            if (langType != null)
                Application.Current.Resources = langType.Language;
        }

        public static string[] ListLanuage()
        {
            var langs = new string[Languages.Count];
            var i = 0;
            foreach (var lang in Languages)
            {
                langs[i] = (string)lang.Value.Language["DisplayName"];
                i++;
            }
            return langs;
        }
    }
}

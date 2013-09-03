using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections;

namespace BMCLV2.Lang
{
    static class LangManager
    {
        static private Hashtable Language = new Hashtable();
        static private ResourceDictionary DefaultLanguage = LoadLangFromResource("pack://application:,,,/Lang/zh-cn.xaml");
        static public void Add(string LanguageName,string LanguageUrl)
        {
            int i = 0;
            if (Language.ContainsKey(LanguageName))
            {
                Language[LanguageName] = new LangType(LanguageName,LanguageUrl);
                return;
            }
            Language.Add(LanguageName, new LangType(LanguageName, LanguageUrl));

        }
        static public string GetLangFromResource(string key)
        {
            if (Application.Current.Resources.Contains(key))
                return Application.Current.Resources[key] as string;
            else
                if (DefaultLanguage.Contains(key))
                    return DefaultLanguage[key] as string;
                else
                    return key;
        }
        static public ResourceDictionary LoadLangFromResource(string path)
        {
            var Lang = new ResourceDictionary();
            Lang.Source = new Uri(path);
            return Lang;
        }
        static public void UseLanguage(string LanguageName)
        {
            Application.Current.Resources = (Language[LanguageName] as LangType).Language;
        }
    }
}

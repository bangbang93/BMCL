using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace BMCLV2.I18N
{
    static class LangManager
    {
        private static readonly Dictionary<string, LangType> Languages = new Dictionary<string, LangType>();
        static private readonly ResourceDictionary DefaultLanguage = LoadLangFromResource("pack://application:,,,/I18N/zh-cn.xaml");
        private static readonly string LocaleDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Lang";
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
            var lang = new ResourceDictionary {Source = new Uri(path)};
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

        public static void LoadLanguage()
        {
            ResourceDictionary lang = LangManager.LoadLangFromResource("pack://application:,,,/I18N/zh-cn.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/I18N/zh-cn.xaml");

            lang = LangManager.LoadLangFromResource("pack://application:,,,/I18N/zh-tw.xaml");
            BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
            LangManager.Add(lang["LangName"] as string, "pack://application:,,,/I18N/zh-tw.xaml");
            if (Directory.Exists(LocaleDirectory))
            {
                foreach (var langFile in Directory.GetFiles(LocaleDirectory, "*.xaml", SearchOption.TopDirectoryOnly))
                {
                    lang = LangManager.LoadLangFromResource(langFile);
                    BmclCore.Language.Add((string)lang["DisplayName"], lang["LangName"]);
                    LangManager.Add(lang["LangName"] as string, langFile);
                }
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Lang");
            }
        }

        public static void ChangeLanguage(string lang)
        {
            LangManager.UseLanguage(lang);
        }
    }
}

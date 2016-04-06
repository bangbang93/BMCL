using System;
using System.Windows;

namespace BMCLV2.I18N
{
    class LangType
    {
        public string Name;
        public ResourceDictionary Language = new ResourceDictionary();
        public LangType(string Name,string Path)
        {
            this.Name = Name;
            Language.Source = new Uri(Path);
        }
    }
}

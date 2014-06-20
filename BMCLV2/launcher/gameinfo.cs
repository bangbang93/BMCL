using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.IO;

using BMCLV2.libraries;

namespace BMCLV2
{
    [DataContract]
    public class gameinfo : ICloneable
    {
        [DataMember(IsRequired = true)]
        public string id = "";
        [DataMember(IsRequired = false)]
        public string time = "";
        [DataMember(IsRequired = false)]
        public string releaseTime = "";
        [DataMember(IsRequired = false)]
        public string type = "";
        [DataMember(IsRequired = true)]
        public string minecraftArguments = "";
        [DataMember(IsRequired = true)]
        public string mainClass = "";
        [DataMember(IsRequired = true)]
        public libraryies[] libraries = null;
        [DataMember(IsRequired = false)]
        public int minimumLauncherVersion = 0;
        [DataMember(IsRequired = false)]
        public string assets;
        object ICloneable.Clone()
        {
            return this.clone();
        }
        public gameinfo clone()
        {
            return (gameinfo)this.MemberwiseClone();
        }

        static public void Write(gameinfo info,string path)
        {
            DataContractJsonSerializer j = new DataContractJsonSerializer(typeof(gameinfo));
            FileStream fs = new FileStream(path, FileMode.Create);
            j.WriteObject(fs, info);
            fs.Close();
        }

        static public gameinfo Read(string path)
        {
            try
            {
                gameinfo info;
                StreamReader JsonFile = new StreamReader(path);
                DataContractJsonSerializer InfoReader = new DataContractJsonSerializer(typeof(gameinfo));
                info = InfoReader.ReadObject(JsonFile.BaseStream) as gameinfo;
                JsonFile.Close();
                return info;
            }
            catch (SerializationException ex)
            {
                Logger.log(ex);
                try
                {
                    StreamReader JsonFile = new StreamReader(path, Encoding.Default);
                    DataContractJsonSerializer InfoReader = new DataContractJsonSerializer(typeof(gameinfo));
                    gameinfo info = InfoReader.ReadObject(JsonFile.BaseStream) as gameinfo;
                    JsonFile.Close();
                    Logger.log("JSON文件使用", Encoding.Default.EncodingName, "解析成功，将转换为UTF8编码");
                    JsonFile = new StreamReader(path, Encoding.Default);
                    string JsonString = JsonFile.ReadToEnd();
                    JsonFile.Close();
                    StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
                    sw.WriteLine(JsonString);
                    sw.Close();
                    Logger.log("JSON文件转存完毕");
                    return info;
                }
                catch (SerializationException ex1)
                {
                    Logger.log(ex1);
                    return null;
                }
            }
        }

        static public string GetGameInfoJsonPath(string version)
        {
            StringBuilder JsonFilePath = new StringBuilder();
            JsonFilePath.Append(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\");
            JsonFilePath.Append(version);
            JsonFilePath.Append(@"\");
            JsonFilePath.Append(version);
            JsonFilePath.Append(".json");
            if (!File.Exists(JsonFilePath.ToString()))
            {
                DirectoryInfo mcpath = new DirectoryInfo(System.IO.Path.GetDirectoryName(JsonFilePath.ToString()));
                bool find = false;
                foreach (FileInfo js in mcpath.GetFiles())
                {
                    if (js.FullName.EndsWith(".json"))
                    {
                        if (Read(js.FullName) != null)
                        {
                            JsonFilePath = new StringBuilder(js.FullName);
                            find = true;
                        }
                    }
                }
                if (!find)
                {
                    return null;
                }
                else
                {
                    return JsonFilePath.ToString();
                }
            }
            else
            {
                return JsonFilePath.ToString();
            }

        }
    }
}

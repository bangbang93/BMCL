using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Threading;
using BMCLV2.I18N;
using BMCLV2.util;

namespace BMCLV2.Versions
{
    static class VersionHelper
    {
        public delegate void ImportProgressChangeEventHandler(string status);
        public static event ImportProgressChangeEventHandler ImportProgressChangeEvent;

        public delegate void ImportFinishEventHandler();
        public static event ImportFinishEventHandler ImportFinish;

        private static void OnImportFinish()
        {
            ImportFinishEventHandler handler = ImportFinish;
            if (handler != null) BmclCore.Invoke(handler);
        }


        private static void OnImportProgressChangeEvent(string status)
        {
            ImportProgressChangeEventHandler handler = ImportProgressChangeEvent;
            if (handler != null) BmclCore.Invoke(handler, new[] {status});
        }

        public static void ImportOldMc(string importName, string importFrom, Delegate callback = null)
        {
            var thread = new Thread(() =>
            {
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportMain"));
                Directory.CreateDirectory(".minecraft\\versions\\" + importName);
                File.Copy(importFrom + "\\bin\\minecraft.jar",
                    ".minecraft\\versions\\" + importName + "\\" + importName + ".jar");
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportCreateJson"));
                var info = new gameinfo {id = importName};
                string timezone = DateTimeOffset.Now.Offset.ToString();
                if (timezone[0] != '-')
                {
                    timezone = "+" + timezone;
                }
                info.time = DateTime.Now.GetDateTimeFormats('s')[0] + timezone;
                info.releaseTime = DateTime.Now.GetDateTimeFormats('s')[0] + timezone;
                info.type = "Port By BMCL";
                info.minecraftArguments = "${auth_player_name}";
                info.mainClass = "net.minecraft.client.Minecraft";
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportSolveNative"));
                var libs = new ArrayList();
                var bin = new DirectoryInfo(importFrom + "\\bin");
                foreach (FileInfo file in bin.GetFiles("*.jar"))
                {
                    var libfile = new libraries.libraryies();
                    if (file.Name == "minecraft.jar")
                        continue;
                    if (
                        !Directory.Exists(".minecraft\\libraries\\" + importName + "\\" +
                                          file.Name.Substring(0, file.Name.Length - 4) + "\\BMCL\\"))
                    {
                        Directory.CreateDirectory(".minecraft\\libraries\\" + importName + "\\" +
                                                  file.Name.Substring(0, file.Name.Length - 4) + "\\BMCL\\");
                    }
                    File.Copy(file.FullName,
                        ".minecraft\\libraries\\" + importName + "\\" + file.Name.Substring(0, file.Name.Length - 4) +
                        "\\BMCL\\" + file.Name.Substring(0, file.Name.Length - 4) + "-BMCL.jar");
                    libfile.name = importName + ":" + file.Name.Substring(0, file.Name.Length - 4) + ":BMCL";
                    libs.Add(libfile);
                }
                FileHelper.CreateDirectoryIfNotExist(".minecraft\\libraries\\" + importName + "\\BMCL\\");
                var nativejar =
                    new ZipArchive(
                        new FileStream(
                            ".minecraft\\libraries\\" + importName + "\\native\\BMCL\\native-BMCL-natives-windows.jar",
                            FileMode.OpenOrCreate));
                var nativeInfo = new DirectoryInfo(importFrom + "\\bin\\natives").GetFiles("\\.dll$");
                foreach (var fileInfo in nativeInfo)
                {
                    nativejar.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                }
                var nativefile = new libraries.libraryies {name = importName + ":native:BMCL"};
                var nativeos = new libraries.OS {windows = "natives-windows"};
                nativefile.natives = nativeos;
                nativefile.extract = new libraries.extract();
                libs.Add(nativefile);
                info.libraries = (libraries.libraryies[]) libs.ToArray(typeof (libraries.libraryies));
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportWriteJson"));
                var wcfg = new FileStream(".minecraft\\versions\\" + importName + "\\" + importName + ".json",
                    FileMode.Create);
                var infojson = new DataContractJsonSerializer(typeof (gameinfo));
                infojson.WriteObject(wcfg, info);
                wcfg.Close();
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportSolveLib"));
                if (Directory.Exists(importFrom + "\\lib"))
                {
                    if (!Directory.Exists(".minecraft\\lib"))
                    {
                        Directory.CreateDirectory(".minecraft\\lib");
                    }
                    foreach (
                        string libfile in Directory.GetFiles(importFrom + "\\lib", "*", SearchOption.AllDirectories))
                    {
                        if (!File.Exists(".minecraft\\lib\\" + System.IO.Path.GetFileName(libfile)))
                        {
                            File.Copy(libfile, ".minecraft\\lib\\" + System.IO.Path.GetFileName(libfile));
                        }
                    }
                }
                OnImportProgressChangeEvent(LangManager.GetLangFromResource("ImportSolveMod"));
                if (Directory.Exists(importFrom + "\\mods"))
                    util.FileHelper.CopyDir(importFrom + "\\mods", ".minecraft\\versions\\" + importName + "\\mods");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + importName + "\\mods");
                if (Directory.Exists(importFrom + "\\coremods"))
                    util.FileHelper.CopyDir(importFrom + "\\coremods",
                        ".minecraft\\versions\\" + importName + "\\coremods");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + importName + "\\coremods");
                if (Directory.Exists(importFrom + "\\config"))
                    util.FileHelper.CopyDir(importFrom + "\\config", ".minecraft\\versions\\" + importName + "\\config");
                else
                    Directory.CreateDirectory(".minecraft\\versions\\" + importName + "\\configmods");
                OnImportFinish();
                if (callback != null)
                {
                    BmclCore.Invoke(callback);
                }
            });
            thread.Start();
        }
    }
}

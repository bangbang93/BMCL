using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace BMCLV2.Texturepack
{
    public class TexturePackEntity
    {
        ZipInputStream Input;
        public TextureInfo textureInfo = new TextureInfo();
        public MemoryStream GuiBackground = new MemoryStream();
        private MemoryStream GuiWidgets = new MemoryStream();
        public System.Windows.Controls.Image GuiButton = new System.Windows.Controls.Image();
        public TexturePackEntity(string FileName)
        {
            Input = new ZipInputStream(File.OpenRead(FileName));
            ZipEntry entity;
            byte[] data = new byte[2048];
            int size;
            while ((entity = Input.GetNextEntry()) != null)
            {
                switch (entity.Name)
                {
                    case "pack.mcmeta":
                        JavaScriptSerializer PackSerializer = new JavaScriptSerializer();
                        MemoryStream ms = new MemoryStream();
                        while (true)
                        {
                            size = Input.Read(data, 0, data.Length);
                            if (size> 0)
                            {
                                ms.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        ms.Position = 0;
                        StreamReader sr = new StreamReader(ms);
                        var tmp = PackSerializer.Deserialize<TextureInfo>(sr.ReadToEnd());
                        textureInfo.pack = tmp.pack;
                        break;
                    case "pack.png":
                        while (true)
                        {
                            size = Input.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                textureInfo.Logo.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    case @"assets/minecraft/textures/gui/options_background.png":
                        while (true)
                        {
                            size = Input.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                GuiBackground.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    case @"assets/minecraft/textures/gui/widgets.png":
                        while (true)
                        {
                            size = Input.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                GuiWidgets.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    default:
                        continue;
                }
            }
            if (GuiWidgets.Length != 0)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = this.GuiBackground;
                bitmap.EndInit();
                GuiButton.Source = bitmap;
                RectangleGeometry clip = new RectangleGeometry(new System.Windows.Rect(0, bitmap.Height * 0.2578125, bitmap.Width * 0.78125, bitmap.Height * 0.0703125));
                GuiButton.Clip = clip;
            }
        }
    }
}

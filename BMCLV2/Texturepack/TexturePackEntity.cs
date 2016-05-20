using System.IO;
using System.IO.Compression;
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using BMCLV2.JsonClass;

namespace BMCLV2.Texturepack
{
    public class TexturePackEntity
    {
        public TextureInfo TextureInfo = new TextureInfo();
        public MemoryStream GuiBackground = new MemoryStream();
        private readonly MemoryStream _guiWidgets = new MemoryStream();
        public System.Windows.Controls.Image GuiButton = new System.Windows.Controls.Image();
        public TexturePackEntity(string fileName)
        {
            var texture = new ZipArchive(new FileStream(fileName, FileMode.Open));
            foreach (var zipArchiveEntry in texture.Entries)
            {
                switch (zipArchiveEntry.Name)
                {
                    case "pack.mcmeta":
                        var tmp = new JSON<TextureInfo>().Parse(zipArchiveEntry.Open());
                        TextureInfo.pack = tmp.pack;
                        break;
                    case "pack.png":
                        zipArchiveEntry.Open().CopyTo(TextureInfo.Logo);
                        break;
                    case @"assets/minecraft/textures/gui/options_background.png":
                        zipArchiveEntry.Open().CopyTo(GuiBackground);
                        break;
                    case @"assets/minecraft/textures/gui/widgets.png":
                        zipArchiveEntry.Open().CopyTo(_guiWidgets);
                        break;
                    default:
                        continue;
                }
            }
            if (_guiWidgets.Length != 0)
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

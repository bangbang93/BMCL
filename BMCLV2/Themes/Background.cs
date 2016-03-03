using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BMCLV2.Themes
{
    public class Background
    {
        private readonly List<string> _files = new List<string>();

        public Background()
        {
            Reload();
        }

        public string GetRandomFile()
        {
            if (_files.Count > 0)
            {
                var rand = new Random();
                return _files[rand.Next(_files.Count)];
            }
            else
            {
                return null;
            }
        }

        public ImageBrush GetRadnomImageBrush()
        {
            var file = GetRandomFile();
            if (file != null)
            {
                return new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(file)),
                    Stretch = Stretch.Fill
                };
            }
            return null;
        }

        public void Reload()
        {
            _files.Clear();
            _files.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.jpg", SearchOption.AllDirectories));
            _files.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.png", SearchOption.AllDirectories));
            _files.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\bg", "*.bmp", SearchOption.AllDirectories));
        }
    }
}
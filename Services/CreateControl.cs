using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Flarial.Services
{
    internal class CreateControl
    {


        /// <summary>
        /// Function to load images (To avoid file locking issues)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapImage? LoadImage(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return null;

                var bitmap = new BitmapImage();
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return bitmap;
            }
            catch
            {
                return null; // prevent crash
            }
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickLauncher.Converter
{
    public class ByteToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] imageBytes = (byte[])value;
            return ConvertByteToImage(imageBytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource imageSource = (ImageSource)value;
            return ImageSourceToBytes(imageSource);
        }

        public static byte[] ImageSourceToBytes(ImageSource imageSource)
        {
            byte[] bytes = null;
            BitmapEncoder encoder = new PngBitmapEncoder();

            if (imageSource is BitmapSource bitmapSource)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using var stream = new MemoryStream();
                encoder.Save(stream);
                bytes = stream.ToArray();
            }

            return bytes;
        }

        public static ImageSource ConvertByteToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageBytes))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}

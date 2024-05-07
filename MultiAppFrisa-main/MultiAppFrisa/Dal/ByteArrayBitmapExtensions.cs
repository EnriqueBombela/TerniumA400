using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PredictorV2.Dal
{
    internal static class ByteArrayBitmapExtensions
    {
        public static async Task<byte[]> AsByteArray(this StorageFile file)
        {
            byte[] pixels = null;
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            using (var inputstr = fileStream.GetInputStreamAt(0))
            {
                using (DataReader reader = new Windows.Storage.Streams.DataReader(inputstr))
                {
                    await reader.LoadAsync((uint)fileStream.Size);
                    pixels = new byte[fileStream.Size];
                    reader.ReadBytes(pixels);
                }
            }
            fileStream.Dispose();
            return pixels;
        }

        //public static async Task<byte[]> AsByteArray(this BitmapImage image)
        //{
        //  I did not find a decent implementation yet ...
        //}

        public static byte[] AsByteArray(this WriteableBitmap bitmap)
        {
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static BitmapImage AsBitmapImage(this byte[] byteArray)
        {
            if (byteArray != null)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    stream.WriteAsync(byteArray.AsBuffer()).GetResults(); // I made this one synchronous on the UI thread; this is not a best practice.
                    var image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    return image;
                }
            }

            return null;
        }

        private static async Task<BitmapImage> AsBitmapImage(this StorageFile file)
        {
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(stream);
            return bitmapImage;
        }
    }
}

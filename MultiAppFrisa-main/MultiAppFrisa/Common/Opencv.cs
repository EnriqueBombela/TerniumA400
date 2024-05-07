using AAM_Lepton.Interfaces;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using PredictorV2.Pages;
using MultiAppFrisa.Common;
using MultiAppFrisa.Models;

namespace MultiAppFrisa.Common
{
    public class OpenCV
    {
        private static ThreadPoolTimer OpenCVTimer;

        private static BackgroundSubtractorMOG2 mog2;

        // codigo comentado por Irving 
        public static SoftwareBitmap softwareBitmap2 = null;
        public static bool imageReady = false;
        private static IMemoryBufferByteAccess reference;
        private static object buffer;
        private static InputOutputArray mOutput;
        private static OutputArray mInput;
        private static InputArray gray;

        public static bool pauseSql = false; 

        public static void Init()
        {
            mog2 = BackgroundSubtractorMOG2.Create();
        }

        public static void Dispose()
        {
            mog2.Dispose();
        }

        public static void RunProcess()
        {
            try
            {
                if (OpenCVTimer == null)
                {
                    OpenCVTimer = ThreadPoolTimer.CreateTimer(OpenCVProcess, TimeSpan.FromMilliseconds(100));
                }
            }
            catch
            {

            }
        }

        private static async void OpenCVProcess(ThreadPoolTimer timer)
        {
            try
            {
                while (true)
                {
                    if (TimeZoom.imageSqlReady)
                    {
                        pauseSql = true;
                        SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 120, 160, BitmapAlphaMode.Straight);
                        //SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Rgba16, Lepton.totalColumns, Lepton.totalRows, BitmapAlphaMode.Straight);

                        softwareBitmap = TimeZoom.sf; 
                        using (var buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.ReadWrite))
                        {
                            using (var reference = buffer.CreateReference())
                            {
                                unsafe
                                {

                                    ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out _);
                                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);


                                }
                            }
                        }

                        //softwareBitmap = await ColorMap(softwareBitmap);
                        //DrawRectangle(softwareBitmap, 0, 0, 80, 50);

                        DrawRectangle(softwareBitmap, 10, 10, 10, 10);
                        DrawRectangle(softwareBitmap, 50, 50, 10, 10);
                        DrawRectangle(softwareBitmap, 90, 90, 10, 10);
                        softwareBitmap2 = softwareBitmap;
                        imageReady = true;
                        // imageSqlReady = true;
                        //await SaveSoftwareBitmapToFile(softwareBitmap);
                        pauseSql= false;

                    }

                    



                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Ocurrió un error: " + ex.Message);
                await dialog.ShowAsync();
            }
            finally
            {
                await Task.Delay(1000);
                OpenCVTimer = ThreadPoolTimer.CreateTimer(OpenCVProcess, TimeSpan.FromMilliseconds(100));
            }
        }

        private static async System.Threading.Tasks.Task<bool> SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap)
        {
            StorageFile outputFile = await KnownFolders.PicturesLibrary.CreateFileAsync("Spotmeter.png", CreationCollisionOption.GenerateUniqueName);
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.IsThumbnailGenerated = false;

                try
                {
                    await encoder.FlushAsync();
                }
                catch
                {
                }
            }
            return true;
        }

        public static async Task<SoftwareBitmap> ColorMap(SoftwareBitmap input, ColormapTypes color)
        {
            SoftwareBitmap input2 = Lepton.originalImage;
            using Mat mInput = SoftwareBitmap2Mat(input);
            using Mat gray = mInput.CvtColor(ColorConversionCodes.BGR2GRAY);
            Cv2.ApplyColorMap(gray, mInput, color);

            return await MatToSoftwareBitmap(mInput);
        }

        public static void DrawRectangle(SoftwareBitmap output, double x, double y, double width, double height)
        {
            using Mat mOutput = SoftwareBitmap2Mat(output);
            Cv2.Rectangle(mOutput, new Rect(new OpenCvSharp.Point(x, y), new OpenCvSharp.Size(width, height)), new Scalar(0, 0, 0), 1);
        }

        public static unsafe Mat SoftwareBitmap2Mat(SoftwareBitmap softwareBitmap)
        {
            using BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write);
            using var reference = buffer.CreateReference();
            ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out var capacity);
            Mat outputMat = new Mat(softwareBitmap.PixelHeight, softwareBitmap.PixelWidth, MatType.CV_8UC4, (IntPtr)dataInBytes);
            // Mat outputMat = new Mat(softwareBitmap.PixelHeight, softwareBitmap.PixelWidth, MatType.CV_16UC4, (IntPtr)dataInBytes);

            return outputMat;

        }

        public static async Task<SoftwareBitmap> MatToSoftwareBitmap(Mat mat)
        {
            byte[] decoded = mat.ToBytes();
            var imageMemoryStream = new InMemoryRandomAccessStream();
            await imageMemoryStream.WriteAsync(decoded.AsBuffer());
            imageMemoryStream.Seek(0);
            BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(imageMemoryStream);

            return await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            // return await bitmapDecoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Rgba16, BitmapAlphaMode.Premultiplied);
        }
    }
}

using System;
using System.Threading.Tasks;
using Windows.System.Threading;
using MultiAppFrisa.Models;
using Windows.Graphics.Imaging;
using OpenCvSharp;
using System.Drawing;
using PredictorV2.Pages;
using System.Linq;
using static MultiAppFrisa.Models.Lepton;
using Newtonsoft.Json.Linq;

namespace MultiAppFrisa.Common
{
    public class ProcesingImage
    {
        private static ThreadPoolTimer ImageTimer;
        private static float[,] matrizTemp = new float[240, 320]; //modificamos para el valor del array de temperaturas

        public static void RunProcess()
        {
            try
            {
                ImageTimer ??= ThreadPoolTimer.CreateTimer(SQLProcess, TimeSpan.FromMilliseconds(1000));
            }
            catch
            {

            }
        }

        private static async void SQLProcess(ThreadPoolTimer timer)
        {
            try
            {
                while (true)
                {
                    if (Lepton.isNewData)
                    {
                        Lepton.isNewData = false;

                        int iterador = 0;
                        var array = Lepton.temperatures;

                        for (var iy = 0; iy < 240; iy++) //modificamos para valores de y 
                        {
                            for (var ix = 0; ix < 320; ix++) //modificamos para valores de x
                            {
                                float dato = array[iterador];
                                matrizTemp[iy, ix] = dato; //los datos ya vienen convertidos  entonces no hace falta hacerlo de nuevo
                                //matrizTemp[iy,ix] = (dato /100) - 273.15f ;

                                iterador++;
                            }
                        }
                        
                        

                        if (TimeZoom.opencvEneable)
                        {
                            switch (TimeZoom.valueOpencv)
                            {
                                case "Original":
                                    Lepton.coloredImage = Lepton.originalImage;
                                    break;
                                case "AUTUMN":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Autumn);
                                    break;
                                case "BONE":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Bone);
                                    break;
                                case "JET":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Jet);
                                    break;
                                case "WINTER":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Winter);
                                    break;
                                case "RAINBOW":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Rainbow);
                                    break;
                                case "OCEAN":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Ocean);
                                    break;
                                case "SUMMER":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Summer);
                                    break;
                                case "SPRING":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Spring);
                                    break;
                                case "COOL":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Cool);
                                    break;
                                case "HSV":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Hsv);
                                    break;
                                case "PINK":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Pink);
                                    break;
                                case "HOT":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Hot);
                                    break;
                                case "PARULA":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Parula);
                                    break;
                                case "MAGMA":
                                    Lepton.coloredImage = await OpenCV.ColorMap(Lepton.originalImage, ColormapTypes.Magma);
                                    break;
                                default:
                                    /*
                   
                                     */
                                    break;
                            }
                        }

                        if (TimeZoom.enebleDarwRectangle)
                        {
                            TimeZoom.enebleDarwRectangle = false;
                            int x = Convert.ToInt32(TimeZoom.X_OriginalImage);
                            int y = Convert.ToInt32(TimeZoom.y_OriginalImage);
                            int width = Convert.ToInt32(TimeZoom.widthOriginalImage);
                            int height = Convert.ToInt32(TimeZoom.heightOriginalImage);

                            // si height width son de dimenciones de 0 significa que se seleccion solo 1 pixel
                            // entonces se le asigna el valor de 1 para que pueda solamente seleccionar 1 pixel

                            if (width < 1 || height < 1)
                            {
                                if (width < 1 && height < 1)
                                {
                                    width = 1;
                                    height = 1;
                                }
                                else if (height < 1)
                                {
                                    height = 1;
                                }
                                else
                                {
                                    
                                    width = 1;
                                }
                                Lepton.scrs.Add(new Lepton.Scr { rect = new Rectangle(x, y, width, height) });
                            }
                            else 
                            {
                                Lepton.scrs.Add(new Lepton.Scr { rect = new Rectangle(x, y, width, height) });
                            }
                            


                        }

                        foreach (var scr in Lepton.scrs.ToList())
                        {
                            scr.averageTemp = getPromedio(scr.rect.X, scr.rect.Y, scr.rect.Width, scr.rect.Height);
                            //OpenCV.DrawRectangle(Lepton.coloredImage, scr.rect.X, scr.rect.Y, scr.rect.Width, scr.rect.Height);
                        }




                        Lepton.isNewDataProcessed = true;
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                await Task.Delay(1000);
                ImageTimer = ThreadPoolTimer.CreateTimer(SQLProcess, TimeSpan.FromMilliseconds(1000));
            }
        }


        private static float getPromedio(int x,int y,int ancho, int alto)
        {
            float resultado = 0;
            int rangoX = x + ancho;
            int rangoY = y + alto;
            float suma = 0;
            int numeroDatos = 0; 
            try
            {

                for (int iy = y; iy < rangoY; iy++)
                {

                    for (int ix = x; ix < rangoX; ix++)
                    {

                        suma += matrizTemp[iy, ix];
                        numeroDatos++; 

                    }

                }

                resultado = suma / numeroDatos; 

            }
            catch (Exception ex)
            {
            }
            return resultado;

        }
    }
}

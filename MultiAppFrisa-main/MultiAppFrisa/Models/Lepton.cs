using System;
using System.Collections.Generic;
using System.Drawing;
using Windows.Graphics.Imaging;

namespace MultiAppFrisa.Models
{
    internal class Lepton
    {
        public static int milisecondsToUpdateData = 1;
        public static int milisecondsToUpdateGraph = 100;
        public static DateTime lastDataUpdate;
        public static DateTime lastGraphUpdate;
        public static List<float> temperatures = new List<float>();
        public static List<Scr> scrs = new List<Scr>();
        public static SoftwareBitmap originalImage;
        public static SoftwareBitmap coloredImage;
        public static bool isNewData;
        public static bool isNewDataProcessed;
        public static DateTime LastTemperatureDataUpdate { get; set; }

        public class Scr
        {
            public float averageTemp = 0;
            public Rectangle rect = new Rectangle();
        }
    }
}

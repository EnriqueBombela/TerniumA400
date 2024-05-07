using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.AppService;
using Windows.UI;
using PredictorV2.Common;
using System.Collections.Generic;
using System.Linq;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Net.Http.Headers;
using Windows.Storage.Streams;
using Windows.Media;
using static PredictorV2.Common.MainContainer;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Text;
using System.Globalization;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MultiAppFrisa.Common;
using MultiAppFrisa.Models;
using Windows.UI.Core;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Input.Inking;
using Windows.Foundation;
using static MultiAppFrisa.Models.Lepton;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PredictorV2.Pages
{
    public sealed partial class TimeZoom : Page
    {
        DispatcherTimer UITimer = null;
        bool IsTimeRun = false;
        double XScaleFactor = 1;
        double ScaleXAxis = 1;
        double ScaleYAxis = 1;
        bool[] IsChEnabled = new bool[8];

        // Variables Botones Graficas
        bool btnp9 = false;
        bool btnp10 = false;
        bool btnp11 = false;
        bool btnp12 = false;
        bool btnp13 = false;

        // variables para graficar los pixeles de las temperaturas de la camara
        int Scr1 = 0;
        int Scr2 = 0;
        int Scr3 = 0;
        
        int pixelScr1 = 0;
        int pixelScr2 = 0;
        int pixelScr3 = 0;

        int ejex = 0; 

        // porcentajes 
        int t1Pocentaje = 0;
        int t2Pocentaje = 0;
        int t3Pocentaje = 0;
        //
        bool GarficaLeton = false;
        //
        DateTime consulta1;
        DateTime consulta2;
        bool consulta = true; 
        bool paint = false;

        int iterador = 0;

        private readonly FrameRenderer previewRenderer;
        private SoftwareBitmap softwareBitmap = null;
        public static bool imageSqlReady = false;
        public static bool jsonReady = false;
        public static string tempJson;
        public static SoftwareBitmap sf;
        public static bool readSql = false;

        int count = 0;

        // variables Ink Analyzer
        InkPresenter inkPresenter;
        InkAnalyzer inkAnalyzer;
        DispatcherTimer dispatcherTimer;
        public static bool enebleDarwRectangle = false;

        public static double X_OriginalImage;
        public static double y_OriginalImage;
        public static double widthOriginalImage;
        public static double heightOriginalImage;
        public byte contador = 0;
        public static bool opencvEneable = false;
        public static string valueOpencv = ""; 

        public TimeZoom()
        {
            this.InitializeComponent();
            inkPresenter = inkCanvas.InkPresenter;
            inkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkPresenter.StrokesErased += InkPresenter_StrokesErased;
            inkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            inkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch;

            inkAnalyzer = new InkAnalyzer();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;

            // We perform analysis when there has been a change to the
            // ink presenter and the user has been idle for 200ms.
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
            clearButton.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
            double inkCanvasWidth = inkCanvas.ActualWidth;
            double inkCanvasHeight = inkCanvas.ActualHeight;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            for (int i = 0; i < IsChEnabled.Length; i++)
            {
                IsChEnabled[i] = true;
            }
            for (int i = 0; i < PacerArray.Length; i++)
            {
                PacerArray[i] = new Pacer();
            }

            CanvasArray[0] = FallaCvs0;
            CanvasArray[1] = FallaCvs1;
            CanvasArray[2] = FallaCvs2;
            CanvasArray[3] = FallaCvs3;
            CanvasArray[4] = FallaCvs4;
            CanvasArray[5] = FallaCvs5;
            ImageArray[0] = FallaImg0;
            ImageArray[1] = FallaImg1;
            ImageArray[2] = FallaImg2;
            ImageArray[3] = FallaImg3;
            ImageArray[4] = FallaImg4;
            ImageArray[5] = FallaImg5;
            DestinationGridArray[0] = GridImg0;
            DestinationGridArray[1] = GridImg1;
            DestinationGridArray[2] = GridImg2;
            DestinationGridArray[3] = GridImg3;
            DestinationGridArray[4] = GridImg4;
            DestinationGridArray[5] = GridImg5;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            IsTimeRun = true;
            UITimer = new DispatcherTimer();
            UITimer.Tick += Process;
            UITimer.Interval = new TimeSpan(0,0,0,0,100);
            UITimer.Start();

            CH9.Background = new SolidColorBrush(Color.FromArgb(255, 3, 169, 244));
            CH10.Background = new SolidColorBrush(Color.FromArgb(255, 58, 254, 0));
            CH11.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 205));

            double inkCanvasWidth = inkCanvas.ActualWidth;
            double inkCanvasHeight = inkCanvas.ActualHeight;

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsTimeRun = false;
            if (UITimer != null) UITimer.Stop();
            base.OnNavigatedFrom(e);
            
        }

        private void ZoomXCallBack(object sender, RangeBaseValueChangedEventArgs e)
        {
         //   XScaleFactor = ZoomX.Value; //samples x decimateFactor should be shorter than source array
        }

        private void CHCallBack9(object sender, RoutedEventArgs e)
        {
            btnp9 = !btnp9;
            if (btnp9)
            {

                CH9.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                Chart9.Visibility = Visibility.Collapsed;
                Chart09.Visibility = Visibility.Collapsed;


            }
            else
            {
                CH9.Background = new SolidColorBrush(Color.FromArgb(255, 3, 169, 244));
                Chart9.Visibility = Visibility.Visible;
                Chart09.Visibility = Visibility.Visible;
            }
        }

        private void CHCallBack10(object sender, RoutedEventArgs e)
        {
            btnp10 = !btnp10;
            if (btnp10)
            {

                CH10.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                Chart10.Visibility = Visibility.Collapsed;
                Chart010.Visibility = Visibility.Collapsed;


            }
            else
            {
                CH10.Background = new SolidColorBrush(Color.FromArgb(255, 58, 254, 0));
                Chart10.Visibility = Visibility.Visible;
                Chart010.Visibility = Visibility.Visible;
            }

        }

        private void CHCallBack11(object sender, RoutedEventArgs e)
        {
            btnp11 = !btnp11;
            if (btnp11)
            {

                CH11.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                Chart11.Visibility = Visibility.Collapsed;
                Chart011.Visibility = Visibility.Collapsed;


            }
            else
            {
                CH11.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 205));
                Chart11.Visibility = Visibility.Visible;
                Chart011.Visibility = Visibility.Visible;
            }

        }

        private void CHCallBack12(object sender, RoutedEventArgs e)
        {
            // Don't run analysis when there is nothing to analyze.
            dispatcherTimer.Stop();

            inkPresenter.StrokeContainer.Clear();
            inkAnalyzer.ClearDataForAllStrokes();
            canvas.Children.Clear();

            try
            {


            }
            catch { }


            Lepton.scrs.Clear();
            contador = 0;
            ejex = 600; 

        }


        private void ZoomOffsetCallBack(object sender, RangeBaseValueChangedEventArgs e)
        {
            if(DAQ.DAQ1.ChannelBuffers !=null)
            {
                //MainApp.Handler.Signals.OffsetChart = (int)(ZoomOffset.Value); //samples x decimateFactor should be shorter than source array
                //OffsetChart = (int)(ZoomOffset.Value);
            }
        }
        bool play = true;
        private void PlayCallBack(object sender, RoutedEventArgs e)
        {
            play = !play;
        }
        private void CHCallBack0(object sender, RoutedEventArgs e)
        {
            IsChEnabled[0] = !IsChEnabled[0];
        }

        private void CHCallBack1(object sender, RoutedEventArgs e)
        {
            IsChEnabled[1] = !IsChEnabled[1];
        }

        private void CHCallBack2(object sender, RoutedEventArgs e)
        {
            IsChEnabled[2] = !IsChEnabled[2];
        }

        private void CHCallBack3(object sender, RoutedEventArgs e)
        {
            IsChEnabled[3] = !IsChEnabled[3];
        }

        private void CHCallBack4(object sender, RoutedEventArgs e)
        {
            IsChEnabled[4] = !IsChEnabled[4];
        }

        private void CHCallBack5(object sender, RoutedEventArgs e)
        {
            IsChEnabled[5] = !IsChEnabled[5];
        }

        private void CHCallBack6(object sender, RoutedEventArgs e)
        {
            IsChEnabled[6] = !IsChEnabled[6];
        }

        private void CHCallBack7(object sender, RoutedEventArgs e)
        {
            IsChEnabled[7] = !IsChEnabled[7];
        }        

        enum GraphType
        {
            Time,
            FFT,
            Int,
            CHxCH
        }
        GraphType CurrentGraph = GraphType.Time;
        int Card = 0; 
      

        
        async void Process(object sender, object e)
        {
            UITimer.Stop();
            try
            {
                if (Lepton.isNewDataProcessed)
                {
                    Lepton.isNewDataProcessed = false;

                    if (Lepton.coloredImage != null)
                    {
                        string DateString = Lepton.LastTemperatureDataUpdate.ToString();
                        var source = new SoftwareBitmapSource();
                        await source.SetBitmapAsync(Lepton.coloredImage);
                        PicDañada.Source = source;
                        UpdateEstadoCam1("Conectada");
                        UpdateDateCam1(DateString);
                    }

                    // muestra los valores de la temperatura sobre los botones 
                    if (Lepton.scrs.Count == 0)
                    {


                        try
                        {
                            lblScr1.Text = Convert.ToString("0.0");
                            lblScr2.Text = Convert.ToString("0.0");
                            lblScr3.Text = Convert.ToString("0.0");

                        }
                        catch { }

                    }
                    else
                    {


                        try
                        {
                            lblScr1.Text = Lepton.scrs[0].averageTemp.ToString("0.0");
                            lblScr2.Text = Lepton.scrs[1].averageTemp.ToString("0.0");
                            lblScr3.Text = Lepton.scrs[2].averageTemp.ToString("0.0");

                        }
                        catch { }

                    }


                    if (Lepton.lastGraphUpdate.AddMilliseconds(Lepton.milisecondsToUpdateGraph) <= DateTime.Now)
                    {
                        Lepton.lastGraphUpdate = DateTime.Now;
                        GarficaLeton = true;

                    }


                }

                var a = await Draw();
            }
            catch (Exception ex)
            {
            }
            UITimer.Start();
        }




        // puntos para los pixeles de temperatura 
        PointCollection points9 = new PointCollection();
        PointCollection points10 = new PointCollection();
        PointCollection points11 = new PointCollection();

        PointCollection points09 = new PointCollection();
        PointCollection points010 = new PointCollection();
        PointCollection points011 = new PointCollection();





        public Image[] ImageArray = new Image[6];
        private Canvas[] CanvasArray = new Canvas[6];
        
        private Grid[] DestinationGridArray = new Grid[6];
        public int OffsetChart = 0;
        int coin = 0;

        public async Task<bool> DrawThermal()
        {
            bool r = false; 
            
            try 
            {
                t1Pocentaje = (int)Lepton.scrs[0].averageTemp;
                t2Pocentaje = (int)Lepton.scrs[1].averageTemp;
                t3Pocentaje = (int)Lepton.scrs[2].averageTemp;

                /*Convertir los porcentajes a valores de pixeles y graficar */
                pixelScr1 = (t1Pocentaje * 330) / 100;
                pixelScr1 = 330 - pixelScr1;

                pixelScr2 = (t2Pocentaje * 330) / 100;
                pixelScr2 = 330 - pixelScr2;

                pixelScr3 = (t3Pocentaje * 330) / 100;
                pixelScr3 = 330 - pixelScr3;



                points10.Add(new Windows.Foundation.Point(ejex, pixelScr2));
                points010.Add(new Windows.Foundation.Point(ejex, pixelScr2));

                points11.Add(new Windows.Foundation.Point(ejex, pixelScr3));
                points011.Add(new Windows.Foundation.Point(ejex, pixelScr3));



                r = true;
            }
            catch 
            {
            
            }
            return r; 
        }


        public async Task<bool> Draw()
        {
            int incrementox = 1; 
            switch (contador)
            {
                case 1:

                    t1Pocentaje = (int)Lepton.scrs[0].averageTemp;
                    pixelScr1 = resizeTempInCanva(t1Pocentaje);

                    Chart9.Points = points9;
                    Chart09.Points = points09;

                    points9.Add(new Windows.Foundation.Point(ejex, pixelScr1));
                    points09.Add(new Windows.Foundation.Point(ejex, pixelScr1));
                    ejex = ejex + incrementox;

                    break;
                case 2:

                    t1Pocentaje = (int)Lepton.scrs[0].averageTemp;
                    t2Pocentaje = (int)Lepton.scrs[1].averageTemp;
                    /*Convertir los porcentajes a valores de pixeles y graficar */
                    pixelScr1 = resizeTempInCanva(t1Pocentaje);
                    pixelScr2 = resizeTempInCanva(t2Pocentaje);

                    Chart9.Points = points9;
                    Chart09.Points = points09;
                    Chart10.Points = points10;
                    Chart010.Points = points010;

                    points9.Add(new Windows.Foundation.Point(ejex, pixelScr1));
                    points09.Add(new Windows.Foundation.Point(ejex, pixelScr1));

                    points10.Add(new Windows.Foundation.Point(ejex, pixelScr2));
                    points010.Add(new Windows.Foundation.Point(ejex, pixelScr2));
                    ejex = ejex + incrementox;

                    break;

                case 3:
                    t1Pocentaje = (int)Lepton.scrs[0].averageTemp;
                    t2Pocentaje = (int)Lepton.scrs[1].averageTemp;
                    t3Pocentaje = (int)Lepton.scrs[2].averageTemp;

                    /*Convertir los porcentajes a valores de pixeles y graficar */
                    pixelScr1 = resizeTempInCanva(t1Pocentaje);
                    pixelScr2 = resizeTempInCanva(t2Pocentaje);
                    pixelScr3 = resizeTempInCanva(t3Pocentaje);

                    Chart9.Points = points9;
                    Chart09.Points = points09;
                    Chart10.Points = points10;
                    Chart010.Points = points010;
                    Chart11.Points = points11;
                    Chart011.Points = points011;

                    points9.Add(new Windows.Foundation.Point(ejex, pixelScr1));
                    points09.Add(new Windows.Foundation.Point(ejex, pixelScr1));

                    points10.Add(new Windows.Foundation.Point(ejex, pixelScr2));
                    points010.Add(new Windows.Foundation.Point(ejex, pixelScr2));

                    points11.Add(new Windows.Foundation.Point(ejex, pixelScr3));
                    points011.Add(new Windows.Foundation.Point(ejex, pixelScr3));
                    ejex = ejex + incrementox;
                    break;

                default:
                    
                    break;
            }



            if (ejex > 576)
            {
                points9.Clear();
                points09.Clear();
                points10.Clear();
                points010.Clear();
                points11.Clear();
                points011.Clear();
                ejex = 0;

            }


            if (false)   
            {
                try
                {


                    if (ejex == 0 && (t1Pocentaje != 0 || t2Pocentaje != 0 || t3Pocentaje != 0))  // hay datos de la camara inicia a graficar 
                    {


                        points9.Clear();
                        points09.Clear();
                        points10.Clear();
                        points010.Clear();
                        points11.Clear();
                        points011.Clear();

                        Chart9.Points = points9;
                        Chart09.Points = points09;
                        Chart10.Points = points10;
                        Chart010.Points = points010;
                        Chart11.Points = points11;
                        Chart011.Points = points011;




                        var IncrementoHoras = 6;       // 1 dia de incremento 
                        lblTime1.Text = DateTime.Now.ToString("t");
                        lblTime1a.Text = DateTime.Now.AddHours(IncrementoHoras).ToString("t");
                        lblTime1b.Text = DateTime.Now.AddHours(IncrementoHoras * 2).ToString("t");
                        lblTime1c.Text = DateTime.Now.AddHours(IncrementoHoras * 3).ToString("t");
                        lblTime2.Text = DateTime.Now.AddHours(IncrementoHoras * 4).ToString("t");
                        paint = true; 

                    }
                    if(t1Pocentaje == 0 && t2Pocentaje == 0 && t3Pocentaje == 0) // no hay datos de la camara no comienza Graficar
                    {

                        points9.Add(new Windows.Foundation.Point(ejex,330));
                        points09.Add(new Windows.Foundation.Point(ejex,330));

                        points10.Add(new Windows.Foundation.Point(ejex,330));
                        points010.Add(new Windows.Foundation.Point(ejex,330));

                        points11.Add(new Windows.Foundation.Point(ejex,330));
                        points011.Add(new Windows.Foundation.Point(ejex,330));
                        ejex = 0;
                        paint = false; 
                    }

                    if (paint)
                    {
                        points9.Add(new Windows.Foundation.Point(ejex, pixelScr1));
                        points09.Add(new Windows.Foundation.Point(ejex, pixelScr1));

                        points10.Add(new Windows.Foundation.Point(ejex, pixelScr2));
                        points010.Add(new Windows.Foundation.Point(ejex, pixelScr2));

                        points11.Add(new Windows.Foundation.Point(ejex, pixelScr3));
                        points011.Add(new Windows.Foundation.Point(ejex, pixelScr3));

                        //ejex++;
                        // incremento del eje x, por 10 unidades en x 
                        ejex = ejex + 10; 

                    }
                    if (ejex > 576)
                    {
                        points9.Clear();
                        points09.Clear();
                        points10.Clear();
                        points010.Clear();
                        points11.Clear();
                        points011.Clear();
                        ejex= 0;

                    }

                    
                }
                catch (Exception ex)
                {

                }

                GarficaLeton = false;

            }
            
            return true;
        }
        public class Pacer
        { 
            public DateTime Past = DateTime.Now;
            public static long Period = 3000000000;  //60sec
            public bool IsFirstTriggerd = false;
        }

        Pacer[] PacerArray = new Pacer[6];

        private int resizeTempInCanva(int tempInput)
        {
            // int tempInput es la temperatura proveniente de la roi de la camara
            // esto es para la grafica de temperaturas, no para la imagen
            int puntos_y = 330;  // pixeles disponibles en y 
            int rangoTemp = 400;  // rango de temperatura de 0 400

            int pixelresize_Y = 0;
            pixelresize_Y = (tempInput * puntos_y) / rangoTemp;
            pixelresize_Y = puntos_y - pixelresize_Y;

            return pixelresize_Y;
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            // Don't perform analysis while a stroke is in progress.
            dispatcherTimer.Stop();

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't run analysis when there is nothing to analyze.
            dispatcherTimer.Stop();

            inkPresenter.StrokeContainer.Clear();
            inkAnalyzer.ClearDataForAllStrokes();
            canvas.Children.Clear();
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            dispatcherTimer.Stop();
            inkAnalyzer.AddDataForStrokes(args.Strokes);
            dispatcherTimer.Start();
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            dispatcherTimer.Stop();
            foreach (var stroke in args.Strokes)
            {
                inkAnalyzer.RemoveDataForStroke(stroke.Id);
            }
            dispatcherTimer.Start();
        }

       
        private async void DispatcherTimer_Tick(object sender, object e)
        {
            dispatcherTimer.Stop();
            if (!inkAnalyzer.IsAnalyzing)
            {
                InkAnalysisResult results = await inkAnalyzer.AnalyzeAsync();
                if (results.Status == InkAnalysisStatus.Updated)
                {
                    if (contador < 3)
                    {
                        ConvertShapes();
                        contador++;
                    }

                    inkPresenter.StrokeContainer.Clear();
                    inkAnalyzer.ClearDataForAllStrokes();
                }
            }
            else
            {
                // Ink analyzer is busy. Wait a while and try again.
                //dispatcherTimer.Start();
            }
        }

        private void ConvertShapes()
        {


            IReadOnlyList<IInkAnalysisNode> drawings = inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing);
            
            var a = inkAnalyzer.AnalysisRoot.BoundingRect;
            Point p1 = new Point(a.X, a.Y);
            Point p2 = new Point(a.X + a.Width, a.Y);
            Point p3 = new Point(a.X + a.Width, a.Y + a.Height);
            Point p4 = new Point(a.X, a.Y + a.Height);
            AddPolygonToCanvasv2(p1, p2, p3, p4,contador);
            inkAnalyzer.ClearDataForAllStrokes();

            X_OriginalImage = ResizeX(a.X);
            y_OriginalImage = ResizeY(a.Y);
            widthOriginalImage = ResizeX(a.Width);
            heightOriginalImage = ResizeY(a.Height);

            enebleDarwRectangle = true; 
            inkPresenter.StrokeContainer.DeleteSelected();

            inkPresenter.StrokeContainer.Clear();
            inkAnalyzer.ClearDataForAllStrokes();
            //canvas.Children.Clear();



        }

        private double ResizeY(double y)
        {
            // reajusta el alto de la imagen en y, por ejemplo de 404 a 120
            int yImagenGrande = 360;
            int yImagenPequena = 240;
            double porcentaje_y = (y * 100) / (yImagenGrande);
            double ysizeOriginal = (porcentaje_y * yImagenPequena) / (100);

            return ysizeOriginal;
        }

        private double ResizeX(double x)
        {
            // reajusta el ancho de la imagen en y, por ejemplo de 538 a 160
            int xImagenGrande = 480;
            int xImagenPequena = 320;
            double porcentaje_y = (x * 100) / (xImagenGrande);
            double xsizeOriginal = (porcentaje_y * xImagenPequena) / (100);

            return xsizeOriginal;
        }


       
        private void AddPolygonToCanvasv2(Point p1, Point p2, Point p3, Point p4,byte opt)
        {

            double grosorLine = 5;

            switch (opt)
            {
                
                case 0:
                    Polygon polygon = new Polygon();
                    polygon.StrokeThickness = grosorLine;
                    polygon.Points.Add(p1);
                    polygon.Points.Add(p2);
                    polygon.Points.Add(p3);
                    polygon.Points.Add(p4);
                    polygon.Stroke = new SolidColorBrush(Color.FromArgb(255, 3, 169, 244));
                    canvas.Children.Add(polygon);
                    break;
                case 1:
                    Polygon polygon2 = new Polygon();
                    polygon2.StrokeThickness = grosorLine;
                    polygon2.Points.Add(p1);
                    polygon2.Points.Add(p2);
                    polygon2.Points.Add(p3);
                    polygon2.Points.Add(p4);
                    polygon2.Stroke = new SolidColorBrush(Color.FromArgb(255, 58, 254, 0));
                    canvas.Children.Add(polygon2);


                    break;
                case 2:
                    Polygon polygon3 = new Polygon();
                    polygon3.StrokeThickness = grosorLine;
                    polygon3.Points.Add(p1);
                    polygon3.Points.Add(p2);
                    polygon3.Points.Add(p3);
                    polygon3.Points.Add(p4);
                    polygon3.Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 0, 205));
                    canvas.Children.Add(polygon3);
                    break;

                default:
                    break; 
            }
        }

        static double Distance(Point p0, Point p1)
        {
            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        private void AddEllipseToCanvas(InkAnalysisInkDrawing shape)
        {
            Ellipse ellipse = new Ellipse();

            // Ellipses and circles are reported as four points
            // in clockwise orientation.
            // Points 0 and 2 are the extrema of one axis,
            // and points 1 and 3 are the extrema of the other axis.
            // See Ellipse.svg for a diagram.
            IReadOnlyList<Point> points = shape.Points;

            // Calculate the geometric center of the ellipse.
            var center = new Point((points[0].X + points[2].X) / 2.0, (points[0].Y + points[2].Y) / 2.0);

            // Calculate the length of one axis.
            ellipse.Width = Distance(points[0], points[2]);

            var compositeTransform = new CompositeTransform();
            if (shape.DrawingKind == InkAnalysisDrawingKind.Circle)
            {
                ellipse.Height = ellipse.Width;
            }
            else
            {
                // Calculate the length of the other axis.
                ellipse.Height = Distance(points[1], points[3]);

                // Calculate the amount by which the ellipse has been rotated
                // by looking at the angle our "width" axis has been rotated.
                // Since the Y coordinate is inverted, this calculates the amount
                // by which the ellipse has been rotated clockwise.
                double rotationAngle = Math.Atan2(points[2].Y - points[0].Y, points[2].X - points[0].X);

                RotateTransform rotateTransform = new RotateTransform();
                // Convert radians to degrees.
                compositeTransform.Rotation = rotationAngle * 180.0 / Math.PI;
                compositeTransform.CenterX = ellipse.Width / 2.0;
                compositeTransform.CenterY = ellipse.Height / 2.0;
            }

            compositeTransform.TranslateX = center.X - ellipse.Width / 2.0;
            compositeTransform.TranslateY = center.Y - ellipse.Height / 2.0;

            ellipse.RenderTransform = compositeTransform;

            canvas.Children.Add(ellipse);
        }

        private void ComboBox_SelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            opencvEneable = true; 
            var selected = (ComboBoxItem)cmbOpencv.SelectedItem;
            String value  = selected.Content.ToString();
            valueOpencv = selected.Content.ToString();
            switch (value)
            {
                case "Original":
                   
                    break;
                case "AUTUMN":
                   
                    break;
                case "BONE":
                    
                    break;
                case "JET":

                    break;
                default:
                    
                    break;
            }

        }
        public void UpdateEstadoCam1(string estado)
        {
            EstatusCam1.Text = estado;
        }
        public void UpdateDateCam1(string estado)
        {
            FechaCam1.Text = estado;
        }
    }
}

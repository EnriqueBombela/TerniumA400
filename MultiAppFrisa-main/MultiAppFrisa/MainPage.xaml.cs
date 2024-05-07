using PredictorV2.Common;
using PredictorV2.Pages;
using Syncfusion.XlsIO;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PredictorV2
{
    public sealed partial class MainPage : Page
    {
        DispatcherTimer UITimer = null;
        bool IsTimerRun = false;
        public static IWorksheet worksheet;

        public MainPage()
        {
            this.InitializeComponent();
            //Defines Monitor resolution
            var scr = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            var visiblebounds = scr.VisibleBounds;

            //ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1024, 768);
            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1200, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            visiblebounds.Height = 800;
            visiblebounds.Width = 1200;

            MainContainer.ScreenProperties.Height = visiblebounds.Height;
            MainContainer.ScreenProperties.Width = visiblebounds.Width;
            MainContainer.ScreenProperties.WorkHeight = MainContainer.ScreenProperties.Height - logo.Height;
            MainContainer.ScreenProperties.WorkWidth = MainContainer.ScreenProperties.Width - logo.Width;

            MenuGrid.Width = MainContainer.ScreenProperties.WorkWidth;
            //NavigationStackPanel.Height = MainContainer.ScreenProperties.WorkHeight;
            //var buttonsize = (MainContainer.ScreenProperties.WorkHeight - 20) / 4;
            //TimeButton.Height = buttonsize;
            //HistButton.Height = buttonsize;
            //HMIButton.Height = buttonsize;
           // HMIButton2.Height = buttonsize;
            MainContainer.ScreenProperties.rFrame = rootFrame;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            IsTimerRun = true;
            UITimer = new DispatcherTimer();
            UITimer.Tick += Process;
            UITimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            UITimer.Start();
            //HistButton.Background = new SolidColorBrush(Color.FromArgb(255, 0, 115, 197));
            //This. vs MainFrame. First does not show buttons
            if(false) //if(!MainContainer.Devices.Web)
            {
                MainContainer.Devices.Web = true;
                this.Frame.Navigate(typeof(MyWebView), null, new SuppressNavigationTransitionInfo());
            }
            else
            {
               // TimeButton.Background = new SolidColorBrush(Color.FromArgb(255, 0, 115, 197));
                MainContainer.Devices.Web = false;
                var BackgroundColor = new SolidColorBrush(Windows.UI.Colors.Transparent);
               // HistButton.Background = BackgroundColor;
                MainFrame.Navigate(typeof(TimeZoom), null, new SuppressNavigationTransitionInfo());

            }

            //OpenFile();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IsTimerRun = false;
            if(UITimer!=null)UITimer.Stop();
            base.OnNavigatedFrom(e);
        }


        private void ClickToPlayLogo_ImageOpened(object sender, RoutedEventArgs e)
        {
        }

        private static async void OpenFile()
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();

                //Instantiates the File Picker. 
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                openPicker.FileTypeFilter.Add(".xlsx");
                openPicker.FileTypeFilter.Add(".xls");
                StorageFile openFile = await openPicker.PickSingleFileAsync();

                //Opens the workbook. 
                IWorkbook workbook = await excelEngine.Excel.Workbooks.OpenAsync(openFile);
                worksheet = workbook.Worksheets[1];
            }
            catch (Exception ex)
            {

            }
        }

        void Process(object sender, object e)
        {
            try
            {
                if (IsTimerRun)
                {
                    Stamp.Text = DateTime.Now.ToString();
                 /* if (MainContainer.InternetConnectionModel.InternetController.IsWiFiAdapter)
                    {
                        WiFiStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 153, 255, 51));
                    }
                    else
                    {
                        WiFiStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    }*/
                  //  WiFiStatus.Text = MainContainer.InternetConnectionModel.InternetController.WiFiStrength.ToString();

                  /*  if (MainContainer.InternetConnectionModel.InternetController.IsEthernetDetected)
                    {
                        EthStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 153, 255, 51));
                        if (MainContainer.InternetConnectionModel.InternetController.IsInternetAccess)
                        {
                            EthStatus.Text = ((char)(59598)).ToString();
                        }
                        else
                        {
                            EthStatus.Text = ((char)(59597)).ToString();
                        }
                    }
                    else
                    {
                        EthStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                        EthStatus.Text = ((char)(59597)).ToString();
                    } */
                    if (Modbus.Modbus1.OK)
                    {
                        ModbusStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    }
                    else
                    {
                        ModbusStatus.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    }
                    if (DAQ.DAQ1.OK)
                    {
                        AcqCard0.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    }
                    else
                    {
                        AcqCard0.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    }
                    //if (DAQ.DAQ2.OK)
                    //{
                    //    AcqCard1.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    //}
                    //else
                    //{
                    //    AcqCard1.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    //}
                    //if (DAQ.DAQ2.OK)
                    //{
                    // AcqCard2.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    //}
                    //else
                    //{
                    //    AcqCard2.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    //}
                    //if (DAQ.DAQ3.OK)
                    //{
                    //    AcqCard3.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    //}
                    //else
                    //{
                    //    AcqCard3.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    //}
                    if (MainContainer.Devices.CLOUDSQL)
                    {
                        CloudIcon(IconStatus.Enable);
                    }
                    else
                    {
                        CloudIcon(IconStatus.Disable);
                    }
                }
            }
            catch(Exception ex)
            {
            }
        }

        static string currentValue = string.Empty;

        private void NavigationButton_Checked(object sender, RoutedEventArgs e)
        {
            var contentControl = sender as ContentControl;
            if (contentControl?.Content != null)
            {
                string value = (string)(contentControl.Content);
                if ((currentValue != value) || (value == "Web"))
                {
                    currentValue = value;
                    var BackgroundColor = new SolidColorBrush(Windows.UI.Colors.Transparent);
                   // HistButton.Background = BackgroundColor;
                    //HMIButton.Background = BackgroundColor;
                    //HMIButton2.Background = BackgroundColor;
                    //TimeButton.Background = BackgroundColor;
                    var SelectedColor = new SolidColorBrush(Color.FromArgb(255, 0, 115, 197));
                    switch (value)
                    {
                        case "RTV":
                           // TimeButton.Background = SelectedColor;
                            MainFrame.Navigate(typeof(TimeZoom), null, new SuppressNavigationTransitionInfo());
                            break;
                        case "Web":
                            //HistButton.Background = SelectedColor;
                            this.Frame.Navigate(typeof(MyWebView));
                            break;
                        case "NONE1":
                           // HMIButton.Background = SelectedColor;
                            MainFrame.Navigate(typeof(TimeZoom), null, new EntranceNavigationTransitionInfo());
                            break;
                        case "NONE2":
                          //  HMIButton2.Background = SelectedColor;
                            MainFrame.Navigate(typeof(TimeZoom), null, new SuppressNavigationTransitionInfo());
                            break;
                    }
                }
            }
        }

        public enum IconStatus
        {
            InProgress,
            Disable,
            Enable
        };

        public void MailIcon(IconStatus s)
        {
            switch (s)
            {
                case IconStatus.Disable:
                    Mail.Text = ((char)(59157)).ToString();
                    Mail.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    break;
                case IconStatus.Enable:
                    Mail.Text = ((char)(59157)).ToString();
                    Mail.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    break;
            }
        }
        public void CloudIcon(IconStatus s)
        {
            switch (s)
            {
                case IconStatus.Disable:
                    Cloud.Text = ((char)(59219)).ToString();
                    Cloud.Foreground = new SolidColorBrush(Color.FromArgb(255, 119, 119, 119));
                    break;
                case IconStatus.Enable:
                    Cloud.Text = ((char)(59219)).ToString();
                    Cloud.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 102));
                    break;
            }
        }
    }
}

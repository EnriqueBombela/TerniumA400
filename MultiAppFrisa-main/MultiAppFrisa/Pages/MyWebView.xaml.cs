using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using PredictorV2.Pages;
using Windows.UI.Xaml;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PredictorV2.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyWebView : Page
    {
        public MyWebView()
        {
            this.InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            //WebViewControl.Navigate(new Uri("http://hornolinea1.azurewebsites.net/Informacion/index"));
            WebViewControl.Navigate(new Uri("http://35.231.212.137/hornolinea1/Operador/Index?U=eFrgTSdRYVsDfHjPlInv"));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void PlaybackCallBack(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
        private void RefreshCallBack(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MyWebView));
        }
        private void WebViewControl_ContainsFullScreenElementChanged(Windows.UI.Xaml.Controls.WebView sender, object args)
        {
            var applicationView = ApplicationView.GetForCurrentView();
            if (sender.ContainsFullScreenElement)
            {
                applicationView.TryEnterFullScreenMode();
            }
            else
            {
                // It is harmless to exit full screen mode when not full screen.
                applicationView.ExitFullScreenMode();
            }
        }
    }
}

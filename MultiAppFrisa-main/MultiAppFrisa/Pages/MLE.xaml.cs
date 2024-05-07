using System;
using System.Collections.ObjectModel;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using PredictorV2.Common;
using Windows.System;
using Windows.Media;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PredictorV2.Pages
{
    public sealed partial class MLE : Page
    {
        DispatcherTimer UITimer = null;

        public MLE()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            UITimer = new DispatcherTimer();
            UITimer.Tick += Process;
            UITimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            UITimer.Start();
        }

        async void Process(object sender, object e)
        {
        }

        private async void ProcessCurrentVideoFrame()
        {
            try
            {
            }
            catch (Exception Ex)
            {
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            UITimer.Stop();
            base.OnNavigatedFrom(e);
        }
    }
}

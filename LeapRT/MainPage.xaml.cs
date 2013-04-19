using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LeapRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        bool loggingenabled = false;
        //LeapListener ll;
        public LeapListener listener;
        Leap.Controller controller;

        public MainPage()
        {
            this.InitializeComponent();
            //var app = (App)Application.Current;
            //ll = app.listener;
            //ll.OnLogUpdate += ll_OnLogUpdate;
        }

        void ll_OnLogUpdate(object sender, LeapListenerArgs e)
        {
            //throw new NotImplementedException();
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, ()=>{ TextBlock_log.Text += "\n" + e.logdata;});
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            listener = new LeapListener();
            controller = new Leap.Controller();
            controller.AddListener(listener);

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            controller.RemoveListener(listener);
            controller.Dispose();

            base.OnNavigatingFrom(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            loggingenabled = !loggingenabled;

            if (loggingenabled)
            {
                listener.OnLogUpdate += ll_OnLogUpdate;
                Button_logtoggle.Content = "Stop Logging";
            }
            else
            {
                listener.OnLogUpdate -= ll_OnLogUpdate;
                Button_logtoggle.Content = "Start Logging";
            }

        }
    }
}

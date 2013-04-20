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

            listener.OnFrameUpdate += listener_OnFrameUpdate;
            listener.OnDeviceStatusUpdate +=listener_OnDeviceStatusUpdate;

            listener.OnCircleGesture += listener_OnCircleGesture;
            listener.OnKeyTapGesture += listener_OnKeyTapGesture;
            listener.OnScreenTapGesture += listener_OnScreenTapGesture;
            listener.OnSwipeGesture += listener_OnSwipeGesture;
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

        #region Leap Events

        #region Gesture Event Handlers
        void listener_OnSwipeGesture(object sender, LeapListenerArgs e)
        {
            PrintLog(e);
        }

        void listener_OnScreenTapGesture(object sender, LeapListenerArgs e)
        {
            PrintLog(e);
        }

        void listener_OnKeyTapGesture(object sender, LeapListenerArgs e)
        {
            PrintLog(e);
        }

        void listener_OnCircleGesture(object sender, LeapListenerArgs e)
        {
            PrintLog(e);
        }

        #endregion

        void listener_OnDeviceStatusUpdate(object sender, LeapListenerArgs e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {

                string texttodisplay = "";

                switch (e.devicestatus)
                {
                    case LeapDeviceState.connected: texttodisplay = "Connected"; break;
                    case LeapDeviceState.disconnected: texttodisplay = "Disconnected"; break;
                    case LeapDeviceState.exited: texttodisplay = "Exited"; break;
                    case LeapDeviceState.initialized: texttodisplay = "Initialized"; break;
                }

                TextBlock_DeviceStatus.Text = texttodisplay;
            });

        }

        void listener_OnFrameUpdate(object sender, LeapListenerArgs e)
        {

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                TextBlock_HandCount.Text = e.currentframe.Hands.Count.ToString();
                TextBlock_FingerCount.Text = e.currentframe.Fingers.Count.ToString();

                var frame = e.currentframe;

                switch (frame.Hands.Count)
                {
                    case 1: circle_left.Visibility = Visibility.Visible;
                        circle_right.Visibility = Visibility.Collapsed;
                        break;

                    case 2: circle_left.Visibility = Visibility.Visible;
                        circle_right.Visibility = Visibility.Visible;
                        break;

                    case 0:
                        circle_left.Visibility = Visibility.Collapsed;
                        circle_left.Visibility = Visibility.Collapsed;
                        break;
                }
            });
        }

        void ll_OnLogUpdate(object sender, LeapListenerArgs e)
        {
            //throw new NotImplementedException();
            PrintLog(e);
        }

        #endregion

        #region Helper Methods

        private void PrintLog(LeapListenerArgs e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { TextBlock_log.Text += "\n" + e.logdata; });
        }

        #endregion

    }
}

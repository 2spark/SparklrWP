using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparklrForWindowsPhone.Helpers
{
    /// <summary>
    /// Handles the display of the global loading Indicator. The indicator will be visible, until Stop() has been called for each Start().
    /// </summary>
    public static class GlobalLoadingIndicator
    {
        private static ProgressIndicator progressIndicator;

        public static void Initialize(PhoneApplicationFrame frame)
        {
            progressIndicator = new ProgressIndicator();
            progressIndicator.IsIndeterminate = true;

            frame.Navigated += OnRootFrameNavigated;
        }

        private static void OnRootFrameNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var ee = e.Content;
            var pp = ee as PhoneApplicationPage;
            if (pp != null)
            {
                pp.SetValue(SystemTray.ProgressIndicatorProperty, progressIndicator);
            }
        }


        private static int loadingCount = 0;

        /// <summary>
        /// Signals the start of a time consuming process
        /// </summary>
        public static void Start()
        {
            
            loadingCount++;
            refreshVisibility();
        }

        /// <summary>
        /// Signals the end of a time consuming process
        /// </summary>
        public static void Stop()
        {
            loadingCount--;
            refreshVisibility();
        }

        private static void refreshVisibility()
        {
            progressIndicator.IsVisible = (loadingCount != 0);
        }
    }
}

﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SparklrLib;
using SparklrWP.Utils;
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Navigation;

namespace SparklrWP
{
    public partial class App : Application
    {
        private static MainViewModel postsViewModel = null;
        private static NotificationsViewModel notificationsViewModel = null;
        private static FriendsViewModel friendsViewModel = null;
        public static WPClogger logger = new WPClogger(LogLevel.debug);

        public static Utils.Task BackgroundTask;

        public static SparklrClient Client = new SparklrClient();
        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel PostsViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (postsViewModel == null)
                    postsViewModel = new MainViewModel();

                return postsViewModel;
            }
        }

        public static FriendsViewModel FriendsViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (friendsViewModel == null)
                    friendsViewModel = new FriendsViewModel();

                return friendsViewModel;
            }
        }

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The NotificationsViewModel object.</returns>
        public static NotificationsViewModel NotificationsViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (notificationsViewModel == null)
                    notificationsViewModel = new NotificationsViewModel();

                return notificationsViewModel;
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

#if DEBUG
            MemoryDiagnosticsHelper.Start(TimeSpan.FromMilliseconds(500), true);
#endif

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            // TODO: Make sure that restoring doesn't screw with the update timer
            //if (!App.ViewModel.IsDataLoaded)
            //{
            //    App.ViewModel.LoadData();
            //}
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (RemoveBackEntryOnNavigate)
            {
                RemoveBackEntryOnNavigate = false;
                RootFrame.Dispatcher.BeginInvoke(() => RootFrame.RemoveBackEntry());
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("authkey") && !App.Client.IsLoggedIn &&
                  IsolatedStorageSettings.ApplicationSettings.Contains("userid"))
            {
                byte[] authBytes = null;
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("authkey", out authBytes);
                authBytes = ProtectedData.Unprotect(authBytes, null);
                App.Client.ManualLogin(Encoding.UTF8.GetString(authBytes, 0, authBytes.Length),
                    (long)IsolatedStorageSettings.ApplicationSettings["userid"]);
            }
            else if (!e.Uri.ToString().Contains("/LoginPage.xaml") && !App.Client.IsLoggedIn)
            {
                e.Cancel = true;
                LoginReturnUri = e.Uri;
                RootFrame.Dispatcher.BeginInvoke(() => RootFrame.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative)));
            }
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                logger.log(LogLevel.critical, e.ExceptionObject);
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigating += RootFrame_Navigating;
            GlobalLoading.Instance.Initialize(RootFrame);

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        private static string _aviaryApiKey;

        public static string AviaryApiKey
        {
            get
            {
                if (_aviaryApiKey == null)
                {
                    var resourceStream = Application.GetResourceStream(new Uri("aviaryapikey.txt", UriKind.Relative));

                    if (resourceStream != null)
                    {
                        using (Stream myFileStream = resourceStream.Stream)
                        {
                            if (myFileStream.CanRead)
                            {
                                //Will be disposed when the underlying stream is disposed, no using required
                                StreamReader myStreamReader = new StreamReader(myFileStream);
                                //read the content here
                                _aviaryApiKey = myStreamReader.ReadToEnd();
                            }
                        }

                    }

                }
                return _aviaryApiKey;
            }
        }

        public static Uri LoginReturnUri { get; set; }

        public static bool RemoveBackEntryOnNavigate { get; set; }
    }
}
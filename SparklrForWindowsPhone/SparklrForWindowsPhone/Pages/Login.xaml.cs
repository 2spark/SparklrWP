using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SparklrSharp;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using SparklrForWindowsPhone.Helpers;
using System.Windows.Input;
using System.Threading.Tasks;

namespace SparklrForWindowsPhone.Pages
{
    public partial class Login : PhoneApplicationPage
    {
        Housekeeper houseKeeper = new Housekeeper();

        /// <summary>
        /// Creates a new instance of the Login-Page
        /// </summary>
        public Login()
        {
            InitializeComponent();
            Housekeeper.ServiceConnection.CurrentUserIdentified += conn_CurrentUserIdentified;
        }


        private void OnBackKey(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Kills the app
            App.Current.Terminate();

            //TODO: Handle the back button once the user is logged in 
        }

        /// <summary>
        /// Checks if the enter key has been pressed. If so, the focus is either moved to finish entering the username/password. If both username and password are present, a login is performed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void checkForEnter(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(String.IsNullOrEmpty(SparklrUsername.Text))
                {
                    SparklrUsername.Focus();
                }
                else if(String.IsNullOrEmpty(SparklrPassword.Password))
                {
                    SparklrPassword.Focus();
                }
                else
                {
                    setUiState(false);
                    await performLogin();
                    setUiState(true);
                }
            }
        }

        /// <summary>
        /// Enables/disables UI elements such as the textboxes and password
        /// </summary>
        /// <param name="isEnabled">True if the user should be able to enter details, otherwise false</param>
        private void setUiState(bool isEnabled)
        {
            SparklrUsername.IsEnabled = isEnabled;
            SparklrPassword.IsEnabled = isEnabled;

            foreach(ApplicationBarIconButton b in this.ApplicationBar.Buttons)
            {
                b.IsEnabled = isEnabled;
            }
        }

        private async void Login_Click(object sender, System.EventArgs e)
        {
            setUiState(false);
            await performLogin();
            setUiState(true);
        }

        /// <summary>
        /// Performs an asynschronous login and displays a message, if it fails.
        /// </summary>
        /// <returns></returns>
        private async Task performLogin()
        {
#if DEBUG
            LoadToast();
#endif

            SparklrForWindowsPhone.Helpers.GlobalLoadingIndicator.Start();
            if (await Housekeeper.ServiceConnection.SigninAsync(SparklrUsername.Text, SparklrPassword.Password))
            {
                DebugHelper.LogDebugMessage("User is logged in");
                //The information about the currently logged in user will be retreived in the background. It will be available, once the event below has fired.
            }
            else
            {
                MessageBox.Show("Something went wrong :( Please check your username, password and make sure you're connected to the internet.", "login failed", MessageBoxButton.OK);
            }
            SparklrForWindowsPhone.Helpers.GlobalLoadingIndicator.Stop();
        }

        /// <summary>
        /// Is invoked when the login succeeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void conn_CurrentUserIdentified(object sender, SparklrSharp.Sparklr.UserIdentifiedEventArgs e)
        {
            DebugHelper.LogDebugMessage("User identified as @{0}", Housekeeper.ServiceConnection.CurrentUser.Handle);
            //Saves the info into the app settings -Suraj
            houseKeeper.SaveCreds(SparklrUsername.Text, SparklrPassword.Password);
            NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
             
        }

        private new void Loaded(object sender, RoutedEventArgs e)
        {
            // Load available login data, if present
            if (houseKeeper.LoginDataAvailable)
            {
                houseKeeper.GetCreds();

                SparklrUsername.Text = houseKeeper.SparklrUsername;
                SparklrPassword.Password = houseKeeper.SparklrPassword;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!NavigationService.CanGoBack && e.NavigationMode == NavigationMode.Back)
                App.Current.Terminate();
        }

#if DEBUG
        private void LoadToast()
        {
            /* TESTING TOAST NOTIFICATIONS ON WP 8.1 IGNORE THIS
             * 
             * 
             * 
             */
             
            // Using the ToastText02 toast template.
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;

            // Retrieve the content part of the toast so we can change the text.
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            //Find the text component of the content
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");

            // Set the text on the toast. 
            // The first line of text in the ToastText02 template is treated as header text, and will be bold.
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Sparklr"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("Runninng, this is a test of the WP action center\nIt works"));

            // Set the duration on the toast
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            // Create the actual toast object using this toast specification.
            ToastNotification toast = new ToastNotification(toastXml);
            toast.SuppressPopup = true;

            // Send the toast.
            ToastNotificationManager.CreateToastNotifier().Show(toast);
           
        }
#endif

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
        }

        private void Register_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Register.xaml", UriKind.Relative));
        }

    }
}
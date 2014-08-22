using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SparklrForWindowsPhone.Resources;
using SparklrForWindowsPhone.Helpers;
using System.Windows.Media;
using System.Diagnostics;
using SparklrSharp;
using SparklrForWindowsPhone.ViewModels;
using Telerik.Windows.Controls;
using Windows.UI.Core;

namespace SparklrForWindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
       
        Housekeeper Housekeeper = new Housekeeper();
        Connection conn = new Connection();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            //TODO: This might keep the page in memory
            conn.CurrentUserIdentified += conn_CurrentUserIdentified;
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void OnBackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //No Need to show the user the log in page after they have logged in once
            App.Current.Terminate();
        }

        private async void Mainpage_Loaded(object sender, RoutedEventArgs e)
        {
            if(Housekeeper.LoginDataAvailable == false)
            {
                NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
            }
            else if (!App.ViewModel.IsDataLoaded)
            {
                bool loginSuccess = false;
                Housekeeper.GetCreds();
                Helpers.GlobalLoadingIndicator.Start();
                loginSuccess = await conn.SigninAsync(Housekeeper.SparklrUsername, Housekeeper.SparklrPassword);
                
                if(!loginSuccess)
                {
                    MessageBox.Show("We were unable to log you in. Pleas reenter your username and password", "Sorry...", MessageBoxButton.OK);
                    NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
                }
            }
        }

       
        void conn_CurrentUserIdentified(object sender, SparklrSharp.Sparklr.UserIdentifiedEventArgs e)
        {
            Helpers.GlobalLoadingIndicator.Stop();
        }

        private async void NewPost_Click(object sender, System.EventArgs e)
        {
            
            InputPromptClosedEventArgs result = await RadInputPrompt.ShowAsync(new string[] { "Post", "Extended" }, "New Post");
            
            if(result.Result == DialogResult.OK && !String.IsNullOrWhiteSpace(result.Text))
            {
#if DEBUG
                Helpers.DebugHelper.LogDebugMessage("Sending post: {0}", result.Text);
#endif
                Helpers.GlobalLoadingIndicator.Start();
                bool success = await Housekeeper.ServiceConnection.SubmitPostAsync(result.Text);
                //TODO: Check if post exceeds maximum char limit
                Helpers.GlobalLoadingIndicator.Stop();

                if(!success)
                {
                    MessageBox.Show("We were unable to submit your post. Please try again later.", "Sorry :(", MessageBoxButton.OK);
                }
            }
            else if(result.Result == DialogResult.Cancel)
            {
                NavigationService.Navigate(new Uri("/Pages/Post.xaml", UriKind.Relative));
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/About.xaml", UriKind.Relative));
        }

        private void RadDataBoundListBox_DataRequested(object sender, EventArgs e)
        {
            App.ViewModel.LoadMore(MainPivot.SelectedIndex);
        }

        private async void RadDataBoundListBox_RefreshRequested(object sender, EventArgs e)
        {
            await App.ViewModel.LoadNewer(MainPivot.SelectedIndex);

            RadDataBoundListBox rdblb = sender as RadDataBoundListBox;

            if(rdblb != null)
            {
                rdblb.StopPullToRefreshLoading(true);
            }
#if DEBUG
            else
            {
                if (Debugger.IsAttached)
                    Debugger.Break(); // The sender was not a RadDataBoundListBox. This should not happen
            }
#endif
        }

        private void PivotChanged(object sender, SelectionChangedEventArgs e)
        {
            App.ViewModel.LoadMore(MainPivot.SelectedIndex);
        }

        private void Search_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Search.xaml", UriKind.Relative));
        }

        private void PostGrid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ListBoxItem control = sender as ListBoxItem;

            if(control != null)
            {
                PostViewModel pvm = control.DataContext as PostViewModel;

                if(pvm != null)
                {
                    SparklrForWindowsPhone.Pages.ViewPost.selectedPost = pvm;
                    NavigationService.Navigate(new Uri("/Pages/ViewPost.xaml", UriKind.Relative));
                }
#if DEBUG
                else
                {
                    if (Debugger.IsAttached)
                        Debugger.Break(); // The DataContext was not a PostViewModel Should not happen
                }
#endif
            }
#if DEBUG
            else
            {
                if (Debugger.IsAttached)
                    Debugger.Break(); // The sender was not a ListBoxItem. This should not happen
            }
#endif
        }
        /// <summary>
        /// Sign off the user though the appbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Signout_Click(object sender, EventArgs e)
        {
            await Housekeeper.ServiceConnection.SignoffAsync();
        
            Housekeeper.RemoveCreds();
            NavigationService.Navigate(new Uri("Login.xaml", UriKind.Relative));
        }
    }
}
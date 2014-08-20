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
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private async void Mainpage_Loaded(object sender, RoutedEventArgs e)
        {
            if(Housekeeper.LoginDataAvailable == false)
            {
                NavigationService.Navigate(new Uri("/Pages/Login.xaml", UriKind.Relative));
            } 
            else
            {
                //Gets Login Info
                Housekeeper.GetCreds();
                Helpers.GlobalLoadingIndicator.Start();
                await conn.SigninAsync(Housekeeper.SparklrUsername, Housekeeper.SparklrPassword);
                conn.CurrentUserIdentified += conn_CurrentUserIdentified;
            }
        }

       
        void conn_CurrentUserIdentified(object sender, SparklrSharp.Sparklr.UserIdentifiedEventArgs e)
        {
            Helpers.GlobalLoadingIndicator.Stop();
        }

        private void NewPost_Click(object sender, System.EventArgs e)
        {
            RadInputPrompt.Show("New Post");
            //NavigationService.Navigate(new Uri("/Pages/Post.xaml", UriKind.Relative));
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/About.xaml", UriKind.Relative));
        }

        private void RadDataBoundListBox_DataRequested(object sender, EventArgs e)
        {
            App.ViewModel.LoadMore(MainPivot.SelectedIndex);
        }

        private void PivotChanged(object sender, SelectionChangedEventArgs e)
        {
            App.ViewModel.LoadMore(MainPivot.SelectedIndex);
        }

        public static void Show(
    ControlTemplate template,
    object title,
    IEnumerable<object> buttonsContent,
    object message = null,
    InputMode inputMode = InputMode.Text,
    Style inputStyle = null,
    object checkBoxContent = null,
    bool isCheckBoxChecked = false,
    bool vibrate = false,
    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Stretch,
    VerticalAlignment verticalAlignment = VerticalAlignment.Top,
    Action<InputPromptClosedEventArgs> closedHandler = null,
    Action<KeyEventArgs> keyDownHandler = null)
        {
            //New post code goes here
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
                    NavigationService.Navigate(new Uri("/Pages/ViewPost.xaml", UriKind.Relative));
                    // TODO: Pass selected post to page
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

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}
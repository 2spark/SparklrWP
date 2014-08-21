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

namespace SparklrForWindowsPhone.Pages
{
    public partial class Post : PhoneApplicationPage
    {
        public Post()
        {
            InitializeComponent();
        }

        private async void NewPost_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(result.Text))
            {
#if DEBUG
                Helpers.DebugHelper.LogDebugMessage("Sending post: {0}", result.Text);
#endif
                Helpers.GlobalLoadingIndicator.Start();
                bool success = await Housekeeper.ServiceConnection.SubmitPostAsync(result.Text);
                //TODO: Check if post exceeds maximum char limit
                Helpers.GlobalLoadingIndicator.Stop();

                if (!success)
                {
                    MessageBox.Show("We were unable to submit your post. Please try again later.", "Sorry :(", MessageBoxButton.OK);
                }
                else
                {
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }
        }
    }
}
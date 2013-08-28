﻿using Microsoft.Phone.Controls;
using SparklrLib.Objects;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace SparklrWP.Pages
{
    public partial class Profile : PhoneApplicationPage
    {
        ProfileViewModel model;

        public Profile()
        {
            InitializeComponent();
            model = new ProfileViewModel();
            this.DataContext = model;
            model.PropertyChanged += model_PropertyChanged;
        }

        async void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BackgroundImage")
            {
                ImageBrush image = new ImageBrush();
                image.ImageSource = await Utils.Helpers.LoadImageFromUrlAsync(model.BackgroundImage);
                image.Stretch = Stretch.UniformToFill;
                image.Opacity = 0;
                MainPanorama.Background = image;
                Storyboard.SetTarget(FadeInAnimation, image);
                FadeInStoryboard.Begin();
            }
        }

        public bool dataLoaded = false;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (dataLoaded) return;
            dataLoaded = true;
            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("userId", out selectedIndex))
            {
                model.ID = int.Parse(selectedIndex);

                JSONRequestEventArgs<SparklrLib.Objects.Responses.Work.User> usargs = await App.Client.GetUserAsync(model.ID);

                if (usargs.IsSuccessful)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        model.Handle = "@" + usargs.Object.handle;
                        model.BackgroundImage = "http://d.sparklr.me/i/b" + model.ID + ".jpg";
                        model.ProfileImage = "http://d.sparklr.me/i/" + model.ID + ".jpg";

                        model.Bio = usargs.Object.bio;
                        if (model.Bio.Trim() == "")
                        {
                            model.Bio = usargs.Object.name + " is too shy to write something about his/herself maybe check again later!";
                        }
                    });
                }
            }
        }
    }
}
﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SparklrForWindowsPhone.Resources;
using SparklrForWindowsPhone.Helpers;
using SparklrSharp;
using SparklrSharp.Extensions;
using SparklrSharp.Sparklr;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparklrForWindowsPhone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public readonly string[] Networks = { "following", "popular", "everything", "music", "funny", "tech", "gaming", "art", "misc" };

        public MainViewModel()
        {
            this.Items = new ObservableCollection<StreamPageViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<StreamPageViewModel> Items { get; private set; }

        /// <summary>
        /// Sample property that returns a localized string
        /// </summary>
        
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Performs an initial load of the stram data
        /// </summary>
        public async void LoadData()
        {
            //TODO: Is this the right place to show the indicator? Maybe move to *Page.cs
            GlobalLoadingIndicator.Start();

            foreach(string s in Networks)
            {
                Items.Add(await StreamPageViewModel.CreateInstanceAsync(s));
            }

            GlobalLoadingIndicator.Stop();
        }
    }
}
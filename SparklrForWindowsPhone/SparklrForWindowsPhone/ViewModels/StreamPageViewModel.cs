using SparklrForWindowsPhone.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SparklrSharp.Extensions;
using SparklrSharp;
using System.Collections.ObjectModel;
using SparklrSharp.Sparklr;

namespace SparklrForWindowsPhone.ViewModels
{
    public class StreamPageViewModel : ViewModelBase
    {
        public StreamPageViewModel(string name)
        {
            this.Network = name;
            Posts = new ObservableCollection<PostViewModel>();
        }

        public StreamPageViewModel()
        {
            Posts = new ObservableCollection<PostViewModel>();
        }

        public ObservableCollection<PostViewModel> Posts { get; set; }

        private string _network;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string Network
        {
            get
            {
                return _network;
            }
            set
            {
                if (value != _network)
                {
                    _network = value;
                    NotifyPropertyChanged();
                }
            }
        }

        internal Stream stream = null;
        private bool currentlyWorking = false;

        internal async Task LoadMore()
        {
            if (!currentlyWorking)
            {
                currentlyWorking = true;
                GlobalLoadingIndicator.Start();

                if (stream == null)
                {
                    stream = await Housekeeper.ServiceConnection.GetStreamAsync(this.Network);

                    foreach (Post p in stream.Posts)
                        this.Posts.Add(new PostViewModel(p.Author, p.Content));
                }
                else
                {
                    int previousCount = this.Posts.Count + 1;

                    await stream.LoadOlderPosts(Housekeeper.ServiceConnection);

                    for (int i = previousCount; i < stream.Posts.Count; i++)
                    {
                        this.Posts.Add(new PostViewModel(stream.Posts[i].Author, stream.Posts[i].Content));
                    }
                }

                GlobalLoadingIndicator.Stop();
                currentlyWorking = false;
            }
        }

        internal async Task LoadNewer()
        {
            if (!currentlyWorking)
            {
                currentlyWorking = true;
                GlobalLoadingIndicator.Start();

                if (stream != null)
                {
                    int oldLength = stream.Posts.Count;
                    bool result = await stream.LoadNewerPosts(Housekeeper.ServiceConnection);

                    if(result)
                    {
                        int newLength = stream.Posts.Count;

                        int numberOfNewPosts = newLength - oldLength;

                        for(int i = numberOfNewPosts - 1; i  >= 0; i--)
                        {
                            PostViewModel pvm = new PostViewModel(stream.Posts[i].Author, stream.Posts[i].Content);
                            Posts.Insert(0, pvm);
                        }
                    }
                }
#if DEBUG
                else
                {
                    System.Diagnostics.Debugger.Break();
                }
#endif

                GlobalLoadingIndicator.Stop();
                currentlyWorking = false;
            }
        }
    }
}
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

        internal static async Task<StreamPageViewModel> CreateInstanceAsync(string s)
        {
            StreamPageViewModel svm = new StreamPageViewModel(s);

#if DEBUG
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
#endif

            Stream stream = await Housekeeper.ServiceConnection.GetStreamAsync(s);

#if DEBUG
            sw1.Stop();
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            
            foreach(Post p in stream.Posts)
            {
                svm.Posts.Add(new PostViewModel(p.Author, p.Content));
            }

#if DEBUG
            sw.Stop();

            Helpers.DebugHelper.LogDebugMessage("Loaded {0} posts from {1} in {2}ms. SparklrSharp took {3}ms", svm.Posts.Count, s, sw.ElapsedMilliseconds, sw1.ElapsedMilliseconds);
#endif

            return svm;
        }
    }
}
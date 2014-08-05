using SparklrSharp.Sparklr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparklrForWindowsPhone.ViewModels
{
    public class PostViewModel : ViewModelBase
    {
        private User user;
        public User User
        {
            get
            {
                return user;
            }
            set
            {
                if(user != value)
                {
                    user = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string content;
        public String Content
        {
            get
            {
                return content;
            }
            set
            {
                if(content != value)
                {
                    content = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PostViewModel()
        { }

        public PostViewModel(SparklrSharp.Sparklr.User user, string content)
        {
            User = user;
            Content = content;
        }

    }
}

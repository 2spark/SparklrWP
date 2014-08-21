using SparklrForWindowsPhone.Helpers;
using SparklrSharp.Sparklr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

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

        private Post post;
        public Post Post
        {
            get
            {
                return post;
            }
            set
            {
                if(post != value)
                {
                    post = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool commentsLoaded = false;

        private ObservableCollection<ConversationViewMessage> comments = new ObservableCollection<ConversationViewMessage>();
        public ObservableCollection<ConversationViewMessage> Comments
        {
            get
            {
                return comments;
            }
            set
            {
                if(comments != value)
                {
                    comments = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PostViewModel()
        { }

        public PostViewModel(User user, Post post)
        {
            User = user;
            Post = post;
        }

        public async void LoadComments()
        {
            if(!commentsLoaded)
            {
                Helpers.GlobalLoadingIndicator.Start();

                List<Comment> comments = new List<Comment>(await Post.GetCommentsAsync(Housekeeper.ServiceConnection));

                if(comments.Count > 0)
                {
                    for(int i = 0; i < comments.Count; i++)
                    {
                        Comments.Add(new ConversationViewMessage(comments[i].Message, new DateTime(1999, 1, 1), comments[i].Author == Housekeeper.ServiceConnection.CurrentUser ? ConversationViewMessageType.Incoming : ConversationViewMessageType.Outgoing));
                    }
                }

                Helpers.GlobalLoadingIndicator.Stop();
                commentsLoaded = true;
            }
        }

    }
}

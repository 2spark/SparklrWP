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
        #region Support for DesignTime data
        private string username;
        private string profileImage;
        private string content;
        private string network = "/";
        private int commentCount;


        public string Username
        {
            get
            {
                if(DesignTimeHelper.IsDesignTime())
                {
                    return username;
                }
                else
                {
                    return user.Handle;
                }
            }

            set
            {
#if DEBUG
                DesignTimeHelper.FailAtRuntime();
#endif
                if(username != value)
                {
                    username = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ProfileImage
        {
            get
            {
                if (DesignTimeHelper.IsDesignTime())
                {
                    return profileImage;
                }
                else
                {
                    return String.Format("http://d.sparklr.me/i/t{0}.jpg", user.UserId);
                }
            }

            set
            {
#if DEBUG
                DesignTimeHelper.FailAtRuntime();
#endif
                if (profileImage != value)
                {
                    profileImage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Content
        {
            get
            {
                if (DesignTimeHelper.IsDesignTime())
                {
                    return content;
                }
                else
                {
                    return post.Content;
                }
            }

            set
            {
#if DEBUG
                DesignTimeHelper.FailAtRuntime();
#endif
                if (content != value)
                {
                    content = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Network
        {
            get
            {
                if (DesignTimeHelper.IsDesignTime())
                    return network;
                else
                    return (post.Network != "everything" && post.Network != "following") ? String.Format("in /{0}", post.Network) : "";
            }
            set
            {
#if DEBUG
                DesignTimeHelper.FailAtRuntime();
#endif
                if(network != value)
                {
                    network = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int CommentCount
        {
            get
            {
                if (DesignTimeHelper.IsDesignTime())
                    return commentCount;
                else
                    return post.CommentCount;
            }
            set
            {
#if DEBUG
                DesignTimeHelper.FailAtRuntime();
#endif
                if(commentCount != value)
                {
                    commentCount = value;
                    NotifyPropertyChanged();
                }
            }
        }
        #endregion

        private Post post;
        private User user;

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
            this.user = user;
            this.post = post;
        }

        public async void LoadComments()
        {
            if(!commentsLoaded)
            {
                Helpers.GlobalLoadingIndicator.Start();

                List<Comment> comments = new List<Comment>(await this.post.GetCommentsAsync(Housekeeper.ServiceConnection));

                if(comments.Count > 0)
                {
                    for(int i = 0; i < comments.Count; i++)
                    {
                        Comments.Add(new ConversationViewMessage(comments[i].ToString(), new DateTime(1999, 1, 1), comments[i].Author == Housekeeper.ServiceConnection.CurrentUser ? ConversationViewMessageType.Incoming : ConversationViewMessageType.Outgoing));
                    }
                }

                Helpers.GlobalLoadingIndicator.Stop();
                commentsLoaded = true;
            }
        }

    }
}

﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SparklrWP
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        public int Id { get; private set; }

        public ItemViewModel(int Id)
        {
            this.Id = Id;
        }
        //For design:
        public ItemViewModel()
        {

        }

        private string _from;
        public string From
        {
            get
            {
                return _from;
            }
            set
            {
                if (value != _from)
                {
                    _from = value;
                    NotifyPropertyChanged("From");
                }
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

        private int _commentCount;
        public int CommentCount
        {
            get
            {
                return _commentCount;
            }
            set
            {
                if (value != _commentCount)
                {
                    _commentCount = value;
                    NotifyPropertyChanged("CommentCount");
                }
            }
        }

        // TODO: Implement properly
        private int _likesCount;
        public int LikesCount
        {
            get
            {
                return _likesCount;
            }
            set
            {
                if (value != _likesCount)
                {
                    _likesCount = value;
                    NotifyPropertyChanged("LikesCount");
                }
            }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get
            {
                return _imageUrl;
            }
            set
            {
                if (value != _imageUrl)
                {
                    _imageUrl = value;
                    NotifyPropertyChanged("ImageUrl");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
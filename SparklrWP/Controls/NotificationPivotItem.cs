﻿using Microsoft.Phone.Controls;
using System;
using System.ComponentModel;
using System.Windows;

namespace SparklrWP.Controls
{
    public class NotificationPivotItem : PivotItem
    {

        private int _newCount = 0;
        public int NewCount
        {
            get
            {
                return _newCount;
            }
            set
            {
                if (value != _newCount)
                {
                    _newCount = value;
                    NotifyPropertyChanged("NewCount");
                    NotifyPropertyChanged("NewCountVisibility");
                }
            }
        }

        public Visibility NewCountVisibility
        {
            get
            {
                return _newCount > 0 ? Visibility.Visible : Visibility.Collapsed;
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

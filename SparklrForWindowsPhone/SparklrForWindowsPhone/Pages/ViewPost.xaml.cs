
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using SparklrForWindowsPhone.Helpers;
using SparklrForWindowsPhone.ViewModels;

namespace SparklrForWindowsPhone.Pages
{
    public partial class ViewPost : PhoneApplicationPage
    {
        internal static PostViewModel selectedPost;

        public ViewPost()
        {
            InitializeComponent();
            DataContext = selectedPost;
        }
    }
}

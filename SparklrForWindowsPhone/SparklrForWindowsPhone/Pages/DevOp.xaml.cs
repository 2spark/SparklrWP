using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SparklrForWindowsPhone.Helpers;

namespace SparklrForWindowsPhone.Pages
{
    public partial class DevOp : PhoneApplicationPage
    {
        ErrorReportHelper err = new ErrorReportHelper();
        public DevOp()
        {
            InitializeComponent();
        }

        private void RaiseEX_click(object sender, RoutedEventArgs e)
        {
            throw new Exception("I CRASHED THE APP!");
        }

        private void Purge_Click(object sender, RoutedEventArgs e)
        {
            err.SendErrorReport();
        }
    }
}
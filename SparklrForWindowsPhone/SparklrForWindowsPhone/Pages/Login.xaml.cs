﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SparklrSharp;
using System.Diagnostics;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace SparklrForWindowsPhone.Pages
{
    public partial class Login : PhoneApplicationPage
    {
        Connection conn = new Connection();
        public Login()
        {
            InitializeComponent();
        }

        private void OnBackKey(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Kills the app
            App.Current.Terminate();
        }

        private async void Login_Click(object sender, System.EventArgs e)
        {
            LoadToast();
            Debugger.Log(1, "Sparklr", SparklrUsername.Text + " " + SparklrPassword.Password);
            await conn.SigninAsync(SparklrUsername.Text, SparklrPassword.Password);
            MessageBox.Show(conn.CurrentUser.Handle.ToString(),"It Worked!",MessageBoxButton.OK);
        }

        private void Loaded(object sender, RoutedEventArgs e)
        {

          
        }
        private void LoadToast()
        {
            /* TESTING TOAST NOTIFICATIONS ON WP 8.1 IGNORE THIS
             * 
             * 
             * 
             */
             
            // Using the ToastText02 toast template.
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;

            // Retrieve the content part of the toast so we can change the text.
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            //Find the text component of the content
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");

            // Set the text on the toast. 
            // The first line of text in the ToastText02 template is treated as header text, and will be bold.
            toastTextElements[0].AppendChild(toastXml.CreateTextNode("Sparklr"));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode("Runninng, this is a test of the WP action center"));

            // Set the duration on the toast
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            // Create the actual toast object using this toast specification.
            ToastNotification toast = new ToastNotification(toastXml);
            toast.SuppressPopup = true;

            // Send the toast.
            ToastNotificationManager.CreateToastNotifier().Show(toast);
           
        }

    }
}
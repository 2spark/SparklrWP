﻿using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using SparklrLib;
using SparklrLib.Objects;
using SparklrLib.Objects.Responses.Beacon;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
namespace SparklrWP_Background_Agent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            if (IsolatedStorageSettings.ApplicationSettings.Contains("username") && IsolatedStorageSettings.ApplicationSettings.Contains("password"))
            {
                SparklrClient client = new SparklrClient();
                string username = IsolatedStorageSettings.ApplicationSettings["username"].ToString();
                byte[] passbyts = ProtectedData.Unprotect((byte[])IsolatedStorageSettings.ApplicationSettings["password"], null);
                string password = Encoding.UTF8.GetString(passbyts, 0, passbyts.Length);

                Func<Notification, String> textGenerator = (not) =>
                {
                    if (not.type == 1)
                    {
                        if (not.body == "☝")
                        {
                            return "{0} likes this post.";
                        }
                        else
                        {
                            return "{0} commented: " + not.body;
                        }
                    }
                    else
                    {
                        return "{0} mentioned you";
                    }
                };

                LoginEventArgs loginArgs = await client.LoginAsync(username, password);
                if (loginArgs.IsSuccessful)
                {
                    JSONRequestEventArgs<Stream> args = await client.GetBeaconStreamAsync(0, 1);
                    if (args.IsSuccessful)
                    {
                        Stream strm = args.Object;

                        if (strm.notifications == null)
                            strm.notifications = new List<Notification>();
#if DEBUG
                        strm.notifications.Add(new Notification()
                       {
                           from = 4,
                           type = 1,
                           body = "Debug test"
                       });
#endif
                        if (strm.notifications != null)
                        {

                            List<int> userIds = new List<int>();
                            foreach (Notification not in strm.notifications)
                            {
                                if (!userIds.Contains(not.from))
                                {
                                    userIds.Add(not.from);
                                }
                            }
                            JSONRequestEventArgs<SparklrLib.Objects.Responses.Work.Username[]> unargs = await client.GetUsernamesAsync(userIds.ToArray()); if (unargs.IsSuccessful)
                            {
                                foreach (Notification not in strm.notifications)
                                {
                                    ShellToast notif = new ShellToast();
                                    notif.Title = "Sparklr*";
                                    notif.Content = String.Format(textGenerator(not), client.Usernames[not.from]);
                                    notif.NavigationUri = new Uri("/Pages/MainPage.xaml?notification=" + not.id, UriKind.Relative);
                                    notif.Show();
                                }
                                if (unargs.IsSuccessful)
                                {

                                    foreach (ShellTile til in ShellTile.ActiveTiles)
                                    {
                                        StandardTileData data = new StandardTileData();
                                        data.Title = "Sparklr*";
                                        data.BackContent = String.Format(textGenerator(strm.notifications[0]), client.Usernames[strm.notifications[0].from]);
                                        System.Net.WebClient wc = new System.Net.WebClient();
                                        //wc.D
                                        //data.BackgroundImage = new Uri("http://d.sparklr.me/i/" + strm.notifications[0].from + ".jpg");
                                        data.BackTitle = "Sparklr*";
                                        data.Count = strm.notifications.Count;
                                        til.Update(data);
                                    }

                                    foreach (Notification not in strm.notifications)
                                    {
                                        ShellToast notif = new ShellToast();
                                        notif.Title = "Sparklr*";
                                        notif.Content = String.Format(textGenerator(not), client.Usernames[not.from]);
                                        notif.NavigationUri = new Uri("/Pages/MainPage.xaml?notification=" + not.id, UriKind.Relative);
                                        notif.Show();
                                    }
                                }
                                else
                                {
                                    foreach (Notification not in strm.notifications)
                                    {
                                        ShellToast notif = new ShellToast();
                                        notif.Title = "Sparklr*";
                                        notif.Content = String.Format(textGenerator(not), "Someone");
                                        notif.NavigationUri = new Uri("/Pages/MainPage.xaml?notification=" + not.id, UriKind.Relative);
                                        notif.Show();
                                    }
                                }
                            }
                            else
                            {
                                foreach (Notification not in strm.notifications)
                                {
                                    ShellToast notif = new ShellToast();
                                    notif.Title = "Sparklr*";
                                    notif.Content = String.Format(textGenerator(not), "Someone");
                                    notif.NavigationUri = new Uri("/Pages/MainPage.xaml?notification=" + not.id, UriKind.Relative);
                                    notif.Show();
                                }
                            }
                        }
                    }
                }
            }
            NotifyComplete();
        }
    }
}
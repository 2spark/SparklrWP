using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;

namespace SparklrForWindowsPhone.Helpers
{
    public class ErrorReportHelper
    {
        private IsolatedStorageSettings appSettings =
          IsolatedStorageSettings.ApplicationSettings;
        private string lasterror { get; set; }

        EmailComposeTask eml = new EmailComposeTask();
        
        public void SendErrorReport()
        {
            try
            {
                  lasterror = (string)appSettings["LastError"].ToString();
            }
            catch
            {
                 lasterror = "No Errors";
            }
            var Platform = Environment.OSVersion.Version.Build.ToString();
            var Device = Microsoft.Phone.Info.DeviceStatus.DeviceName.ToString();
            var DeviceID = Windows.Phone.System.Analytics.HostInformation.PublisherHostId.ToString();
            var DeviceCarrier = Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.CellularMobileOperator;
            var Ticket = Guid.NewGuid();
            eml.To = "Support@2Spark.PixelatedTile.com";
            eml.Subject = "SUPPORT: 2Spark [" + Ticket + "]";
            eml.Body = "<enter your suggestions/complaint details here>\n\n--------\n\nTicket: " + Ticket.ToString() + "\nDevice: " + Device + "\nDevice ID: " + DeviceID + "\nDevice Network: " + DeviceCarrier + "\nWindows Phone Build: " + Platform + "\n----------\n\n" + "LAST ERROR:\n\n\n" + lasterror;
            eml.Show();
            try
            {
                appSettings.Remove("LastError");
            }
            catch
            {
                
            }
        }
    }
}

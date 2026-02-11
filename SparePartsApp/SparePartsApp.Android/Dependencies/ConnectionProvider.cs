using Android.Content;
using Android.Net;
using SparePartsApp.Droid.Dependencies;
using SparePartsApp.Helpers;
using System;
using System.Net;
using Xamarin.Forms;

[assembly: Dependency(typeof(ConnectionProvider))]
namespace SparePartsApp.Droid.Dependencies
{
    public class ConnectionProvider : IConnectionProvider
    {
        public bool CheckWifi()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Forms.Context.GetSystemService(Context.ConnectivityService);

            NetworkInfo info = connectivityManager.ActiveNetworkInfo;

            return info != null && info.IsConnected;
        }

        public bool HasWebServerAvaiable(string checkUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(checkUrl);

                request.Timeout = 10000;

                WebResponse response = request.GetResponse();

                response.Close();

                return true;
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        } 
    }
}
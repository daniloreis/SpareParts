using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using SparePartsApp.Dependencies;
using SparePartsApp.Helpers;
using SparePartsApp.Services;
using Xamarin.Forms;

namespace SparePartsApp.Droid
{
    [Service]
    public class UpdateService : Service
    {
        IUpdateProvider updateProvider;

        private IUpdateProvider GetupdateProvider()
        {
            if (updateProvider == null)
                updateProvider = new UpdateProvider();

            return updateProvider;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            try
            {
                Task.Run(() =>
                {
                    GetupdateProvider().CheckUpdate();
                });
            }
            catch (Exception)
            {

            }
            return StartCommandResult.Sticky;
        }
    }
}
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Java.Util;
using SparePartsApp.Dependencies;
using Xamarin.Forms;
using Android;
using System;
using System.Linq;

namespace SparePartsApp.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        ProgressDialog progress;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            #region LOADING

            MessagingCenter.Subscribe<App>(this, "ShowLoading", (sender) =>
            {
                progress = new ProgressDialog(this);
                progress.Indeterminate = true;
                progress.SetProgressStyle(ProgressDialogStyle.Spinner);
                progress.SetMessage("Processando...");
                progress.SetCancelable(false);
                progress.SetButton("Fechar", new EventHandler<DialogClickEventArgs>((source, args) => { progress.Dismiss(); }));

                if (!progress.IsShowing)
                    progress.Show();
            });

            MessagingCenter.Subscribe<App>(this, "HideLoading", (sender) =>
            {
                if (progress != null)
                    progress.Dismiss();
            });

            #endregion          

            //StartService(new Intent(this, typeof(UpdateService)));
            CheckPermissions();

            Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        private void CheckPermissions()
        {
            string[] permissions = { Manifest.Permission.ReadExternalStorage,
                                     Manifest.Permission.WriteExternalStorage,
                                     Manifest.Permission.AccessNetworkState,
                                     Manifest.Permission.Camera,
                                     Manifest.Permission.InstallPackages
                                    };

            if (permissions.Any(permission => CheckSelfPermission(permission) != (int)Permission.Granted))
            {
                RequestPermissions(permissions, 0);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            CameraProvider.OnResult(resultCode);
        }
    }
}
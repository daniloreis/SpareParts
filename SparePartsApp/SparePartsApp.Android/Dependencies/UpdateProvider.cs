using System;
using Android.App;
using Android.Content;
using System.IO;
using System.Net;
using Xamarin.Forms;
using SparePartsApp.Dependencies;
using SparePartsApp.Helpers;
using Android.Content.PM;
using System.Threading.Tasks;
using static Android.Content.PM.PackageInstaller;

[assembly: Dependency(typeof(UpdateProvider))]
namespace SparePartsApp.Dependencies
{
    public class UpdateProvider : IUpdateProvider
    {
        long downloadId = 0;

        public async void CheckUpdate()
        {
            await ToUpdate("app.apk");
        }

        private async Task ToUpdate(string fileName)
        {
            try
            {
                String urlServer = $"http://{Helper.Server}/" + fileName;
                string file = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + "/" + fileName;
                Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + file);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlServer);

                using (WebResponse response = await request.GetResponseAsync())
                {
                    DateTime localAppDate = File.GetLastWriteTime(file);
                    DateTime remoteAppDate = DateTime.Parse(response.Headers["Last-Modified"].ToString());

                    if (remoteAppDate.CompareTo(localAppDate) > 0)
                    {
                        if (File.Exists(file))
                            File.Delete(file);

                        DownloadManager.Request requestDownload = new DownloadManager.Request(Android.Net.Uri.Parse(urlServer));
                        requestDownload.SetDescription("Atualização de aplicativo");
                        requestDownload.SetTitle("SPA Update");
                        requestDownload.SetDestinationUri(uri);

                        DownloadManager manager = (DownloadManager)Android.App.Application.Context.GetSystemService(Context.DownloadService);

                        var cursor = manager.InvokeQuery(new DownloadManager.Query().SetFilterById(downloadId));

                        if (cursor != null && cursor.MoveToNext())
                        {
                            int status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
                            cursor.Close();

                            if (status != (int)DownloadStatus.Running && status != (int)DownloadStatus.Pending)
                            {
                                downloadId = manager.Enqueue(requestDownload);
                            }
                        }
                        else
                            downloadId = manager.Enqueue(requestDownload);
                    }
                }
            }
            catch (WebException ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

        public string GetVersion()
        {
            var context = Android.App.Application.Context;
            var VersionNumber = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MetaData).VersionName;
            //var BuildNumber = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.MetaData).VersionCode.ToString();

            return VersionNumber;
        }

        public void CheckVersion()
        {
            try
            {

                Android.Net.Uri uri = Android.Net.Uri.Parse(@"file://" + Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + "/app.apk");

                var context = Android.App.Application.Context;

                long localLastModified = context.PackageManager.GetPackageInfo(context.Packag‌​eName, 0).LastUpdateTime;

                var remoteLastModified = new Java.IO.File(uri.Path).LastModified();

                long fileSize = new Java.IO.File(uri.Path).Length();

                var localAppDate = new Java.Util.Date(localLastModified);
                var remoteAppDate = new Java.Util.Date(remoteLastModified);

                if (remoteAppDate.After(localAppDate))
                {
                    var ad = new AlertDialog.Builder(Forms.Context).Create();
                    ad.SetTitle("Aviso");
                    ad.SetMessage("Uma nova versão está disponível.");
                    ad.SetButton("Instalar", delegate
                    {
                        Intent install = new Intent(Intent.ActionView, uri);
                        install.AddFlags(ActivityFlags.NewTask);
                        install.SetDataAndType(uri, "application/vnd.android.package-archive");
                        context.StartActivity(install);
                    });
                    ad.Show();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.GetInnerExceptionMessage();
            }
        }

    }
}
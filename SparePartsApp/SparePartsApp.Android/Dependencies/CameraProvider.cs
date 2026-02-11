using Android.App;
using Android.Content;
using Android.Provider;
using Java.IO;
using SparePartsApp.Dependencies;
using SparePartsApp.Helpers;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using System.Linq;

[assembly: Dependency(typeof(CameraProvider))]
namespace SparePartsApp.Dependencies
{
    public class CameraProvider : ICameraProvider
    {
        private static File file;
        private static File picDir;

        private static TaskCompletionSource<CameraResult> tcs;

        public async Task<CameraResult> TakePictureAsync()
        {
            try
            {
                Intent intent = new Intent(MediaStore.ActionImageCapture);

                picDir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "SparePartsApp");

                if (!picDir.Exists())
                    picDir.Mkdirs();

                file = new File(picDir, String.Format("fotografia_{0}.jpg", Guid.NewGuid()));

                intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(file));

                Activity activity = (Activity)Forms.Context;
                activity.StartActivityForResult(intent, 0);

                tcs = new TaskCompletionSource<CameraResult>();

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ImageSource GetPicture(string filePath)
        {
            try
            {
                return ImageSource.FromFile(filePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void OnResult(Result resultCode)
        {
            try
            {
                if (resultCode == Result.Canceled)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                if (resultCode != Result.Ok)
                {
                    tcs.TrySetException(new Exception("Unexpected error"));
                    return;
                }

                CameraResult res = new CameraResult();
                res.Picture = ImageSource.FromFile(file.Path);
                res.FilePath = file.Path;

                tcs.TrySetResult(res);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string[] ListAllPictures()
        {
            try
            {
                var picDir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "SparePartsApp");

                var files = picDir.ListFiles();

                return files.Select(x => x.Path).ToArray();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
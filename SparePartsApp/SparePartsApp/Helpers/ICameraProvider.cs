using System.Threading.Tasks;
using Xamarin.Forms;

namespace SparePartsApp.Helpers
{
    public interface ICameraProvider
    {
        Task<CameraResult> TakePictureAsync();

        ImageSource GetPicture(string filePath);

        string[] ListAllPictures();
    }
}

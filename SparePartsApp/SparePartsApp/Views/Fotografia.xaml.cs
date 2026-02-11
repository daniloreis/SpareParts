using SparePartsApp.Helpers;
using SparePartsApp.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Fotografia : ContentPage
    {
        public Fotografia(Material material)
        {
            InitializeComponent();

            imgFoto.Source = ImageSource.FromFile(material.Foto);

            //BindingContext = new FotografiaViewModel(material);
        }
    }

    public class FotografiaViewModel : ObservableObject
    {
        private ImageSource picture;

        public ImageSource Picture
        {
            get { return picture; }
            set
            {
                if (Equals(value, picture))
                    return;
                picture = value;
                OnPropertyChanged();
            }
        }
        public FotografiaViewModel(Material material)
        {
            Picture = material.Foto;

            ICameraProvider cameraProvider = DependencyService.Get<ICameraProvider>();

            Picture = cameraProvider.GetPicture(material.Foto);
        }
    }

}
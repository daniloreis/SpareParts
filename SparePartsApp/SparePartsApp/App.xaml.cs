using SparePartsApp.Helpers;
using SparePartsApp.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SparePartsApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
 
            MainPage = new NavigationPageCustom(new Login()); 
        }
    }
}

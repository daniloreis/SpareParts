using SparePartsApp.Helpers;
using SparePartsApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Geral
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Imagens : ContentPage
    {
        ICameraProvider cameraProvider = DependencyService.Get<ICameraProvider>();

        public Imagens()
        {
            InitializeComponent();

            var pics = cameraProvider.ListAllPictures().Select(x => Path.GetFileName(x));

            listView.ItemsSource = pics;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var service = new RecebimentoService();

            var fotos = cameraProvider.ListAllPictures();

            foreach (var foto in fotos)
            {
                service.FileUpload("EnviarFoto", foto);
            }
        }
    }
}
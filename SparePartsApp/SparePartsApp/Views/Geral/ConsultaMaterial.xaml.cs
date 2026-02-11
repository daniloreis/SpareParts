using SparePartsApp.Models;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Geral
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConsultaMaterial : ContentPage
    {
        public ConsultaMaterial()
        {
            InitializeComponent();
        }

        private void txtSap_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MaterialDetalhe());
        }

        private void txtBC_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ConsultaBC());
        }

        private void Nova_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Imagens());
        }
    }
}
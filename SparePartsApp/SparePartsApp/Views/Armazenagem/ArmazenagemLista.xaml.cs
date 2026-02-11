using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ArmazenagemLista : ContentPage
    {
        private MaterialService materialService { get; set; }

        public ArmazenagemLista()
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();

            ListarMateriais();
        }

        private async void ListarMateriais()
        {
            try
            {
                MaterialsListView.ItemsSource = await materialService.GetList("Pendentes");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void OnMaterialSelected(object sender, SelectedItemChangedEventArgs args)
        {
            try
            {
                var materialSelected = args.SelectedItem as Material;

                if (materialSelected == null) return;

                await Navigation.PushAsync(new ArmazenagemAbertura(materialSelected.CodMaterial));

                MaterialsListView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void MaterialsListView_Refreshing(object sender, EventArgs e)
        {
            try
            {
                ListarMateriais();
                MaterialsListView.EndRefresh();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
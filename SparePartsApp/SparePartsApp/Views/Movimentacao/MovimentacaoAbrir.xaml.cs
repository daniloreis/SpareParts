using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovimentacaoAbrir : ContentPage
    {
        private MaterialService materialService { get; set; }

        public MovimentacaoAbrir()
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();
        } 

        async void txtOrigem_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.OldTextValue != e.NewTextValue && e.NewTextValue?.Length > 2)
                {
                    txtOrigem.Text = e.NewTextValue;
                    CarregarLista();
                }
                else
                {
                    txtOrigem.Text = "";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        async void CarregarLista()
        {
            if (!string.IsNullOrWhiteSpace(txtOrigem.Text))
            {
                MaterialsListView.ItemsSource = await materialService.GetList($@"Armazenados/{txtOrigem.Text}");
            }
        }

        async void OnMaterialSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                var material = e.SelectedItem as Material;

                if (material == null) return;

                await Navigation.PushAsync(new MovimentacaoConfirmar(material));

                MaterialsListView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

    }
}
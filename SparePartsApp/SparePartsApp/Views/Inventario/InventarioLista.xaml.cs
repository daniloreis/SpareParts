using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventarioLista : ContentPage
    {
        private InventarioService inventarioService { get; set; }

        public InventarioLista(Inventario inventario)
        {
            InitializeComponent();

            if (inventarioService == null)
                inventarioService = new InventarioService();

            BindingContext = inventario;
        }

        private async void BarCodeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var entry = sender as Entry;

                if (e.OldTextValue != e.NewTextValue && e.NewTextValue.Length > 2)
                {
                    var material = entry.BindingContext as Material;

                    var txtQuantidade = entry.Parent.FindByName<Entry>("txtQuantidade");

                    var inventario = BindingContext as Inventario;

                    foreach (var item in inventario.Materiais)
                    {
                        if (e.NewTextValue == material.PosicaoDeposito)
                        {
                            item.PosicaoBipada = material.PosicaoDeposito;
                            txtQuantidade.IsEnabled = true;
                        }
                    }
                }

                entry.Text = "";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void MaterialsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MaterialsListView.SelectedItem = null;
        }

        private async void btnFinalizar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var inventario = BindingContext as Inventario;

                var materiais = inventario.Materiais;

                inventario.Usuario = Helper.UsuarioAutenticado;

                var retorno = await inventarioService.Inventariar(inventario);

                await DisplayAlert("Aviso", retorno, "OK");

                if (retorno.StartsWith("\"OK:") || retorno.StartsWith("OK:"))
                    await Navigation.PushAsync(new Principal());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void BarCodeEntry_Focused(object sender, FocusEventArgs e)
        {
            try
            {
                var entry = sender as Entry;

                var material = entry.BindingContext as Material;

                var inventario = BindingContext as Inventario;

                var txtQuantidade = entry.Parent.FindByName<Entry>("txtQuantidade");

                if (inventario.Materiais.Any(x => x.PosicaoBipada == material.PosicaoDeposito))
                    txtQuantidade.IsEnabled = true;

            }
            catch (Exception ex)
            {
                DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }
        
        private void TxtQuantidade_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var entry = sender as Entry;

                var material = entry.BindingContext as Material;

                if (!string.IsNullOrWhiteSpace(entry.Text))
                {
                    material.Quantidade = Convert.ToDouble(entry.Text);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
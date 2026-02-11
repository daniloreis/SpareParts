using SparePartsApp.Services;
using System;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InventarioAbertura : ContentPage
    {
        private InventarioService inventarioService { get; set; }
        public InventarioAbertura()
        {
            InitializeComponent();

            if (inventarioService == null)
                inventarioService = new InventarioService();
        }

        private void btnAbrir_Clicked(object sender, EventArgs e)
        {
            AbrirInventario();
        }

        private void txtDocumento_Completed(object sender, EventArgs e)
        {
            AbrirInventario();
        }

        private async void AbrirInventario()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDocumento.Text))
                {
                    await DisplayAlert("Aviso", "Informe o número do documento!", "OK");
                    txtDocumento.Focus();
                    return;
                }

                var numero = txtDocumento.Text;

                var intentario = await inventarioService.BuscarInventario(numero);

                var materiais = intentario.Materiais;

                if (materiais == null || !materiais.Any())
                {
                    await DisplayAlert("Aviso", "Lista não encontrada ou contagem já realizada!", "OK");
                    return;
                }

                await Navigation.PushAsync(new InventarioLista(intentario));

            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void txtDocumento_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                if (e.NewTextValue?.Length > 10)
                {
                    var entry = (sender as Entry);
                    entry.Text = e.OldTextValue;
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
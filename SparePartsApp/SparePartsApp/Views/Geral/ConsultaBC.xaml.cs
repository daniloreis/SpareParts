using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Geral
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConsultaBC : ContentPage
    {
        private Material Material { get; set; }
        private RecebimentoService listaRecebimentoService { get; set; }
        private MaterialService materialService { get; set; }

        public ConsultaBC()
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();

            if (listaRecebimentoService == null)
                listaRecebimentoService = new RecebimentoService();
        }
         
        private async void txtQRCode_Completed(object sender, EventArgs e)
        {
            await BuscarBC();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await BuscarBC();
        }

        private async Task BuscarBC()
        {
            try
            {
                if (string.IsNullOrEmpty(txtQRCode.Text))
                    return;

                Material = await materialService.Get($@"ConsultarBC/{txtQRCode.Text}");

                if (Material == null)
                {
                    await DisplayAlert("Aviso", "BC não encontrado no SAP!", "OK");
                    return;
                }

                BindingContext = Material;

                txtQRCode.Text = "";
                txtQRCode.Focus();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void btnImprimir_Clicked(object sender, EventArgs e)
        {
            try
            {
                var lista = new Recebimento();

                var material = BindingContext as Material;

                lista.Materiais.Add(material);

                await listaRecebimentoService.ImprimirMaterial(lista);

                await DisplayAlert("Aviso", "Etiqueta enviada para impressão!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
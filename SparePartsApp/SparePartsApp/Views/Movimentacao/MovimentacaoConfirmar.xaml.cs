using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MovimentacaoConfirmar : ContentPage
    {
        private MaterialService materialService { get; set; }

        private Material material { get; set; }
         

        public MovimentacaoConfirmar()
        {
            InitializeComponent();
        }

        public MovimentacaoConfirmar(Material material)
        {
            try
            {
                InitializeComponent();

                if (materialService == null)
                    materialService = new MaterialService();

                BindingContext = material;

                btnConfirmar.IsEnabled = false;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        async void txtDestino_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtDestino.Text?.Length > 2)
                {
                    btnConfirmar.IsEnabled = true;
                }
                else
                {
                    txtDestino.Text = "";
                    btnConfirmar.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnConfirmar_Clicked(object sender, EventArgs e)
        {
            Movimentar();
        }

        private void txtDestino_Completed(object sender, EventArgs e)
        {
            Movimentar();
        }

        private async void Movimentar()
        {
            try
            {
                var material = BindingContext as Material;

                material.PosicaoDeposito = txtDestino.Text;
                material.Usuario = Helper.UsuarioAutenticado;

                var retorno = await materialService.Movimentar(material);

                if (retorno.StartsWith("\"OK:") || retorno.StartsWith("OK:"))
                {
                    await DisplayAlert("Aviso", "Material movimentado com sucesso!", "OK");
                    await Navigation.PushAsync(new Principal());
                }
                else
                {
                    await DisplayAlert("Aviso", retorno, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
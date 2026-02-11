using SparePartsApp.Helpers;
using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.IO;
using Xamarin.Forms;

namespace SparePartsApp.Views
{
    public partial class RecebimentoDetalhe : ContentPage
    {
        private RecebimentoService listaRecebimentoService { get; set; }

        public RecebimentoDetalhe()
        {
            InitializeComponent();
        }

        public RecebimentoDetalhe(Material material)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(material.PosicaoDeposito))
                material.PosicaoDeposito = Helper.ListaRecebimento.PosicaoDeposito;

            if (listaRecebimentoService == null)
                listaRecebimentoService = new RecebimentoService();

            BindingContext = material;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var material = BindingContext as Material;
            var index = Helper.ListaRecebimento.Materiais.IndexOf(material);

            if (string.IsNullOrWhiteSpace(material.PosicaoDeposito))
                material.PosicaoDeposito = Helper.ListaRecebimento.PosicaoDeposito;

            btnVoltar.IsVisible = (index > 0);
            btnAvancar.IsVisible = (index < Helper.ListaRecebimento.Materiais.Count - 1);

            btnLotes.IsVisible = material.ExigeValidade;
        }

        private void btnLotes_Clicked(object sender, EventArgs e)
        {
            var material = BindingContext as Material;

            Navigation.PushAsync(new RecebimentoDetalheLote(material));
        }

        async void btnFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                ICameraProvider cameraProvider = DependencyService.Get<ICameraProvider>();

                var material = BindingContext as Material;

                if (string.IsNullOrWhiteSpace(material.Foto))
                {
                    var photoResult = await cameraProvider.TakePictureAsync();

                    if (photoResult != null)
                        material.Foto = Path.GetFileName(photoResult.FilePath);
                }
                else
                    await Navigation.PushAsync(new Fotografia(material));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnAvancar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var material = BindingContext as Material;
                var index = Helper.ListaRecebimento.Materiais.IndexOf(material);

                if (index + 1 < Helper.ListaRecebimento.Materiais.Count)
                {
                    var materialAtual = Helper.ListaRecebimento.Materiais[index + 1];

                    if (string.IsNullOrWhiteSpace(materialAtual.PosicaoDeposito))
                        materialAtual.PosicaoDeposito = Helper.ListaRecebimento.PosicaoDeposito;

                    BindingContext = materialAtual;

                    var indexAtual = Helper.ListaRecebimento.Materiais.IndexOf(materialAtual);

                    btnVoltar.IsVisible = (indexAtual > 0);
                    btnAvancar.IsVisible = (indexAtual < Helper.ListaRecebimento.Materiais.Count - 1);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnVoltar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var material = BindingContext as Material;
                var index = Helper.ListaRecebimento.Materiais.IndexOf(material);

                if (index - 1 > -1)
                {
                    var materialAtual = Helper.ListaRecebimento.Materiais[index - 1];

                    if (string.IsNullOrWhiteSpace(materialAtual.PosicaoDeposito))
                        materialAtual.PosicaoDeposito = Helper.ListaRecebimento.PosicaoDeposito;

                    BindingContext = materialAtual;

                    var indexAtual = Helper.ListaRecebimento.Materiais.IndexOf(materialAtual);

                    btnVoltar.IsVisible = (indexAtual > 0);
                    btnAvancar.IsVisible = (indexAtual < Helper.ListaRecebimento.Materiais.Count - 1);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnConcluir_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void btnAtivos_Clicked(object sender, EventArgs e)
        {
            var material = BindingContext as Material;

            if (string.IsNullOrWhiteSpace(material.QuantidadeEstoqueFisico))
            {
                DisplayAlert("Erro", "Informe a quantidade primeiro!", "OK");
                return;
            }

            Navigation.PushAsync(new RecebimentoDetalheAtivo(material));
        }
    }
}
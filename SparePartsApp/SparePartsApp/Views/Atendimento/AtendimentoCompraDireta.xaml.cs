using SparePartsApp.Helpers;
using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoCompraDireta : ContentPage
    {
        private AtendimentoService service;
        private MaterialService materialService;
        private ObservableRangeCollection<Material> materiais;

        public AtendimentoCompraDireta()
        {
            InitializeComponent();

            try
            {
                if (service == null)
                    service = new AtendimentoService();

                if (materialService == null)
                    materialService = new MaterialService();

                materiais = new ObservableRangeCollection<Material>();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }


        private async void txtQRCode_Completed(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtQRCode.Text))
                    return;

                if (materiais.Any(x => x.NumeroBC == txtQRCode.Text))
                    return;

                Material material = await materialService.Get($@"ConsultarBC/{txtQRCode.Text}");

                if (material == null)
                {
                    await DisplayAlert("Aviso", "BC não encontrado no SAP!", "OK");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(material.Deposito))
                {
                    await DisplayAlert("Aviso", $"Esse Material está armazenado no depósito {material.Deposito}, portanto não pode ser atendido por Compra Direta!", "OK");
                    return;
                }

                material.Usuario = Helper.UsuarioAutenticado;
                material.QuantidadeEstoqueSAP = material.Quantidade;
                material.QuantidadeRetirada = material.QuantidadeString;

                materiais.Add(material);

                MaterialsListView.ItemsSource = materiais;

                txtQRCode.Text = "";
                txtQRCode.Focus();
            }
            catch (Exception ex)
            {
                await DisplayAlert("ERRO", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnRemover_Clicked(object sender, EventArgs e)
        {
            try
            {
                var numeroBC = Convert.ToString(((Button)sender).CommandParameter);

                materiais.Remove(materiais.First(x => x.NumeroBC == numeroBC));

                MaterialsListView.ItemsSource = materiais;
            }
            catch (Exception ex)
            {
                DisplayAlert("ERRO", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void btnFinalizar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (materiais.Any(x => string.IsNullOrWhiteSpace(x.ImagemAssinatura)))
                {
                    await Navigation.PushModalAsync(new AtendimentoReservaAssinatura(materiais));
                }
                else
                {
                    materiais.ForEach(x => x.Usuario = Helper.UsuarioAutenticado);

                    var retorno = await service.AtenderCompraDireta(materiais);

                    if (retorno.StartsWith("\"OK") || retorno.StartsWith("OK"))
                    {
                        await DisplayAlert("Aviso", "Atendimento finalizado com sucesso!", "OK");
                        await Navigation.PushAsync(new Principal());
                    }
                    else
                        await DisplayAlert("Aviso", retorno, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("ERRO", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void BtnAdicionar_Clicked(object sender, EventArgs e)
        {
            txtQRCode_Completed(sender, e);
        }

    }
}
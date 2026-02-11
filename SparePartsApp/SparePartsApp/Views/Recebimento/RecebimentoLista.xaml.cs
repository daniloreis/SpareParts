using System;
using Xamarin.Forms;
using SparePartsApp.Helpers;
using SparePartsApp.Models;
using System.Linq;
using SparePartsApp.Services;
using System.Text;
using System.Collections.Generic;

namespace SparePartsApp.Views
{
    public partial class RecebimentoLista : ContentPage
    {
        private RecebimentoService listaRecebimentoService { get; set; }

        private bool recontou = false;

        public RecebimentoLista()
        {
            InitializeComponent();

            BindingContext = Helper.ListaRecebimento;

            if (listaRecebimentoService == null)
                listaRecebimentoService = new RecebimentoService();

            btnEntradaMercadoria.IsEnabled = true;
        }

        async void OnMaterialSelected(object sender, SelectedItemChangedEventArgs args)
        {
            try
            {
                var materialSelected = args.SelectedItem as Material;

                if (materialSelected == null) return;

                await Navigation.PushAsync(new RecebimentoDetalhe(materialSelected));

                MaterialsListView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        async void btnFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                var material = (Material)((Button)sender).BindingContext;

                ICameraProvider cameraProvider = DependencyService.Get<ICameraProvider>();

                if (string.IsNullOrWhiteSpace(material.Foto))
                {
                    var photoResult = await cameraProvider.TakePictureAsync();

                    if (photoResult != null)
                        material.Foto = photoResult.FilePath;
                }
                else
                    await Navigation.PushAsync(new Fotografia(material));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        async void btnEntradaMercadoria_Clicked(object sender, EventArgs e)
        {
            try
            {
                short qtdDivergencia = 0, qtdNaoInformada = 0, qtdExigeValidade = 0;

                #region Validações

                if (Helper.ListaRecebimento.Materiais != null && Helper.ListaRecebimento.Materiais.Any())
                {
                    foreach (var material in Helper.ListaRecebimento.Materiais)
                    {
                        if (material.quantidadeEstoqueFisico.HasValue && material.quantidadeEstoqueFisico > 0 && material.quantidadeEstoqueFisico != material.Quantidade) //divergência quantitativa
                        {
                            qtdDivergencia++;
                            material.CorTexto = "Red";
                            material.TemDivergencia = true;
                        }
                        else
                            material.TemDivergencia = false;

                        if (!material.quantidadeEstoqueFisico.HasValue)
                            qtdNaoInformada++;

                        if (material.ExigeValidade)
                        {
                            if (!material.Lotes.Any())
                                qtdExigeValidade++;
                        }
                    }
                }

                if (qtdNaoInformada > 0)
                {
                    await DisplayAlert("Aviso", "Informe todas as quantidades!", "OK");
                    qtdNaoInformada = 0;
                    return;
                }

                if (qtdExigeValidade > 0)
                {
                    await DisplayAlert("Aviso", "Informe os lotes para os materiais em azul!", "OK");
                    qtdExigeValidade = 0;
                    return;
                }

                if (qtdDivergencia > 0 && !recontou)
                {
                    await DisplayAlert("Aviso", "Os materiais em vermelho possuem divergência na quantidade. Favor recontar!", "OK");
                    recontou = true;
                    foreach (var material in Helper.ListaRecebimento.Materiais)
                    {
                        if (material.quantidadeEstoqueFisico.HasValue && material.quantidadeEstoqueFisico != material.Quantidade)
                        {
                            material.QuantidadeEstoqueFisico = null;

                            if (material.ExigeValidade && material.Lotes.Any())
                            {
                                material.Lotes.Clear();
                            }
                        }
                    }

                    return;
                }

                #endregion

                if (qtdDivergencia == 0 || recontou)
                {
                    foreach (var material in Helper.ListaRecebimento.Materiais)
                    {
                        if (!string.IsNullOrWhiteSpace(material.Foto))
                            material.TemDivergencia = true;

                        if (string.IsNullOrWhiteSpace(material.PosicaoDeposito))
                            material.PosicaoDeposito = Helper.ListaRecebimento.PosicaoDeposito;

                        material.CodLista = Helper.ListaRecebimento.CodLista;
                        material.Usuario = Helper.UsuarioAutenticado;
                    }

                    btnEntradaMercadoria.IsEnabled = false;

                    var retorno = await listaRecebimentoService.CriarMigo(Helper.ListaRecebimento);

                    await DisplayAlert("Aviso", retorno.Message, "OK");

                    if (retorno.Message.StartsWith("\"OK:") || retorno.Message.StartsWith("OK:"))
                    {
                        Helper.ListaRecebimento.NumeroDocumento = retorno.Message.Split(':')[1];

                        await Navigation.PushAsync(new RecebimentoImpressao(retorno.Result));
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
            finally
            {
                btnEntradaMercadoria.IsEnabled = true;
            }
        }
    }
}

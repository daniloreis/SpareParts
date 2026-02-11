using System;
using SparePartsApp.Models;
using Xamarin.Forms;
using SparePartsApp.Services;
using System.Linq;

namespace SparePartsApp.Views
{
    public partial class RecebimentoVolumes : ContentPage
    {
        private RecebimentoService recebimentoService { get; set; }

        public RecebimentoVolumes()
        {
            InitializeComponent();

            if (recebimentoService == null)
                recebimentoService = new RecebimentoService();

            Helper.ListaRecebimento = new Recebimento();
        }


        private async void txtListaRecebimento_Completed(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtListaRecebimento.Text))
                {
                    await DisplayAlert("Aviso", "Informe um número de documento!", "OK");
                    txtListaRecebimento.Focus();
                    return;
                }

                Helper.ListaRecebimento = await recebimentoService.Get(txtListaRecebimento.Text);

                if (Helper.ListaRecebimento != null && Helper.ListaRecebimento.Materiais.Any())
                {
                    lblFornecedor.Text = Helper.ListaRecebimento?.Fornecedor;
                    lblNfe.Text = Helper.ListaRecebimento?.NumeroNfe;
                    txtVolumes.Text = Helper.ListaRecebimento.NumeroVolumes?.ToString();

                    txtPosicaoDeposito.Focus();
                }
                else
                {
                    await DisplayAlert("Aviso", "Lista não encontrada ou já lançada no SAP!", "OK");
                    txtListaRecebimento.Focus();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
                txtListaRecebimento.Focus();
            }
        }

        private void btnValidarVolumes_Clicked(object sender, EventArgs e)
        {
            if (ValidarVolumes())
                Navigation.PushAsync(new RecebimentoLista());
        }

        private bool ValidarVolumes()
        {
            try
            {
                if (Helper.ListaRecebimento == null || !Helper.ListaRecebimento.Materiais.Any())
                {
                    DisplayAlert("Aviso", "A lista não está carregada!", "OK");
                    txtListaRecebimento.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtPosicaoDeposito.Text))
                {
                    DisplayAlert("Aviso", "Informe um local de descarga!", "OK");
                    txtPosicaoDeposito.Focus();
                    return false;
                }

                Helper.ListaRecebimento.CodLista = txtListaRecebimento.Text;
                Helper.ListaRecebimento.PosicaoDeposito = txtPosicaoDeposito.Text;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }

            return true;
        }

        async void btnImprimirVolumes_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (ValidarVolumes())
                {
                    Helper.ListaRecebimento.NumeroVolumes = Convert.ToInt32(txtVolumes.Text);

                    await recebimentoService.ImprimirVolumes(Helper.ListaRecebimento);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void BtnOk_Clicked(object sender, EventArgs e)
        {
            txtListaRecebimento_Completed(sender, e);
        }
    }
}
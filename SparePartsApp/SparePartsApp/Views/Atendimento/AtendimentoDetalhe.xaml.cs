using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoDetalhe : ContentPage
    {
        private IList<Material> materiaisOrdenados;

        public AtendimentoDetalhe()
        {
            InitializeComponent();
        }

        public AtendimentoDetalhe(IEnumerable<Material> materiais)
        {
            InitializeComponent();

            try
            {
                materiaisOrdenados = materiais.OrderBy(x => x.PosicaoDeposito).ToList();

                BindingContext = materiaisOrdenados.First();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ConfigurarTela();
        }

        private void ConfigurarTela()
        {
            try
            {
                var material = BindingContext as Material;
                var index = materiaisOrdenados.IndexOf(material);

                btnVoltar.IsVisible = (index > 0);
                btnAvancar.IsVisible = (index < materiaisOrdenados.Count - 1);

                txtQuantidadeRetirada.IsEnabled = txtQuantidade.IsEnabled = (material.PosicaoDeposito == material.PosicaoBipada);

                if (material.Depositos.Count == 1)
                {
                    pkrDeposito.SelectedItem = material.Depositos.First();
                    pkrDeposito.IsEnabled = false;
                }
                else
                {
                    pkrDeposito.SelectedItem = material.Depositos.FirstOrDefault(x => x.NumeroLote == material.NumeroLote) ?? material.Depositos.FirstOrDefault(x => x.Nome == material.Deposito) ?? material.Depositos.First();
                    pkrDeposito.IsEnabled = true;
                }

                bool comReserva = !string.IsNullOrWhiteSpace(material.NumeroReserva);

                lblTxtReservada.IsVisible = lblReservada.IsVisible = comReserva;

                if (comReserva) lblRetirar.Text = "RETIRAR:"; else lblRetirar.Text = "TRANSFERIR:";

            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
        
        private void btnAvancar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var material = BindingContext as Material;
                var index = materiaisOrdenados.IndexOf(material);

                if (index + 1 < materiaisOrdenados.Count)
                {
                    var materialAtual = materiaisOrdenados[index + 1];

                    BindingContext = materialAtual;

                    var indexAtual = materiaisOrdenados.IndexOf(materialAtual);

                    btnVoltar.IsVisible = (indexAtual > 0);
                    btnAvancar.IsVisible = (indexAtual < materiaisOrdenados.Count - 1);
                }

                ConfigurarTela();
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
                var index = materiaisOrdenados.IndexOf(material);

                if (index - 1 > -1)
                {
                    var materialAtual = materiaisOrdenados[index - 1];

                    BindingContext = materialAtual;

                    var indexAtual = materiaisOrdenados.IndexOf(materialAtual);

                    btnVoltar.IsVisible = (indexAtual > 0);
                    btnAvancar.IsVisible = (indexAtual < materiaisOrdenados.Count - 1);
                }

                ConfigurarTela();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var material = BindingContext as Material;
                txtQuantidade.IsEnabled = (material.PosicaoDeposito == material.PosicaoBipada);
                txtQuantidadeRetirada.IsEnabled = txtQuantidade.IsEnabled;

                var entry = sender as Entry;

                materiaisOrdenados.Where(x => x.PosicaoDeposito == entry.Text).ForEach(x => x.PosicaoBipada = entry.Text);
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void pkrDeposito_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var deposito = ((Picker)sender).SelectedItem as Deposito;

                if (deposito != null)
                {
                    txtEndereco.Placeholder = deposito.PosicaoDeposito;

                    lblValidade.Text = deposito.Validade?.ToString("dd/MM/yyyy");
                    lblTxtValidade.IsVisible = !string.IsNullOrWhiteSpace(lblValidade.Text);

                    lblQtdeSAP.Text = deposito.QuantidadeEstoqueSAP?.ToString();

                    var material = BindingContext as Material;

                    bool comReserva = !string.IsNullOrWhiteSpace(material.NumeroReserva);

                    lblTxtReservada.IsVisible = lblReservada.IsVisible = comReserva;

                    if (comReserva) lblRetirar.Text = "A RETIRAR"; else lblRetirar.Text = "TRANSFERIR";

                    material.Deposito = deposito.Nome;
                    material.NumeroLote = deposito.NumeroLote;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnConcluir_Clicked(object sender, EventArgs e)
        {
            try
            {
                var criticos = materiaisOrdenados.Where(x => x.Critico && !x.quantidadeEstoqueFisico.HasValue);

                if (criticos.Any())
                {
                    var msg = $"Os materiais abaixo exigem contagem de estoque físico pois são críticos:\n\n{string.Join(Environment.NewLine, criticos.GroupBy(x => x.CodMaterial).Select(x => x.First()).Select(x => x.CodMaterial))}";

                    DisplayAlert("Aviso", msg, "OK");
                    return;
                }

                if (materiaisOrdenados.Any(x => !x.quantidadeRetirada.HasValue))
                {
                    DisplayAlert("Aviso", "Informe a quantidade a retirar de todos os materiais.", "OK");
                    return;
                }

                if (materiaisOrdenados.Any(x => !string.IsNullOrWhiteSpace(x.NumeroReserva) && x.quantidadeRetirada > x.QuantidadeDisponivel))
                {
                    DisplayAlert("Aviso", "A quantidade a retirar não pode ser maior que o saldo disponível na reserva.", "OK");
                    return;
                }

                Navigation.PushAsync(new AtendimentoFinalizar(materiaisOrdenados));

            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

    }
}
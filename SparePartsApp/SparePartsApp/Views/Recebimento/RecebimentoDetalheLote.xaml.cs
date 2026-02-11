using SparePartsApp.Models;
using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecebimentoDetalheLote : ContentPage
    {
        public RecebimentoDetalheLote()
        {
            InitializeComponent();
        }
        public RecebimentoDetalheLote(Material material)
        {
            InitializeComponent();

            BindingContext = material;

            dtpValidade.MinimumDate = DateTime.Today;

            LotesListView.ItemsSource = material.Lotes;

            txtLote.Focus();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ConfigDatePicker();
        }

        private void btnIncluir_Clicked(object sender, EventArgs e)
        {
            IncluirLote();
        }

        private void IncluirLote()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLote.Text) || string.IsNullOrWhiteSpace(txtQuantidade.Text))
                {
                    DisplayAlert("Aviso", "Informe o lote e a quantidade!", "OK");
                    return;
                }

                var material = BindingContext as Material;

                if (material.ExigeValidade && dtpValidade.Date <= DateTime.Today)
                {
                    DisplayAlert("Aviso", "Informe a data de validade!", "OK");
                    return;
                }

                material.Lotes.Add(new Lote()
                {
                    CodMaterial = material.CodMaterial,
                    Deposito = material.Deposito,
                    NumeroLote = txtLote.Text,
                    Quantidade = Convert.ToDouble(txtQuantidade.Text),
                    Validade = material.ExigeValidade ? (DateTime?)dtpValidade.Date : null,
                });

                LotesListView.ItemsSource = material.Lotes;

                var qtdTotal = material.Lotes.Sum(x => x.Quantidade ?? 0);

                material.QuantidadeEstoqueFisico = qtdTotal == 0 ? "" : qtdTotal.ToString();


                //limpar componentes
                txtLote.Text = null;
                txtQuantidade.Text = null;
                dtpValidade.Date = DateTime.Today;
                txtLote.Focus();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnRemover_Clicked(object sender, EventArgs e)
        {
            try
            {
                var numero = Convert.ToString(((Button)sender).CommandParameter);

                var material = BindingContext as Material;

                var lote = material.Lotes.FirstOrDefault(x => x.NumeroLote.Equals(numero));

                if (lote != null)
                    material.Lotes.Remove(lote);

                var qtdTotal = material.Lotes.Sum(x => x.Quantidade ?? 0);

                material.QuantidadeEstoqueFisico = qtdTotal == 0 ? "" : qtdTotal.ToString();

                LotesListView.ItemsSource = material.Lotes;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void txtQuantidade_Completed(object sender, EventArgs e)
        {
            IncluirLote();
        }

        private void txtLote_TextChanged(object sender, TextChangedEventArgs e)
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
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void txtQuantidade_TextChanged()
        {
            try
            {
                txtQuantidade.Text = Convert.ToDouble(txtQuantidade.Text).ToString();
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void txtLote_Completed(object sender, EventArgs e)
        {
            txtQuantidade.Focus();
        }

        private void dtpValidade_PropertyChanged(object sender, EventArgs e)
        {
            ConfigDatePicker();
        }

        private void ConfigDatePicker()
        {
            if (dtpValidade.Date.Equals(DateTime.Today))
                dtpValidade.TextColor = Color.Transparent;
            else
                dtpValidade.TextColor = Color.Black;
        }
    }
}
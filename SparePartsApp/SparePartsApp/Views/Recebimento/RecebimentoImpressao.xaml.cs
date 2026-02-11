using SparePartsApp.Models;
using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;
using SparePartsApp.Helpers;
using System;
using SparePartsApp.Services;

namespace SparePartsApp.Views
{
    public partial class RecebimentoImpressao : ContentPage
    {
        private RecebimentoService listaRecebimentoService { get; set; }
        private ObservableRangeCollection<Material> _materiais = new ObservableRangeCollection<Material>();

        public RecebimentoImpressao(List<Material> materiais)
        {
            InitializeComponent();

            if (listaRecebimentoService == null)
                listaRecebimentoService = new RecebimentoService();

            LotesListView.ItemsSource = materiais;
            _materiais.AddRange(materiais);
        }

        async void btnImprimir_Clicked(object sender, EventArgs e)
        {
            try
            {
                await listaRecebimentoService.ImprimirMaterial(new Recebimento { Materiais = _materiais });

                await DisplayAlert("Aviso", "Etiquetas enviadas para impressão!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void txtQuantidade_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue?.Length > 7)
            {
                var entry = (sender as Entry);
                entry.Text = e.OldTextValue;
                return;
            }
        }

        async void btnFinalizar_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Principal());
        }
    }
}

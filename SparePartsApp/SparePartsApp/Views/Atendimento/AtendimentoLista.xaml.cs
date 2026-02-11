using SparePartsApp.Helpers;
using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoLista : ContentPage
    {
        private AtendimentoService service { get; set; }
        private IEnumerable<Material> Materiais { get; set; }
        public ObservableRangeCollection<Material> Selecionados { get; set; } = new ObservableRangeCollection<Material>();

        public AtendimentoLista(IEnumerable<Material> materiais)
        {
            InitializeComponent();

            try
            {
                if (service == null)
                    service = new AtendimentoService();

                Materiais = materiais;

                var mats = materiais.GroupBy(x => x.CodMaterial).Select(x => x.First());

                Selecionados.AddRange(mats);

                MaterialsListView.ItemsSource = Selecionados;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void MaterialsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MaterialsListView.SelectedItem = null;
        }

        private async void btnAvancar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var materiais = Materiais.Where(m => Selecionados.Any(x => x.CodMaterial == m.CodMaterial && x.Selecionado));

                if (!materiais.Any())
                {
                    await DisplayAlert("Aviso", "Selecione pelo menos um material para realizar o atendimento.", "OK");
                    return;
                }

                var pendentes = materiais.Where(x => x.Deposito == "DM15");

                if (pendentes.Any())
                {
                    await DisplayAlert("Aviso", "Os materiais abaixo estão pendentes de armazenagem:\n\n" + string.Join(Environment.NewLine, pendentes.Select(x => x.CodMaterial + " " + x.NumeroLote)), "OK");
                    return;
                }

                var divergentesDaReserva = materiais.Where(x => !string.IsNullOrWhiteSpace(x.NumeroReserva) && x.NaoPodeAtender);

                if (divergentesDaReserva.Any())
                {
                    foreach (var item in divergentesDaReserva)
                    {
                        materiais = materiais.Where(x => divergentesDaReserva.Any(y => y.CodMaterial != x.CodMaterial));
                    }

                    await service.Post(divergentesDaReserva, "SalvarDivergentes");

                    await DisplayAlert("Aviso", "Os materiais abaixo não poderão ser atendidos, pois não cumprem os requisitos de quantidade ou validade:\n\n" + string.Join(Environment.NewLine, divergentesDaReserva.GroupBy(x => x.CodMaterial).Select(x => x.First()).Select(x => x.CodMaterial)), "OK");
                }

                if (!materiais.Any())
                {
                    return;
                }

                await Navigation.PushAsync(new AtendimentoListaLote(materiais));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }

}

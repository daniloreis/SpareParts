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
    public partial class AtendimentoListaLote : ContentPage
    {
        private AtendimentoService atendimentoService { get; set; } = new AtendimentoService();

        public AtendimentoListaLote(IEnumerable<Material> materiais)
        {
            try
            {
                InitializeComponent();

                if (materiais.Any(x => !string.IsNullOrWhiteSpace(x.NumeroReserva)))
                    materiais.ToList().ForEach(x => x.Selecionado = true);

                MaterialsListView.ItemsSource = materiais;
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
                var lista = MaterialsListView.ItemsSource as IEnumerable<Material>;

                var materiais = lista.Where(x => x.Selecionado);

                if (!materiais.Any())
                {
                    DisplayAlert("Aviso", "Selecione pelo menos um material para realizar o atendimento.", "OK");
                    return;
                }

                var pendentes = materiais.Where(x => x.Deposito == "DM15");

                if (pendentes.Any())
                {
                    DisplayAlert("Aviso", "Os materiais abaixo estão pendentes de armazenagem:\n\n" + string.Join(Environment.NewLine, pendentes.Select(x => x.CodMaterial + " " + x.NumeroLote)), "OK");
                    return;
                }

                Navigation.PushAsync(new AtendimentoDetalhe(materiais));

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
    }
}
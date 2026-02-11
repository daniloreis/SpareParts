using SparePartsApp.Models;
using System;
using System.Linq;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SparePartsApp.Views
{
    public partial class RecebimentoDetalheAtivo : ContentPage
    {
        public RecebimentoDetalheAtivo(Material material)
        {
            InitializeComponent();

            BindingContext = material;

            if (material.quantidadeEstoqueFisico.HasValue)
            {
                material.Ativos = new List<Ativo>();

                for (int i = 1; i <= material.quantidadeEstoqueFisico; i++)
                {
                    material.Ativos.Add(new Ativo { Sequencia = i, Placa = "" });
                }
            }

            MaterialsListView.ItemsSource = material.Ativos;

            btnConcluir.IsEnabled = false;        }

        private async void BtnConcluir_Clicked(object sender, EventArgs e)
        {
            var material = (Material)BindingContext;

            if (material.Ativos.Any(a => string.IsNullOrWhiteSpace(a.Placa)))
            {
                await DisplayAlert("Erro", "Informe todas as placas de ativos!", "OK");
            }
            else
                await Navigation.PopAsync();
        }

        private void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = sender as Entry;
            var material = (Material)BindingContext;

            var seq = Convert.ToInt32(entry.Parent.FindByName<Label>("lblSequencia").Text);

            material.Ativos.Find(x => x.Sequencia == seq).Placa = entry.Text;

            MaterialsListView.ItemsSource = material.Ativos;

            if (!material.Ativos.Any(a => string.IsNullOrWhiteSpace(a.Placa)))
            {
                btnConcluir.IsEnabled = true;
            }
        }
    }
}

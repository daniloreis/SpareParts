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
    public partial class AtendimentoFinalizar : ContentPage
    {
        private bool porReserva = false;
        public bool PorReserva { get { return porReserva; } set { porReserva = value; } }

        private AtendimentoService service { get; set; }

        public AtendimentoFinalizar(IEnumerable<Material> materiais)
        {
            try
            {
                InitializeComponent();
                MaterialsListView.ItemsSource = materiais;

                if (service == null)
                    service = new AtendimentoService();

                PorReserva = materiais.Any(x => !string.IsNullOrWhiteSpace(x.NumeroReserva));

                lblReserva.IsVisible = porReserva;

                if (porReserva) lblRetirar.Text = "RETIRAR"; else lblRetirar.Text = "TRANSF";
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void btnFinalizar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var materiais = MaterialsListView.ItemsSource as IEnumerable<Material>;

                if (materiais.Any(x => !string.IsNullOrWhiteSpace(x.NumeroReserva) && x.quantidadeRetirada > x.QuantidadeDisponivel))
                {
                    await DisplayAlert("Aviso", "A quantidade a retirar não pode ser maior que o saldo disponível na reserva.", "OK");
                    return;
                }

                if (porReserva && materiais.Any(x => string.IsNullOrWhiteSpace(x.ImagemAssinatura)))
                {
                    await Navigation.PushModalAsync(new AtendimentoReservaAssinatura(materiais));
                }
                else
                {
                    materiais.ForEach(x => x.Usuario = Helper.UsuarioAutenticado);

                    var retorno = await service.Atender(materiais);

                    if (retorno.StartsWith("\"OK:") || retorno.StartsWith("OK:"))
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
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void MaterialsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MaterialsListView.SelectedItem = null;
        }
    }
}
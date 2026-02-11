using SparePartsApp.Services;
using SparePartsApp.Views.Atendimento;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoAbertura : ContentPage
    {
        private AtendimentoService service { get; set; } = new AtendimentoService();

        public AtendimentoAbertura()
        {
            InitializeComponent();
        }
         
        private async void btnComReserva_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AtendimentoReserva());
        }

        private async void btnPorTransferencia_Clicked(object sender, EventArgs e)
        {
            try
            {
                var materiais = await service.BuscarTransferencia();

                if (!materiais.Any())
                {
                    await DisplayAlert("Aviso", "Não há material disponível para transferência.", "OK");
                    return;
                }

                await Navigation.PushAsync(new AtendimentoLista(materiais));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void txtNumeroReserva_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue?.Length > 10 || e.NewTextValue.Contains(","))
            {
                (sender as Entry).Text = e.OldTextValue;
                return;
            }
        }

        private void txtNumeroReserva_Completed(object sender, EventArgs e)
        {
            btnComReserva.Focus();
        }

        private async void btnCompraDireta_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AtendimentoCompraDireta());
        }

        private async void btnVisitante_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AtendimentoVisitante());
        }
    }
}
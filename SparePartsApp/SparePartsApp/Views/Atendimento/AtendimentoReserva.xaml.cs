using SparePartsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AtendimentoReserva : ContentPage
	{
        private AtendimentoService service { get; set; } = new AtendimentoService();

        public AtendimentoReserva ()
		{
			InitializeComponent ();
		}

        private async void BtnAvancar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNumeroReserva.Text))
                {
                    await DisplayAlert("Aviso", "Informe o número da reserva!", "Fechar");
                    txtNumeroReserva.Focus();
                    return;
                }

                var materiais = await service.BuscarReserva(txtNumeroReserva.Text);

                if (materiais == null || !materiais.Any())
                {
                    await DisplayAlert("Aviso", "Essa reserva não possui materiais ou não existe no SAP!", "Fechar");
                    return;
                }

                await Navigation.PushAsync(new AtendimentoLista(materiais));

            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
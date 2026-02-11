using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoVisitante : ContentPage
    {
        private Material acompanhamento;
        private AtendimentoService service { get; set; }
        private List<Material> materiais;

        public AtendimentoVisitante()
        {
            InitializeComponent();

            try
            {
                if (service == null)
                    service = new AtendimentoService();

                materiais = new List<Material>();

                acompanhamento = new Material();

                IniciarAtendimento();

                materiais.Add(acompanhamento);

                if (string.IsNullOrWhiteSpace(acompanhamento.ImagemAssinatura))
                {
                    Navigation.PushModalAsync(new AtendimentoReservaAssinatura(materiais));
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void IniciarAtendimento()
        {
            try
            {
                var dataHora = await service.IniciarVisita();

                acompanhamento.DataHoraInicio = Convert.ToDateTime(dataHora);
                acompanhamento.Usuario = Helper.UsuarioAutenticado;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void PkrMotivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                acompanhamento.MotivoAcompanhamento = pkrMotivo.SelectedItem as string;
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private async void BtnFinalizar_Clicked(object sender, EventArgs e)
        {
            try
            {
                btnFinalizar.IsEnabled = false;

                if (string.IsNullOrWhiteSpace(acompanhamento.ImagemAssinatura))
                {
                    await Navigation.PushModalAsync(new AtendimentoReservaAssinatura(materiais));
                    return;
                }

                if (string.IsNullOrWhiteSpace(acompanhamento.MotivoAcompanhamento))
                {
                    await DisplayAlert("Aviso", "Selecione um motivo!", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCodMaterial.Text))
                {
                    await DisplayAlert("Aviso", "Informe o código do material!", "OK");
                    return;
                }

                acompanhamento.CodMaterial = txtCodMaterial.Text;

                var resultado = await service.AtenderVisitante(materiais);

                if (resultado == "OK")
                {
                    await DisplayAlert("Aviso", "Atendimento finalizado com sucesso!", "OK");
                    await Navigation.PushAsync(new Principal());
                }
                else
                    await DisplayAlert("Aviso", resultado, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
            finally
            {
                btnFinalizar.IsEnabled = true;
            }
        }
    }
}
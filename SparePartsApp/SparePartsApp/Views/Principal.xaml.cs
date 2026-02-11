using SparePartsApp.Views.Geral;
using System;
using Xamarin.Forms;

namespace SparePartsApp.Views
{
    public partial class Principal : ContentPage
    {
        public Principal()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
            SAIR.Text += $" ({Helper.UsuarioAutenticado})";
            MessagingCenter.Send((App)Application.Current, "HideLoading");
        }

        private void RECEBIMENTO_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new RecebimentoVolumes());
        }

        private void ARMAZENAGEM_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ArmazenagemAbertura());
        }

        private void MOVIMENTACAO_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MovimentacaoAbrir());
        }

        private void INVENTARIO_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new InventarioAbertura());
        }

        private void ATENDIMENTO_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AtendimentoAbertura());
        }

        private void SAIR_Clicked(object sender, EventArgs e)
        {
            Helper.UsuarioAutenticado = null;

            Navigation.InsertPageBefore(new Login(), this);
            Navigation.PopToRootAsync();
        }

        private void MATERIALDETALHE_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ConsultaMaterial());
        }
    }
}
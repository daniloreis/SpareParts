//#define HOMOLOG
using SparePartsApp.Helpers;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using SparePartsApp.Models;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        private AutenticacaoService autenticacaoService { get; set; }
        private IUpdateProvider updateProvider;

        public Login()
        {
            InitializeComponent();

            updateProvider = DependencyService.Get<IUpdateProvider>();

            if (autenticacaoService == null)
                autenticacaoService = new AutenticacaoService();

            pkrServidor.ItemsSource = Helper.Servidores;

#if !DEBUG
        pkrServidor.SelectedItem = Helper.Servidores.Last();
#else
        pkrServidor.SelectedItem = Helper.Servidores.First();
#endif

            btnAtualizar.Text = "Versão: " + updateProvider.GetVersion();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            txtUsuario.Text = "";
            txtSenha.Text = "";
            txtUsuario.Focus();

            updateProvider.CheckVersion();
        }

        private async void btnEntrar_Clicked(object sender, EventArgs e)
        {
            try
            {
                var servidor = pkrServidor.SelectedItem as Tuple<string, string>;

                if (servidor != null)
                    Helper.Server = servidor.Item2;

#if DEBUG
                var retorno = "OK:debug";
#else
     
                if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
                {
                    await DisplayAlert("Aviso", "Informe o usuário e a senha!", "OK");
                    return;
                }

                btnEntrar.IsEnabled = false;

                var usuario = new Usuario(txtUsuario.Text, txtSenha.Text);

                var retorno = await autenticacaoService.Autenticar(usuario);           
#endif

                if (retorno.StartsWith("\"OK:") || retorno.StartsWith("OK:"))
                {
                    Helper.UsuarioAutenticado = retorno.Split(':')[1];

                    await Navigation.PushAsync(new Principal());
                }
                else
                {
                    await DisplayAlert("Aviso", retorno, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Aviso", $"Usuário ou senha incorretos: {ex.GetInnerExceptionMessage()}", "OK");
            }
            finally
            {
                try
                {
                    btnEntrar.IsEnabled = true;
                    MessagingCenter.Send((App)Application.Current, "HideLoading");
                }
                catch { }
            }
        }

        private void btnAtualizar_Clicked(object sender, EventArgs e)
        {
            updateProvider.CheckUpdate();
        }

        private void txtSenha_Completed(object sender, EventArgs e)
        {
            btnEntrar.Focus();
        }

        private void PkrServidor_SelectedIndexChanged(object sender, EventArgs e)
        {
            var servidor = pkrServidor.SelectedItem as Tuple<string, string>;

            Helper.Server = servidor.Item2;
        }
    }
}
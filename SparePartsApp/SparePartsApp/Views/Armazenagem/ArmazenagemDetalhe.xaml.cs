using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ArmazenagemDetalhe : ContentPage
    {
        private MaterialService materialService { get; set; }

        private bool recontou = false;
         
        public ArmazenagemDetalhe(Material model)
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();

            BindingContext = model;

            txtQuantidadeEstoqueFisico.IsEnabled = false;
        }

        async void txtPosicaoDeposito_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.NewTextValue.Length > 2)
                {
                    var material = BindingContext as Material;

                    btnArmazenar.IsEnabled = txtQuantidadeEstoqueFisico.IsEnabled = (txtPosicaoDeposito.Text == material.PosicaoDeposito);

                    if (txtPosicaoDeposito.Text != material.PosicaoDeposito)
                    {
                        await DisplayAlert("Aviso", "Só é permitido armazenar na posição sugerida!", "OK");
                        txtPosicaoDeposito.Text = "";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private bool Validar()
        {
            try
            {
                var material = BindingContext as Material;

                if (material.QuantidadeEstoqueSAP != material.quantidadeEstoqueFisico && !recontou)
                {
                    recontou = true;
                    txtQuantidadeEstoqueFisico.Text = "";
                    DisplayAlert("Aviso", "Quantidade em estoque diverge da quantidade no SAP.\nFavor recontar!", "OK");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(material.Deposito))
                {
                    DisplayAlert("Aviso", "Informe o depósito!", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
            return true;
        }

        async void btnArmazenar_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!Validar())
                    return;

                if (string.IsNullOrWhiteSpace(txtPosicaoDeposito.Text))
                {
                    await DisplayAlert("Aviso", "Informe a posição a armazenar!", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtQuantidadeEstoqueFisico.Text))
                {
                    await DisplayAlert("Aviso", "Informe a quantidade do estoque atual!", "OK");
                    return;
                }

                var material = BindingContext as Material;

                if (!string.IsNullOrWhiteSpace(material.CondicaoArmazenagem) || !string.IsNullOrWhiteSpace(material.CondicaoTemperatura))
                {
                    var msg = $@"Confirma armazenamento nas seguintes condições: {Environment.NewLine}
                                 {material.CondicaoArmazenagem}{Environment.NewLine}
                                 {material.CondicaoTemperatura}";

                    var confirmou = await DisplayAlert("Aviso", msg, "Sim", "Cancelar");

                    if (!confirmou)
                        return;
                }

                material.QuantidadeEstoqueFisico = txtQuantidadeEstoqueFisico.Text;
                material.Usuario = Helper.UsuarioAutenticado;

                var retorno = await materialService.Post(material, "Armazenar");

                if (retorno.StartsWith("\"OK:") || retorno.StartsWith("OK:"))
                {
                    await DisplayAlert("Aviso", "Material armazenado com sucesso!", "OK");
                    await Navigation.PushAsync(new Principal());
                }
                else
                {
                    await DisplayAlert("Aviso", retorno, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void PkrDeposito_SelectedIndexChanged(object sender, EventArgs e)
        {
            var material = BindingContext as Material;

            material.Deposito = ((Picker)sender).SelectedItem as string;
        }
    }
}
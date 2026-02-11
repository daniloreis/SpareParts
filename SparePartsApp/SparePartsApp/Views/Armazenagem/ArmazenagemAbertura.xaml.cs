using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ArmazenagemAbertura : ContentPage
    {
        private MaterialService materialService { get; set; }

        public ArmazenagemAbertura(string codMaterial = null)
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();
        }
         
        private void btnOK_Clicked(object sender, EventArgs e)
        {
            Executar();
        }

        private void txtCodMaterial_Completed(object sender, EventArgs e)
        {
            Executar();
        }

        private async void Executar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCodMaterial.Text))
                {
                    return;
                }

                if (txtCodMaterial.Text.Length > 2)
                {
                    string CodMaterial = txtCodMaterial.Text;

                    var material = await materialService.Get($"Pendente/{CodMaterial}");

                    if (material == null)
                    {
                        await DisplayAlert("Aviso", "Material não está pendente de armazenagem!", "OK");
                        return;
                    }

                    if (material.InventarioAtivo)
                    {
                        await DisplayAlert("Aviso", "Esse material está em inventário ativo!", "OK");
                        return;
                    }

                    await Navigation.PushAsync(new ArmazenagemDetalhe(material));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        private void btnListarPendentes_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ArmazenagemLista());
        }
    }
}
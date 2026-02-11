using SparePartsApp.Models;
using SparePartsApp.Services;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MaterialDetalhe : ContentPage
    {
        private Material Material { get; set; }

        private MaterialService materialService { get; set; }

        public MaterialDetalhe()
        {
            InitializeComponent();

            if (materialService == null)
                materialService = new MaterialService();

            txtCodigo.Focus();
        }

        private async void txtCodigo_Completed(object sender, EventArgs e)
        {
            try
            {
                Material = await materialService.Get($@"Detalhe/{txtCodigo.Text}");

                if (Material == null)
                {
                    await DisplayAlert("Aviso", "Material não encontrado no SAP!", "OK");
                    return;
                }

                lblCodMaterial.Text = Material.CodMaterial;
                lblMaterial.Text = Material.Nome;
                lblUnidade.Text = Material.UnidadeMedida;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
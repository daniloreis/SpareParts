using SignaturePad.Forms;
using SparePartsApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SparePartsApp.Views.Atendimento
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AtendimentoReservaAssinatura : ContentPage
    {
        private IEnumerable<Material> materiais;

        public AtendimentoReservaAssinatura(IEnumerable<Material> materiais)
        {
            InitializeComponent();

            this.materiais = materiais;
        }

        private async void OnGetImage(object sender, EventArgs e)
        {
            try
            {
                var nomeCompleto = txtNomeCompleto.Text;

                if (string.IsNullOrWhiteSpace(nomeCompleto) || nomeCompleto.Length < 2 || nomeCompleto.Split(' ')[0].Length < 3 || nomeCompleto.Split(' ')[1].Length < 3)
                {
                    await DisplayAlert("Aviso", "É necessário o nome completo para finalizar o atendimento.", "OK");
                    return;
                }

                var settings = new ImageConstructionSettings
                {
                    Padding = 12,
                    StrokeColor = Color.Black,
                    BackgroundColor = Color.White,
                    DesiredSizeOrScale = 1f
                };

                var imageStream = await padView.GetImageStreamAsync(SignatureImageFormat.Jpeg, settings);

                if (imageStream == null || imageStream.Length < 10)
                {
                    await DisplayAlert("Aviso", "É necessário assinar para finalizar o atendimento.", "OK");
                    return;
                }

                var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var nomeArquivo = @"assinatura_" + Guid.NewGuid() + ".jpg";

                using (FileStream fileStream = new FileStream(Path.Combine(dir, nomeArquivo), FileMode.Create))
                {
                    imageStream?.CopyTo(fileStream);
                }

                foreach (var material in materiais)
                {
                    material.UsuarioAtendido = txtNomeCompleto.Text;
                    material.ImagemAssinatura = nomeArquivo;
                }

                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }
}
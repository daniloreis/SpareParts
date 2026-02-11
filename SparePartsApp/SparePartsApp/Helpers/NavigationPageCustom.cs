using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SparePartsApp.Helpers
{
    public class NavigationPageCustom : NavigationPage
    {
        public NavigationPageCustom() { }

        //Implements initial page in constructor
        public NavigationPageCustom(Page p) => Task.Run(() => { PushAsync(p); });

        public void RemovePage(Page p)
        {
            try
            {
                Navigation.RemovePage(p);
            }
            catch (Exception ex)
            {
                DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }

        public new Task PushAsync(Page page, bool animated = false)
        {
            if (Navigation.NavigationStack.Count > 0 && (page.GetType() == Navigation.NavigationStack.Last().GetType()))
            {
                return Task.FromResult(false);
            }
            return base.PushAsync(page, animated);
        }

        public Task PushModalAsync(Page page, bool animated)
        {
            if (Navigation.NavigationStack.Count > 0 && (page.GetType() == Navigation.NavigationStack.Last().GetType()))
            {
                return Task.FromResult(false);
            }
            return Navigation.PushModalAsync(page, animated);
        }

        public async Task PopUntil(Type tipo)
        {
            try
            {
                while (Navigation.NavigationStack.Count > 1 && (Navigation.NavigationStack.Last().GetType() != tipo))
                {
                    await Navigation.PopAsync(false);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", ex.GetInnerExceptionMessage(), "OK");
            }
        }
    }



}

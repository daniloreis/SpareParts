using Android.Content;
using Android.Views.InputMethods;
using SparePartsApp.Components;
using SparePartsApp.Dependencies;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(NumericEntry), typeof(KeyboardProvider))]
namespace SparePartsApp.Dependencies
{
    public class KeyboardProvider : EntryRenderer
    {
        public KeyboardProvider(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                e.NewElement.PropertyChanging += OnPropertyChanging;
            }

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanging -= OnPropertyChanging;
            }

#if !DEBUG
            Control.ShowSoftInputOnFocus = false;
#endif
        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            if (propertyChangingEventArgs.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
            {
                InputMethodManager imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(Control.WindowToken, HideSoftInputFlags.None);
            }
        }

    }
}
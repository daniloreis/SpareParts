using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(SparePartsApp.Droid.Dependencies.CheckBoxRenderer), typeof(SparePartsApp.Droid.Dependencies.CheckBoxRenderer))]
namespace SparePartsApp.Droid.Dependencies
{
    public class CheckBoxRenderer : ButtonRenderer
    {
        public CheckBoxRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            var control = new Android.Widget.CheckBox(this.Context);
            this.SetNativeControl(control);

            base.OnElementChanged(e);
        }      
    }
}
using Xamarin.Forms.Platform.Android;
using Ambulance.Droid.ExtendedControls;
using Ambulance.ExtendedControls;
using Xamarin.Forms;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(BeautifulGrayButton), typeof(BeautifulGrayButtonRenderer))]
namespace Ambulance.Droid.ExtendedControls
{
    public class BeautifulGrayButtonRenderer : BeatifulButtonRenderer
    {
        int[][] ColorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

           this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.RedRoundedButton);

             Control.SetTextColor(new ColorStateList(ColorStates, new int[] { Color.White.ToAndroid().ToArgb(), Color.FromHex("#F1F0EE").ToAndroid().ToArgb() }));           
        }
    }
}
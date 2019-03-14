using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Ambulance.ExtendedControls;
using Ambulance.Droid;

[assembly: ExportRenderer(typeof(RoundedCornerEntry), typeof(RoundedCornerEntryRenderer))]
namespace Ambulance.Droid
{
    public class RoundedCornerEntryRenderer : EntryRenderer
    {
        // Override the OnElementChanged method so we can tweak this renderer post-initial setup
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.RoundedCornerEntry);
            }
        }
    }
}
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;
using Ambulance.ExtendedControls;
using Ambulance.Droid.ExtendedControls;

[assembly: ExportRenderer(typeof(RoundedCornerEditor), typeof(RoundedCornerEditorRenderer))]
namespace Ambulance.Droid.ExtendedControls
{
    class RoundedCornerEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.RoundedCornerEntry);
				//Control.Hint = "§µ§Ü§Ñ§Ø§Ú§ä§Ö §á§â§Ú§é§Ú§ß§å §à§ä§Ü§Ñ§Ù§Ñ";               
            }
        }
    }
}
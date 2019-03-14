using Xamarin.Forms.Platform.Android;
using Ambulance.Droid.ExtendedControls;
using Ambulance.ExtendedControls;
using Xamarin.Forms;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(BeautifulOrangeButton), typeof(BeautifulOrangeButtonRenderer))]
namespace Ambulance.Droid.ExtendedControls
{

    public class BeautifulOrangeButtonRenderer : BeatifulButtonRenderer
    {
        int[][] ColorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            var button = Element as BeautifulOrangeButton;
            if (button != null)
            {
                SetBackgroundColor(button);
                button.PropertyChanged += (sender, ea) => 
                {
                    if (ea?.PropertyName == "Color")
                        SetBackgroundColor(sender as BeautifulOrangeButton);
                };
            }

            Control.SetTextColor(new ColorStateList(ColorStates, new int[] { Color.White.ToAndroid().ToArgb(), Color.FromHex("#E3E2E0").ToAndroid().ToArgb() }));

        }

        void SetBackgroundColor(BeautifulOrangeButton button)
        {
			if (button != null)
			{
				if (button.Color == Color.Red)
					Control.Background = Resources.GetDrawable(Resource.Drawable.RedRoundedButton);
				else if (button.Color == Color.Green)
					Control.Background = Resources.GetDrawable(Resource.Drawable.GreenRoundedButton2);
				else if (button.Color == Color.Gray)
					Control.Background = Resources.GetDrawable(Resource.Drawable.GrayRoundedButton);
				else
					Control.Background = Resources.GetDrawable(Resource.Drawable.RoundedButton);
			}
        }
    }
}
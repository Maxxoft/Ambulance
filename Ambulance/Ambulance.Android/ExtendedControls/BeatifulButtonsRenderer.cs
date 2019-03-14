using System;
using Xamarin.Forms.Platform.Android;
using Ambulance.Droid.ExtendedControls;
using Xamarin.Forms;
using Ambulance.ExtendedControls;

[assembly: ExportRenderer(typeof(BeatifulButton), typeof(BeatifulButtonRenderer))]
namespace Ambulance.Droid.ExtendedControls
{
    public class BeatifulButtonRenderer : ButtonRenderer
    {
        int[][] ColorStates = { new[] { global::Android.Resource.Attribute.StateEnabled }, new[] { -global::Android.Resource.Attribute.StateEnabled } };

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.RoundedButton);


            var Btn = e.NewElement as Button;
            Btn.TextColor = Color.White;
            Btn.FontAttributes = FontAttributes.None;
            Control.SetPadding(0, 0, 0, 3);
            (e.NewElement as Button).Clicked += BeatifulButtonRenderer_Clicked; ; 
        }

        private void BeatifulButtonRenderer_Clicked(object sender, EventArgs e)
        {
           var Button = sender as BeatifulButton;
                if (Button.IsPressed == true)
                   this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.GreenRoundedButton2);
                if (Button.IsPressed == false)
                       this.Control.Background = this.Resources.GetDrawable(Resource.Drawable.RoundedButton);

        }
    }
}
using Xamarin.Forms;

namespace Ambulance.ExtendedControls
{
    public class BeautifulOrangeButton : BeatifulButton
    {
        public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(BeautifulOrangeButton), Color.Orange);
        public Color Color
		{
			get { return (Color)GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}
    }
}

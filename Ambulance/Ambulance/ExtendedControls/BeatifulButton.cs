using Xamarin.Forms;

namespace Ambulance.ExtendedControls
{
    public class BeatifulButton : Button
    {
        public static readonly BindableProperty IsPressedProperty =
            BindableProperty.Create("IsPressed", typeof(bool?), typeof(BeatifulButton), null);

        public bool? IsPressed
        {
            get { return (bool?)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }
    }
}

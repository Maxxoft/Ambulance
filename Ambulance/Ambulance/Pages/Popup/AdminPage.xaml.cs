using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AdminPage : PopupPage
	{
        public AdminPage()
        {
            InitializeComponent();
            SetContent();
        }

        bool result;
        public bool Result => result;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pwdTextBox.Text = "";
        }

        private void SetContent()
        {
            //MainLayout.HeightRequest = StaticDeviceInfo.Height / 2;
            result = false;
            btnOk.Clicked += (sender, e) =>
            {
                result = pwdTextBox.Text == "09031986";
                Device.BeginInvokeOnMainThread(() => PopupNavigation.Instance.PopAllAsync());
            };

        }
    }
}
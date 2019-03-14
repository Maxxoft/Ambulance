using Ambulance.Dependency;
using Ambulance.ExtendedControls;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : PopupPage, IMapPage
    {
        public bool DialogResult = false;

        public LoadingPage()
        {
            InitializeComponent();
            SetContent();
        }

        public WebviewMap MapControl
        {
            get { return WebViewMap; }
        }

        public void MapLoaded()
        {
        }

        public void OpenPage(PageType Page, string x, string y)
        {
        }

        public void RouteCalculated()
        {
            try
            {
                PopupNavigation.Instance.PopAsync();
                DialogResult = true;
            }
            catch { }
        }

        private void SetContent()
        {
            LoadingText.Text = "Идет расчет маршрута";
            PopupLayout.Padding = new Thickness(DependencyData.Widht / 5 * 2, DependencyData.Height / 3, DependencyData.Widht / 5 * 2, DependencyData.Height / 3);
        }

        public void MapErrorAsync(string Error)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Error == null)
                    LoadingText.Text = "В данный момент яндекс не может рассчитать маршрут. Повторите попытку позже.";
                else
                    LoadingText.Text = Error;
            });
        }
    }
}
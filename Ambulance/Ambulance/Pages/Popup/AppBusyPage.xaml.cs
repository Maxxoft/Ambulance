using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppBusyPage : PopupPage
	{
        public AppBusyPage(string infoText = null)
        {
            InitializeComponent();
            lblInfoText.Text = !string.IsNullOrEmpty(infoText) ? infoText : "Подождите, идет загрузка данных...";
            CloseWhenBackgroundIsClicked = false;
        }
    }
}
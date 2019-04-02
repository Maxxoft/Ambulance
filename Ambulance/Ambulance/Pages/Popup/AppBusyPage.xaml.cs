using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppBusyPage : PopupPage
	{
        public AppBusyPage(string text = null)
        {
            InitializeComponent();
            CloseWhenBackgroundIsClicked = false;
            SetText(text);
        }

        public void SetText(string text)
        {
            lblInfoText.Text = !string.IsNullOrEmpty(text) ? text : "Подождите, идет загрузка данных...";
        }

        public void SetActivity(bool active)
        {
            activityIndicator.IsRunning = active;
        }

        private static AppBusyPage page;
        public static void Show(string text)
        {
            if (page == null)
                page = new AppBusyPage(text);
            else
                page.SetText(text);
            page.SetActivity(true);

            Device.BeginInvokeOnMainThread(async () =>
            { 
                if (!PopupNavigation.Instance.PopupStack.Contains(page))
                    await PopupNavigation.Instance.PushAsync(page, true);
            });
        }

        public static void Close()
        {
            if (page == null) return;
            page.SetActivity(false);

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (PopupNavigation.Instance.PopupStack.Contains(page))
                    await PopupNavigation.Instance.RemovePageAsync(page);
            });
        }
    }
}
using Ambulance.Data;
using Ambulance.Dependency;
using Ambulance.Helper;
using Ambulance.Pages.Popup;
using Ambulance.WebService;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AuthorizationPage : ContentPage
	{
        public AuthorizationPage()
        {
            InitializeComponent();
            Title = "Добро пожаловать";
            SetContent();
        }

        private void SetContent()
        {
            lblVersion.Text = "Версия: " + DependencyService.Get<IAPIHelper>().GetAppVersion();
            var tapGesture = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
            tapGesture.Tapped += LblVersionTapped;
            lblVersion.GestureRecognizers.Add(tapGesture);
            AcceptButton.Clicked += AcceptButton_Clicked;
            AcceptButton.IsEnabled = true;
            CloseImage.Source = ImageSource.FromFile("Close.png");
            PhoneTB.Keyboard = Keyboard.Numeric;
            PassTB.Keyboard = Keyboard.Text;


            PhoneTB.HorizontalTextAlignment = PassTB.HorizontalTextAlignment = TextAlignment.Center;
#if DEBUG
            PhoneTB.Text = "+7 (213) 243-2523"; 
            PassTB.Text = "9343"; 
#else
			PhoneTB.Text = "+7 (";
			PassTB.Text = "";
#endif
            PassTB.TextChanged += TBs_TextChanged;
            PhoneTB.TextChanged += PhoneHelper.PhoneNumber_Changed;
            PhoneTB.TextChanged += TBs_TextChanged;
            PhoneTB.FontAttributes = FontAttributes.Bold;
            if (!string.IsNullOrEmpty(AppData.Crew?.Phone))
                PhoneTB.Text = AppData.Crew?.Phone;
            else if (!string.IsNullOrEmpty(DependencyData.PhoneNumber))
                PhoneTB.Text = DependencyData.PhoneNumber;
            var Tgr = new TapGestureRecognizer();
            Tgr.Tapped += CloseApp;
            CloseImage.GestureRecognizers.Add(Tgr);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AutoLogin();
        }

        bool autoLogin;
        void AutoLogin()
        {
            if (Settings.AppState.LastPhone > 0 && !string.IsNullOrEmpty(Settings.AppState.LastPin) && Settings.AppState.LastLoginDate.Date == DateTime.Today)
            {
                PhoneTB.Text = PhoneHelper.FormattedPhoneNumber(Settings.AppState.LastPhone.ToString());
                PassTB.Text = Settings.AppState.LastPin;
                autoLogin = true;
                AcceptButton_Clicked(null, null);
            }
        }

        AdminPage adminPopup;
        async void LblVersionTapped(object sender, EventArgs e)
        {
            if (adminPopup == null)
            {
                adminPopup = new AdminPage();
                adminPopup.Disappearing += AdminPopup_Disappearing;
            }
            await PopupNavigation.Instance.PushAsync(adminPopup, true);
        }

        async void AdminPopup_Disappearing(object sender, EventArgs e)
        {
            if (!adminPopup.Result) return;
            ApiService.TestMode = true;
            await DisplayAlert("", "Тестовый режим работы включен", "OK");
        }

        private void TBs_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsAcceptBtnEnable();
        }

        private void IsAcceptBtnEnable()
        {
            AcceptButton.IsEnabled =
                !string.IsNullOrEmpty(PhoneTB.Text) &&
                !string.IsNullOrEmpty(PassTB.Text) &&
                (PhoneHelper.ValidPhoneNumber(PhoneTB.Text) != null);
        }

        private void CloseApp(object sender, EventArgs e)
        {
            DependencyService.Get<IAPIHelper>().CloseApp();
        }

        async void AcceptButton_Clicked(object sender, EventArgs e)
        {
            AcceptButton.IsEnabled = false;
            var phone = PhoneHelper.ValidPhoneNumber(PhoneTB.Text);
            if (phone == 0) return;

            App.ClearTestLog();
            var res = await ApiService.Auth(PhoneTB.Text, PassTB.Text);
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "ОК");
                AcceptButton.IsEnabled = true;
                return;
            }

            App.ClearTestLog();
            res = await ApiService.GetNewOrders();
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "ОК");
                AcceptButton.IsEnabled = true;
                return;
            }

            Settings.AppState.LastPhone = PhoneHelper.ValidPhoneNumber(PhoneTB.Text);
            Settings.AppState.LastPin = PassTB.Text;
            Settings.AppState.LastLoginDate = DateTime.Now;
            Settings.AppState.Save();

            if (AppData.Crew.ActiveOrder != null)
                App.Current.MainPage = new MainPage(PageType.ActiveOrder);
            else
                App.Current.MainPage = new MainPage(PageType.FreeOrders);
        }
    }
}
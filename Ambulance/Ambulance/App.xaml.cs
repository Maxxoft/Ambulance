using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Ambulance.Pages;
using Ambulance.WebService;
using System.Threading.Tasks;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace Ambulance
{
	public partial class App : Application
	{
        public static App Instance { get; private set; }
        public App()
        {
            InitializeComponent();
            Instance = this;
            MainPage = new AuthorizationPage();
            //MainPage = new CompanyCarsPage();
        }

        protected override void OnStart()
        {
            //
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void ClearTestLog()
        {
            ApiService.ReqLog.Clear();
        }

        public static async Task<bool> ShowTestLog(Page page)
        {
            if (page == null || !ApiService.TestMode) return true;
            foreach (var req in ApiService.ReqLog)
            {
                await page.DisplayAlert("Запрос", req.Request, "OK");
                await page.DisplayAlert("Ответ", req.Response, "OK");
            }
            return true;
        }
    }
}

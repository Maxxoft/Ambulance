using Ambulance.Data;
using Ambulance.Dependency;
using Ambulance.ExtendedControls;
using Ambulance.Helper;
using Ambulance.ObjectModel;
using Ambulance.WebService;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrdersPage : ContentPage
    {
        const string FREE_ORDERS_TITLE = "Новые заказы";
        const string ACTIVE_ORDER_TITLE = "Активный заказ";
        const string FREE_ORDERS_HEADER = "Список новых заказов";
        const string ACTIVE_ORDER_HEADER = "Активный заказ";
        const string FREE_ORDERS_EMPTY_TEXT = "Новых заказов нет";
        const string ACTIVE_ORDER_EMPTY_TEXT = "Активных заказов нет";

        public static string TitleByPageType(PageType pageType)
        {
            switch (pageType)
            {
                case PageType.FreeOrders:
                    return FREE_ORDERS_TITLE;
                case PageType.ActiveOrder:
                    return ACTIVE_ORDER_TITLE;
            }
            return null;
        }

        public static string HeaderByPageType(PageType pageType)
        {
            return pageType == PageType.ActiveOrder ? ACTIVE_ORDER_HEADER : FREE_ORDERS_HEADER;
        }

        public static string EmptyTextByPageType(PageType pageType)
        {
            return pageType == PageType.ActiveOrder ? ACTIVE_ORDER_EMPTY_TEXT : FREE_ORDERS_EMPTY_TEXT;
        }

        public PageType PageType { get; }

        public OrdersPage(PageType pageType)
        {
            InitializeComponent();
            PageType = pageType;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshContent();
            //DependencyService.Get<IAPIHelper>().RequestLocationsPermissions();

            // Send FCM Token            
            //Task.Run(() => ApiService.SendPushToken());

            //StartPositionUpdateTimer();
        }

        List<Order> curOrders;
        Order selectedOrder;
        int selectedOrderId;

        public void RefreshContent()
        {
            if (PageType == PageType.ActiveOrder)
            {
                curOrders = new List<Order>();
                if (AppData.Crew.ActiveOrder != null)
                    curOrders.Add(AppData.Crew.ActiveOrder);
                btnSwitch.Text = string.Format("НОВЫЕ ЗАКАЗЫ ({0})", AppData.Orders?.Count ?? 0);
            }
            else if (PageType == PageType.FreeOrders)
            {
                curOrders = AppData.Orders.OrderBy(x => x.ArrivalDate).ToList();
                btnSwitch.Text = string.Format("АКТИВНЫЕ ЗАКАЗЫ ({0})", AppData.Crew.ActiveOrder == null ? 0 : 1);
            }

            var ordersEmpty = curOrders.Count == 0;
            if (!ordersEmpty)
            {
                tableOrders.ItemsSource = OrdersToView();
                selectedOrder = curOrders.FirstOrDefault(x => x.OrderId == selectedOrderId);
                if (selectedOrder == null)
                {
                    selectedOrder = curOrders[0];
                }
            }
            SetContent();
            SetDetailInfo();
            ResizeTable();
            HighlightItems();
        }

        private List<OrderView> OrdersToView()
        {
            if (curOrders == null) return null;
            var ov = new List<OrderView>();
            for (int i = 0; i < curOrders.Count; i++)
            {
                ov.Add(new OrderView
                {
                    Order = curOrders[i],
                    BackgroundRowColor = Color.Transparent
                });
            }
            return ov;
        }

        private void SetContent()
        {
            Title = TitleByPageType(PageType);
            lblHeader.Text = HeaderByPageType(PageType);
            lblEmptyOrders.Text = EmptyTextByPageType(PageType);
            btnSwitch.Clicked += BtnSwitch_Clicked;
            btnRefresh.IsVisible = PageType == PageType.FreeOrders;

            var ordersEmpty = curOrders.Count == 0;
            lblEmptyOrders.IsVisible = ordersEmpty;
            gridOrders.IsVisible = !ordersEmpty;
            lblDetailsHeader.IsVisible = !ordersEmpty;

            PersonIco.Source = ImageSource.FromFile("PersonIco.ico");
            PersonIco.HeightRequest = 100;

            //pickerOrderType.IsVisible = gridFreeOrderActions.IsVisible;
            tableOrders.ItemSelected += OrdersTable_ItemSelected;
            btnRefresh.Clicked += BtnRefresh_Clicked;
            //btnShowOnMap.Clicked += CarLocationButton_Clicked;
            btnCall.Clicked += BtnCall_Clicked;
            //btnGetRoute.Clicked += BtnGetRoute_Clicked;
            btnShowOnMap.Clicked += BtnShowOnMap_Clicked;
            btnOpenNavigator.Clicked += BtnOpenNavigator_Clicked;
            btnUpdateOrder.Clicked += BtnUpdateOrder_Clicked;
            btnCancelOrder.Clicked += BtnCancelOrder_Clicked;           
            
            /*btnActiveOrder.Clicked += BackToMyOrdersButton_Clicked;
            pickerOrderType.PickerControl.Items.Clear();
            pickerOrderType.PickerControl.Items.Add("Все");
            pickerOrderType.PickerControl.Items.Add("Срочные");
            pickerOrderType.PickerControl.Items.Add("Обычные");
            pickerOrderType.PickerControl.Items.Add("Не срочные");
            pickerOrderType.PickerControl.SelectedIndex = 0;
            pickerOrderType.PickerControl.SelectedIndexChanged += PickerControl_SelectedIndexChanged;*/
        }

        private void BtnOpenNavigator_Clicked(object sender, EventArgs e)
        {
            DependencyService.Get<INavigatorHelper>().OpenNavigatorWithMultiRoute(selectedOrder);
        }

        int newOrdersAmount;
        public void SetAlertNewOrders(int amount)
        {
            bool alert = newOrdersAmount < amount;
            newOrdersAmount = amount;
            btnSwitch.Text = string.Format("НОВЫЕ ЗАКАЗЫ ({0})", AppData.Orders?.Count ?? 0);
            btnSwitch.Color = newOrdersAmount > 0 ? Color.Green : Color.Orange;
            if (alert) DependencyService.Get<IAPIHelper>().PlayAlertSound();
        }

        void BtnSwitch_Clicked(object sender, EventArgs e)
        {
            if (PageType == PageType.FreeOrders && AppData.Crew.ActiveOrder == null)
                return;
            MainPage.Instance?.SwitchToPage(PageType == PageType.FreeOrders ? PageType.ActiveOrder : PageType.FreeOrders, false);
        }

        async void BtnCall_Clicked(object sender, EventArgs e)
        {            
            var num = PhoneHelper.ValidPhoneNumber(selectedOrder?.SickPhone);
            if (num == 0) 
            {
                await DisplayAlert("Ошибка", "Номер телефона не указан или имеет некорректный формат", "OK");
                return;
            }
            var pn = num.ToString();
            if (pn[0] == '7') pn = "8" + pn.Substring(1);
            Device.OpenUri(new Uri("tel:" + pn));
        }

        void BtnRefresh_Clicked(object sender, EventArgs e)
        {
            MainPage.Instance.SwitchToPage(PageType, true);
        }

        private void BtnGetRoute_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MapPage(selectedOrder, MapType.SimpleRoute));

            /*var LoadingPage = new LoadingPage();
            LoadingPage.MapControl.MapId = MapType.SimpleRoute;
            LoadingPage.MapControl.Orders = new List<Order> { selectedOrder };
            LoadingPage.MapControl.ParentPage = LoadingPage;
            PopupNavigation.Instance.PushAsync(LoadingPage, true);
            LoadingPage.Disappearing += LoadingPage_Disappearing;*/
        }

        /*private void LoadingPage_Disappearing(object sender, EventArgs e)
        {
            var loadPage = sender as LoadingPage;
            if (loadPage == null) return;

            if (!loadPage.DialogResult) return;

            Orders = loadPage.MapControl.Orders.Clone();
            tableOrders.ItemsSource = Orders.ToView(PageType == PageType.BoosterOrders);
            SelectedOrder = Orders[0];
            HighlightItems();
            loadPage.Disappearing -= LoadingPage_Disappearing;

            Settings.AppState.CalcRouteTime = DateTime.Now;
            Settings.AppState.Save();
        }*/

        private void BtnShowOnMap_Clicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is MapPage) != null)
                return;
            Navigation.PushAsync(new MapPage(selectedOrder, MapType.SimpleRoute));
            /*if (selectedOrder?.AddressFromLat != 0 && selectedOrder?.AddressFromLng != 0)
                Navigation.PushAsync(new MapPage(selectedOrder.AddressFromLat, selectedOrder.AddressFromLng));*/
        }

        async void BtnUpdateOrder_Clicked(object sender, EventArgs e)
        {
            if (selectedOrder == null) return;

            var ans = await DisplayAlert("", OrderHelper.EligibleActionConfirmation(selectedOrder.Status), "Ок", "Отмена");
            if (!ans) return;
            var newStatus = selectedOrder.Status + 1;
            if (newStatus == OrderStatus.Cancelled)
                newStatus++;
            App.ClearTestLog();
            var res = await ApiService.UpdateOrder(selectedOrder, (int)newStatus);
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "OK");
                return;
            }
            if (selectedOrder.Status == OrderStatus.Assigned)
            {
                AppData.Crew.ActiveOrder = selectedOrder;
                AppData.Orders.Remove(selectedOrder);
                MainPage.Instance?.SwitchToPage(PageType.ActiveOrder, false);
            }
            else
                btnUpdateOrder.Text = OrderHelper.EligibleActionName(selectedOrder.Status);
        }

        async void BtnCancelOrder_Clicked(object sender, EventArgs e)
        {
            if (selectedOrder == null) return;

            var ans = await DisplayAlert("", "Отказаться от выполнения заявки?", "Ок", "Отмена");
            if (!ans) return;
            var newStatus = OrderStatus.Cancelled;
            App.ClearTestLog();
            var res = await ApiService.UpdateOrder(selectedOrder, (int)newStatus);
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "OK");
                return;
            }
            AppData.Crew.ActiveOrder = null;
            MainPage.Instance?.SwitchToPage(PageType.FreeOrders, true);
        }
        

        private void GetRouteButton_Clicked(object sender, EventArgs e)
        {
            /*var LoadingPage = new LoadingPopup();
            LoadingPage.MapControl.MapId = MapType.OptimalRoute;
            LoadingPage.MapControl.BoosterOrders = Orders;
            LoadingPage.MapControl.ParentPage = LoadingPage;
            Navigation.PushPopupAsync(LoadingPage, true);
            LoadingPage.Disappearing += LoadingPage_Disappearing;*/
        }

        //TODO
        /*private void LoadingPage_Disappearing(object sender, EventArgs e)
        {
            var loadPage = sender as LoadingPopup;
            if (loadPage == null) return;

            if (!loadPage.DialogResult) return;

            Orders = loadPage.MapControl.BoosterOrders.Clone();
            tableOrders.ItemsSource = Orders.ToView(PageType == PageType.BoosterOrders);
            SelectedOrder = Orders[0];
            HighlightItems();
            loadPage.Disappearing -= LoadingPage_Disappearing;

            Settings.AppState.CalcRouteTime = DateTime.Now;
            Settings.AppState.Save();
        }*/


        static bool PositionUpdateTimer;
        public void StartPositionUpdateTimer()
        {
            if (PositionUpdateTimer) return;
            PositionUpdateTimer = true;
            Task.Run(async () =>
            {
                while (PositionUpdateTimer)
                {
                    await Task.Delay(60000);
                    if (!PositionUpdateTimer) return;
                    var currentCoord = DependencyService.Get<IAPIHelper>().GetCurrentLocation();
                    if (currentCoord == null || currentCoord.Latitude < 1 || currentCoord.Longitude < 1)
                        continue;
                    await ApiService.UpdateLocation(currentCoord.Latitude, currentCoord.Longitude);
                }
            });
        }

        private void ResizeTable()
        {
            var ordersCount = tableOrders.ItemsSource?.Cast<OrderView>().Count() ?? 0;
            if (ordersCount != 0)
                tableOrders.HeightRequest = ((int)ordersCount + 1) * tableOrders.RowHeight + 10;
        }

        /*private void PickerControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tableOrders.ItemsSource != null)
                if (pickerOrderType.PickerControl.SelectedIndex == 0)
                    tableOrders.ItemsSource = Orders.OrderBy(x => (x.ToDate != null) ? x.ToDate.Value.Ticks : 99999999999999999).ToList().ToView(false);
            if (pickerOrderType.PickerControl.SelectedIndex == 1)
                tableOrders.ItemsSource = GetFilteredOrders(4).ToView(false);
            if (pickerOrderType.PickerControl.SelectedIndex == 2)
                tableOrders.ItemsSource = GetFilteredOrders(2).ToView(false);
            if (pickerOrderType.PickerControl.SelectedIndex == 3)
                tableOrders.ItemsSource = GetFilteredOrders(99999999).ToView(false);
            ResizeTable();

        }

        private List<Order> GetFilteredOrders(int ExpiredTime)
        {
            return Orders.Where(x => TimeSpan.FromTicks(x.ToDate.Value.Ticks - DateTime.Now.Ticks).TotalHours < ExpiredTime).OrderBy(x => (x.ToDate != null) ? x.ToDate.Value.Ticks : 99999999999999999).ToList();
        }*/

        private void CalculateRoute()
        {

        }
        /*
        RejectPopup rejectPopup;
        async private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (SelectedOrder == null) return;

            if (rejectPopup == null)
            {
                rejectPopup = new RejectPopup();
                rejectPopup.Disappearing += RejetPage_Disappearing;
            }
            rejectPopup.Clear();
            await Navigation.PushPopupAsync(rejectPopup, true);
        }

        async void RejetPage_Disappearing(object sender, EventArgs e)
        {
            if (!rejectPopup.Result) return;
            App.ClearTestLog();
            var res = await Task.Run(() => MFService.BoosterRejectOrder(SelectedOrder, rejectPopup.Text));
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "OK");
                return;
            }
            SelectedOrder.Booster = null;
            MFData.BoosterOrders.Remove(SelectedOrder);
            MFData.FreeOrders.Add(SelectedOrder);

            MainPage.Instance?.SwitchToPage(PageType.BoosterOrders, false);
        }

        */

        private void SetDetailInfo()
        {
            gridOrderDetails.IsVisible = selectedOrder != null;
            gridOrderActions.IsVisible = selectedOrder != null;
            if (selectedOrder == null) return;
            lbPatient.Text = "Пациент: " + selectedOrder.SickName + ", " + selectedOrder.SickPhone;
            lbOrder.Text = "№ Заказа: " + selectedOrder.OrderId.ToString();
            lbArrivalDate.Text = "Дата и время: " + selectedOrder.ArrivalDate.ToString("dd.MM.yy HH:mm");
            lbAddressFrom.Text = "Адрес откуда: " + selectedOrder.AddressFrom;
            lbAddressTo.Text = "Адрес куда: " + selectedOrder.AddressTo;
            lbDistance.Text = "До клиента: " + selectedOrder.Distance.ToString() + " км";
            lbComment.Text = "Доп. инфо: " + selectedOrder.Comment;
            btnUpdateOrder.Text = OrderHelper.EligibleActionName(selectedOrder.Status);
            btnCancelOrder.IsVisible = PageType == PageType.ActiveOrder;
        }

        private void OrdersTable_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(e.SelectedItem is OrderView))
                return;
            selectedOrder = (e.SelectedItem as OrderView).Order;
            selectedOrderId = selectedOrder.OrderId;
            SetDetailInfo();
            HighlightItems();
        }

        private void HighlightItems()
        {
            var temp = OrdersToView();
            if (temp == null) return;

            int idx = -1;
            for (int i = 0; i < temp.Count; i++)
            {
                int bc = 0;
                //if (temp[i].Order.CallMeNow) bc = 1;
                if (temp[i].Order.OrderId == selectedOrder?.OrderId) bc += 2;
                switch (bc)
                {
                    case 0:
                        temp[i].BackgroundRowColor = Color.Transparent;
                        break;
                    case 1:
                        temp[i].BackgroundRowColor = Color.Red;
                        break;
                    case 2:
                        temp[i].BackgroundRowColor = Color.FromHex("#FFE465");
                        idx = i;
                        break;
                    case 3:
                        temp[i].BackgroundRowColor = Color.OrangeRed;
                        idx = i;
                        break;
                }
            }
            tableOrders.ItemsSource = temp;
        }

        public class OrderView
        {
            public Order Order { get; set; }
            public Color BackgroundRowColor { get; set; }
        }

        T First<T>(IEnumerable<T> items)
        {
            using (IEnumerator<T> iter = items.GetEnumerator())
            {
                iter.MoveNext();
                return iter.Current;
            }
        }
    }
}
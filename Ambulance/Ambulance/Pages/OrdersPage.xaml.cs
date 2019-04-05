using Ambulance.Data;
using Ambulance.Dependency;
using Ambulance.ExtendedControls;
using Ambulance.Helper;
using Ambulance.ObjectModel;
using Ambulance.Pages.Popup;
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
    public partial class OrdersPage : ContentPage, IMapPage
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


        private PageType _pageType;
        public PageType PageType
        {
            get { return _pageType; }
            set
            {
                if (_pageType == value) return;
                _pageType = value;
                RefreshContent();
            }
        }

        public bool IsBusy { get; private set; }
        public OrdersPage(PageType pageType)
        {
            InitializeComponent();
            _pageType = pageType;

            //btnSwitch.Clicked += BtnSwitch_Clicked;
            //btnSwitch.IsVisible = false;
            swOnline.Toggled += Online_Toggled;

            tableOrders.ItemSelected += OrdersTable_ItemSelected;
            btnRefresh.Clicked += BtnRefresh_Clicked;
            btnCall.Clicked += BtnCall_Clicked;
            btnShowOnMap.Clicked += BtnShowOnMap_Clicked;
            btnOpenNavigator.Clicked += BtnOpenNavigator_Clicked;
            btnUpdateOrder.Clicked += BtnUpdateOrder_Clicked;
            btnCancelOrder.Clicked += BtnCancelOrder_Clicked;

            PersonIco.Source = ImageSource.FromFile("PersonIco.ico");
            PersonIco.HeightRequest = 100;

            RefreshContent();
            StartCalcDistances();
        }

        List<Order> curOrders;
        Order selectedOrder;
        int selectedOrderId;

        public async void RefreshContent()
        {
            try
            {
                IsBusy = true;
                if (PageType == PageType.ActiveOrder)
                {
                    curOrders = new List<Order>();
                    if (AppData.Crew.ActiveOrder != null)
                        curOrders.Add(AppData.Crew.ActiveOrder);
                    //btnSwitch.Text = string.Format("НОВЫЕ ЗАКАЗЫ ({0})", AppData.Orders?.Count ?? 0);
                }
                else if (PageType == PageType.FreeOrders)
                {
                    curOrders = AppData.Orders.OrderBy(x => x.ArrivalDate).ToList();
                    //btnSwitch.Text = string.Format("АКТИВНЫЕ ЗАКАЗЫ ({0})", AppData.Crew.ActiveOrder == null ? 0 : 1);
                }

                swOnlineHandling = false;
                swOnline.IsToggled = AppData.Crew?.Online ?? false;
                lbOnline.Text = (AppData.Crew?.Online ?? false) ? "Текущий Статус: \"НА ЛИНИИ\"" : "Текущий Статус: \"ЗАНЯТ\"";
                slOnline.BackgroundColor = (AppData.Crew?.Online ?? false) ? Color.Orange : Color.Gray;
                swOnlineHandling = true;
                selectedOrder = null;
                var ordersEmpty = curOrders.Count == 0;
                if (!ordersEmpty)
                {
                    tableOrders.ItemsSource = OrdersToView();
                    selectedOrder = curOrders.FirstOrDefault(x => x.OrderId == selectedOrderId);
                    if (selectedOrder == null)
                    {
                        selectedOrder = curOrders[0];
                    }
                    selectedOrderId = selectedOrder.OrderId;
                }

                Title = TitleByPageType(PageType);
                lblHeader.Text = HeaderByPageType(PageType);
                lblEmptyOrders.Text = EmptyTextByPageType(PageType);

                //btnRefresh.IsVisible = PageType == PageType.FreeOrders;

                lblEmptyOrders.IsVisible = ordersEmpty;
                gridOrders.IsVisible = !ordersEmpty;
                lblDetailsHeader.IsVisible = !ordersEmpty;

                SetDetailInfo();
                ResizeTable();
                HighlightItems();

                await DependencyService.Get<IAPIHelper>().RequestLocationsPermissions();
            }
            finally
            {
                IsBusy = false;
            }
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

        bool swOnlineHandling;
        private async void Online_Toggled(object sender, ToggledEventArgs e)
        {
            if (!swOnlineHandling) return;

            var s = "Установить статус \"НА ЛИНИИ\"?";
            if (!e.Value)
                s = "Установить статус \"ЗАНЯТ\"?";
            var res = await DisplayAlert("Подтверждение", s, "OK", "Отмена");
            if (res)
            {
                App.ClearTestLog();
                AppBusyPage.Show("Изменение статуса...");
                var r = await ApiService.SetCrewState(e.Value);
                AppBusyPage.Close();
                await App.ShowTestLog(this);
                if (!string.IsNullOrEmpty(r))
                {
                    await DisplayAlert("Ошибка", r, "OK");
                    res = false;
                }
            }
            if (!res)
            {
                swOnlineHandling = false;
                swOnline.IsToggled = !e.Value;
                swOnlineHandling = true;
            }
            else
            {
                AppData.Crew.Online = e.Value;
                lbOnline.Text = (AppData.Crew?.Online ?? false) ? "Текущий Статус: \"НА ЛИНИИ\"" : "Текущий Статус: \"ЗАНЯТ\"";
                slOnline.BackgroundColor = (AppData.Crew?.Online ?? false) ? Color.Orange : Color.Gray;
                AppData.Save();
            }
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
            //btnSwitch.Text = string.Format("НОВЫЕ ЗАКАЗЫ ({0})", AppData.Orders?.Count ?? 0);
            //btnSwitch.Color = newOrdersAmount > 0 ? Color.Green : Color.Orange;
            if (alert) DependencyService.Get<IAPIHelper>().PlayAlertSound();
        }

        public void NotifyActiveOrder(Order oldActiveOrder)
        {
            string alert = "";
            if (oldActiveOrder?.OrderId == null && AppData.Crew.ActiveOrder != null)
                alert = "Назначен новый заказ!";
            else if (oldActiveOrder != null)
            {
                if (AppData.Crew.ActiveOrder == null)
                    alert = "Активный заказ был отменен на сервере!";
                else if (oldActiveOrder.Status != AppData.Crew.ActiveOrder.Status)
                    alert = "Статус активного заказа был изменен на сервере";
            }

            if (string.IsNullOrEmpty(alert)) return;

            RefreshContent();
            DependencyService.Get<IAPIHelper>().PlayAlertSound();
            DisplayAlert("Информация", alert, "ОК");
        }

        void BtnSwitch_Clicked(object sender, EventArgs e)
        {
            if (PageType == PageType.FreeOrders && AppData.Crew.ActiveOrder == null)
                return;

            PageType = PageType == PageType.FreeOrders ? PageType.ActiveOrder : PageType.FreeOrders;
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

        async void BtnRefresh_Clicked(object sender, EventArgs e)
        {
            try
            {
                IsBusy = true;
                if (PageType == PageType.FreeOrders)
                    await ApiService.GetNewOrders();
                else
                    await ApiService.GetActiveOrder();
                RefreshContent();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void BtnShowOnMap_Clicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.FirstOrDefault(x => x is MapPage) != null)
                return;
            Navigation.PushAsync(new MapPage(selectedOrder, MapType.SimpleRoute));
        }

        async void BtnUpdateOrder_Clicked(object sender, EventArgs e)
        {
            if (selectedOrder == null) return;

            try
            {
                IsBusy = true;
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
                    PageType = PageType.ActiveOrder;
                }
                else if (selectedOrder.Status == OrderStatus.Done)
                {
                    AppData.Crew.ActiveOrder = null;
                    PageType = PageType.FreeOrders;
                }
                else
                    btnUpdateOrder.Text = OrderHelper.EligibleActionName(selectedOrder.Status);
            }
            finally
            {
                IsBusy = false;
            }
        }

        SelectCancelReasonPage reasonPage;
        private async void BtnCancelOrder_Clicked(object sender, EventArgs e)
        {
            if (selectedOrder == null) return;

            if (reasonPage == null)
            {
                reasonPage = new SelectCancelReasonPage();
                reasonPage.Disappearing += SelectReasonPopup_Disappearing;
            }
            await PopupNavigation.Instance.PushAsync(reasonPage, true);
        }

        async void SelectReasonPopup_Disappearing(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(reasonPage?.SelectedReason))
                return;

            App.ClearTestLog();
            AppBusyPage.Show("Отмена заказа...");
            var res = await ApiService.CancelOrder(selectedOrder, reasonPage.SelectedReason);
            AppBusyPage.Close();
            await App.ShowTestLog(this);
            if (!string.IsNullOrEmpty(res))
            {
                await DisplayAlert("Ошибка", res, "OK");
                return;
            }
            AppData.Crew.ActiveOrder = null;
            RefreshContent();
        }

        private void ResizeTable()
        {
            var ordersCount = tableOrders.ItemsSource?.Cast<OrderView>().Count() ?? 0;
            if (ordersCount != 0)
                tableOrders.HeightRequest = ((int)ordersCount + 1) * tableOrders.RowHeight + 10;
        }

        private void SetDetailInfo()
        {
            gridOrderDetails.IsVisible = selectedOrder != null;
            gridOrderActions.IsVisible = (selectedOrder?.Status ?? OrderStatus.Cancelled) != OrderStatus.Cancelled;
            if (selectedOrder == null) return;
            lbPatient.Text = "Пациент: " + selectedOrder.SickName + ", " + selectedOrder.SickPhone;
            lbOrder.Text = "№ Заказа: " + selectedOrder.OrderId.ToString();
            lbArrivalDate.Text = "Дата и время: " + selectedOrder.ArrivalDate.ToString("dd.MM.yy HH:mm");
            lbAddressFrom.Text = "Адрес откуда: " + selectedOrder.AddressFrom;
            lbAddressTo.Text = "Адрес куда: " + selectedOrder.AddressTo;
            lbDistance.Text = "До пациента: " + selectedOrder.Distance.ToString() + " км";
            lbComment.Text = "Доп. инфо: " + selectedOrder.Comment;
            btnUpdateOrder.Text = OrderHelper.EligibleActionName(selectedOrder.Status);
            btnUpdateOrder.IsVisible = (selectedOrder?.Status ?? OrderStatus.Cancelled) < OrderStatus.Cancelled;
            btnCancelOrder.IsVisible = (selectedOrder?.Status ?? OrderStatus.Cancelled) < OrderStatus.Cancelled;
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

        Order calcOrder;
        WebviewMap webviewMap;
        public bool CalcDistance = false;
        int CalcTime;
        public void StartCalcDistances()
        {
            if (CalcDistance) return;
            CalcDistance = true;
            Task.Run(async () =>
            {
                while (CalcDistance)
                {
                    while (webviewMap != null)
                    {
                        await Task.Delay(500);
                        CalcTime += 500;
                        if (CalcTime > 90000)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                if (webviewMap != null)
                                    slFake.Children.Remove(webviewMap);
                                webviewMap = null;
                            });
                            if (calcOrder != null)
                                calcOrder.CalcDistanceDate = DateTime.Now;
                        }
                    }
                    if (!CalcDistance) return;

                    calcOrder = null;
                    if ((DateTime.Now - (AppData.Crew.ActiveOrder?.CalcDistanceDate ?? DateTime.Now)).TotalMinutes > 5)
                        calcOrder = AppData.Crew.ActiveOrder;
                    if (calcOrder == null)
                        calcOrder = curOrders?.FirstOrDefault(x => (DateTime.Now - x.CalcDistanceDate).TotalMinutes > 5);
                    if (calcOrder != null)
                    {
                        webviewMap = new WebviewMap
                        {
                            ParentPage = this,
                            MapId = MapType.OptimalRoute,
                            Order = calcOrder,
                            HeightRequest = 0,
                            WidthRequest = 0
                        };
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (webviewMap != null)
                                slFake.Children.Add(webviewMap);
                        });
                    }
                }
            });
        }

        public void MapLoaded()
        {
            //throw new NotImplementedException();
        }

        public void RouteCalculated()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (webviewMap != null)
                    slFake.Children.Remove(webviewMap);
                webviewMap = null;
                RefreshContent();
            });
        }

        public void OpenPage(PageType Type, string MapX, string Map)
        {
            //throw new NotImplementedException();
        }

        public void MapErrorAsync(string Error)
        {
            //throw new NotImplementedException();
        }
    }
}
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MasterPage : ContentPage
	{
        public ListView ListView { get { return listView; } }

        public MasterPage()
        {
            InitializeComponent();

            var masterPageItems = new List<MasterPageItem>();

            /*masterPageItems.Add(new MasterPageItem
            {
                Title = "Новые заказы",
                IconSource = "",
                TargetType = typeof(OrdersPage),
                Pagetype = PageType.FreeOrders
            });*/
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Активный заказ",
                IconSource = "",
                TargetType = typeof(OrdersPage),
                Pagetype = PageType.ActiveOrder
            });
            masterPageItems.Add(new MasterPageItem
            {
                Title = "Выход",
                IconSource = "",
                Pagetype = PageType.LogOut
            });

            listView.ItemsSource = masterPageItems;
        }
    }
}
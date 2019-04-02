using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectCancelReasonPage : PopupPage
	{
		public SelectCancelReasonPage ()
		{
			InitializeComponent ();
            CloseWhenBackgroundIsClicked = false;
            UpdateContent();
        }

        public string SelectedReason { get; private set; }

        private void UpdateContent()
        {
            ObservableCollection<Reason> reasons = new ObservableCollection<Reason>();
            reasons.Add(new Reason { Name = "Поломка в пути" });
            reasons.Add(new Reason { Name = "Остановка в пути для оказания помощи" });

            lvReasons.ItemsSource = reasons;
            lvReasons.SelectedItem = reasons[0];

            btnAccept.Clicked += (sender, e) =>
            {
                if (lvReasons.SelectedItem == null) return;
                SelectedReason = (lvReasons.SelectedItem as Reason)?.Name;
                Device.BeginInvokeOnMainThread(() => PopupNavigation.Instance.RemovePageAsync(this));
            };

            btnCancel.Clicked += (sender, e) =>
            {
                SelectedReason = null;
                Device.BeginInvokeOnMainThread(() => PopupNavigation.Instance.RemovePageAsync(this));
            };
        }

        class Reason
        {
            public string Name { get; set; }
        }
    }
}
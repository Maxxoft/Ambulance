using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Ambulance.ObjectModel;
using System.Collections.ObjectModel;
using Ambulance.Data;
using System;
using System.Linq;

namespace Ambulance.Pages.Popup
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SelectCrewTypePage : PopupPage
	{
		public SelectCrewTypePage ()
		{
			InitializeComponent ();
            CloseWhenBackgroundIsClicked = false;
            UpdateContent();
		}

        public CrewType SelectedCrewType { get; private set; }
        private Action<CrewType> done;

        private void UpdateContent()
        {
            ObservableCollection<CrewType> crewTypes = new ObservableCollection<CrewType>();
            foreach (var ct in AppData.CrewTypes)
                crewTypes.Add(ct);
            lvCrewTypes.ItemsSource = crewTypes;
            if (AppData.Crew.CrewTypeId > 0)
                lvCrewTypes.SelectedItem = crewTypes.FirstOrDefault(x => x.Id == AppData.Crew.CrewTypeId);
            if (lvCrewTypes.SelectedItem == null)
                lvCrewTypes.SelectedItem = crewTypes[0];

            btnAccept.Clicked += (sender, e) =>
            {
                if (lvCrewTypes.SelectedItem == null) return;
                SelectedCrewType = lvCrewTypes.SelectedItem as CrewType;
                Device.BeginInvokeOnMainThread(() => PopupNavigation.Instance.RemovePageAsync(this));
                
            };
        }
	}
}
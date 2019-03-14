using System;

namespace Ambulance.Pages
{
    public enum PageType
    {
        FreeOrders,
        ActiveOrder,
        LogOut
    }

    public class MasterPageItem
	{
		public string Title { get; set; }
		public string IconSource { get; set; }
		public Type TargetType { get; set; }
        public PageType Pagetype { get; set; }
	}
}

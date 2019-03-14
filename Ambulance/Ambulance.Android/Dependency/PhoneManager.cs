using System;
using Ambulance.Dependency;
using Android.Telephony;
using Android.App;
using Android.Content;
using Ambulance.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(PhoneManager))]
namespace Ambulance.Droid
{
	public class PhoneManager : Java.Lang.Object, IPhoneManager
	{
		public string GetOwnPhoneNumber()
		{
			string ret;
			try
			{
				TelephonyManager mTelephonyMgr;
                mTelephonyMgr = (TelephonyManager)Application.Context.GetSystemService(Context.TelephonyService);
             

                ret = mTelephonyMgr.Line1Number;
			}
			catch (Exception e)
			{
				ret = null;
			}
			return ret;
		}
	}
}

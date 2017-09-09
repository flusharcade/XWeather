using Foundation;
using UIKit;

using Microsoft.Azure.Mobile.Distribute;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace XWeather.iOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public AppDelegate ()
		{
			Shared.Bootstrap.Run ();

			MobileCenter.Start ("fae15d34-af1f-47ec-bec2-6f37affc1cb3",
				   typeof (Analytics), typeof (Crashes));
			
			//Analytics.Start ();
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{

#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start ();
#endif
			return true;
		}

		public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
		{
			Distribute.OpenUrl (url);

			return true;
		}

#if ENABLE_TEST_CLOUD
		[Export ("updateSettingsToImperial")]
		public NSString UpdateSettingsToImperial ()
		{
			SettingsStudio.Settings.UomTemperature = TemperatureUnits.Celsius;
			SettingsStudio.Settings.UomDistance = DistanceUnits.Kilometers;
			SettingsStudio.Settings.UomPressure = PressureUnits.Millibars;
			SettingsStudio.Settings.UomLength = LengthUnits.Millimeters;
			SettingsStudio.Settings.UomSpeed = SpeedUnits.KilometersPerHour;

			return new NSString ("done");
		}
#endif
	}
}
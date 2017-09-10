using Foundation;
using UIKit;

using Microsoft.Azure.Mobile.Push;
using Microsoft.Azure.Mobile.Distribute;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Newtonsoft.Json.Linq;

namespace XWeather.iOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public AppDelegate ()
		{
			Shared.Bootstrap.Run ();

			Analytics.Start ();
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{

#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start ();
#endif

			Push.SetEnabledAsync (true);

			// This should come before MobileCenter.Start() is called
			Push.PushNotificationReceived += (sender, e) =>
			{

				// Add the notification message and title to the message
				var summary = $"Push notification received:" +
									$"\n\tNotification title: {e.Title}" +
									$"\n\tMessage: {e.Message}";

				// If there is custom data associated with the notification,
				// print the entries
				if (e.CustomData != null)
				{
					summary += "\n\tCustom data:\n";
					foreach (var key in e.CustomData.Keys)
					{
						summary += $"\t\t{key} : {e.CustomData [key]}\n";
					}
				}

				// Send the notification summary to debug output
				System.Diagnostics.Debug.WriteLine (summary);
			};

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

		#region Remote Notifications

		public override void RegisteredForRemoteNotifications (UIApplication application, NSData deviceToken)
		{
			Push.RegisteredForRemoteNotifications (deviceToken);
		}

		public override void FailedToRegisterForRemoteNotifications (UIApplication application, NSError error)
		{
			Push.FailedToRegisterForRemoteNotifications (error);
		}

		public override void DidReceiveRemoteNotification (UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			var result = Push.DidReceiveRemoteNotification (userInfo);
			if (result)
			{
				completionHandler?.Invoke (UIBackgroundFetchResult.NewData);
			}
			else
			{
				completionHandler?.Invoke (UIBackgroundFetchResult.NoData);
			}
		}

		#endregion
	}
}
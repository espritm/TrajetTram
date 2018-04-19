using Plugin.Geolocator;
using System;
using System.Threading.Tasks;

namespace Trajet_Tram.Helpers
{
    public class GpsHelper
    {
        public static async Task StartListening(EventHandler<Plugin.Geolocator.Abstractions.PositionEventArgs> OnPositionChanged)
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            ///This logic will run on the background automatically on iOS, however for Android and UWP you must put logic in background services. Else if your app is killed the location updates will be killed.
            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(Settings.MinimumTimeGPS), Settings.MinimumDistanceGPS, false, 
                //iOS only
                new Plugin.Geolocator.Abstractions.ListenerSettings
                {
                    ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = true,
                    DeferralDistanceMeters = 1,
                    DeferralTime = TimeSpan.FromSeconds(1),
                    ListenForSignificantChanges = true,
                    PauseLocationUpdatesAutomatically = false
                });

            CrossGeolocator.Current.PositionChanged += OnPositionChanged;
        }

        public static async Task StopListening()
        {
            await CrossGeolocator.Current.StopListeningAsync();
        }

    }
}

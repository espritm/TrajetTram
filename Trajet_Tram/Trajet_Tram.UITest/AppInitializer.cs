using System;
using System.IO;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Trajet_Tram.UITest
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
                return ConfigureApp
                    .Android
                    .WaitTimes(new WaitTimes())
                    .ApkFile("../../../Trajet_Tram.Android/bin/Debug/com.respawnsive.TrajetTram.apk")
                    .StartApp();
            }

            return ConfigureApp
                .iOS
                .StartApp();
        }

        public class WaitTimes : Xamarin.UITest.Utils.IWaitTimes
        {
            public TimeSpan GestureWaitTimeout
            {
                get { return TimeSpan.FromSeconds(30); }
            }
            public TimeSpan WaitForTimeout
            {
                get { return TimeSpan.FromMinutes(2); }
            }

            public TimeSpan GestureCompletionTimeout
            {
                get { return TimeSpan.FromSeconds(30); }
            }
        }
    }
}


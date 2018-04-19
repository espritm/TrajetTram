using Android.App;
using Android.OS;
using Trajet_Tram.Helpers;
using Plugin.CurrentActivity;
using System;
using System.IO;

namespace Trajet_Tram.Droid
{
#if DEBUG
    [Application(Theme = "@style/TrajetTramTheme_dayMode", Debuggable = true)]
#else
    [Application(Theme = "@style/TrajetTramTheme_dayMode", Debuggable = false)]
#endif
    public class Application_TrajetTram : Application, Application.IActivityLifecycleCallbacks
    {
        public Application_TrajetTram(IntPtr handle, global::Android.Runtime.JniHandleOwnership transer)
            : base(handle, transer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            RegisterActivityLifecycleCallbacks(this);

            string sLocalDBPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "TrajetTram.db3");
            LocalDatabase.Init(sLocalDBPath);

            Settings.AppVersionName = PackageManager.GetPackageInfo(PackageName, 0).VersionName;
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}
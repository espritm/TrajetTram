using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Trajet_Tram.Droid.Activities
{
    [Activity(Theme = "@style/Theme.Splash", Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon", NoHistory = true, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class Activity_Splash : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            MobileCenter_Helper.InitAndroidAnalytics();

            if (Utils.AskForPermissions.askForWriteExternalStoragePermission(this, 1))
                Init();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 1 && grantResults[0] == Permission.Granted)
                Init();
            else
            {
                Android.Support.V7.App.AlertDialog dialog = new Android.Support.V7.App.AlertDialog.Builder(this)
                    .SetTitle(Resources.GetString(Resource.String.app_name))
                    .SetMessage(Resources.GetString(Resource.String.activitySplash_authorizationHasBeenRefused))
                    .SetPositiveButton(Resources.GetString(Resource.String.snackbar_OK), delegate
                    {
                        Finish();
                    })
                    .SetIcon(Resource.Drawable.Icon)
                    .SetCancelable(false)
                    .Show();
            }
        }

        private void Init()
        {
            try
            {
                //Create the log file
                string sLogFilesDirectory = $"{Android.OS.Environment.ExternalStorageDirectory.Path}{System.IO.Path.DirectorySeparatorChar}{GetString(Resource.String.app_name)}{Path.DirectorySeparatorChar}";
                Settings.LogFilePath = $"{sLogFilesDirectory}log_{DateTime.Now.ToString("yyyy-MM-dd")}.txt";
                if (!Directory.Exists(sLogFilesDirectory))
                    Directory.CreateDirectory(sLogFilesDirectory);
                if (!File.Exists(Settings.LogFilePath))
                    File.Create(Settings.LogFilePath);

                //Remove all old log files
                List<string> lsAllLogFilesNames = new List<string>(Directory.GetFiles(sLogFilesDirectory));
                foreach (string sLogFileName in lsAllLogFilesNames)
                    if (File.GetCreationTime(sLogFileName).Date < DateTime.Now.Date.AddDays(-30))
                        File.Delete(sLogFileName);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            if (Settings.UserPassword == "" || Settings.UserToken == "")
                StartActivity(typeof(Activity_Login));
            else
                StartActivity(typeof(Activity_Dashboard));
        }
    }
}
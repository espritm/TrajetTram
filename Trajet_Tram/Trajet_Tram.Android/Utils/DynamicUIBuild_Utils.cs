using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.App;
using Trajet_Tram.Helpers;
using Android.Widget;
using Android.Hardware;
using Android.Content.PM;
using System;
using Trajet_Tram.Droid.Activities;

namespace Trajet_Tram.Droid.Utils
{
    public static class DynamicUIBuild_Utils
    {
        public static void HideKeyboard(Context context, View v)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(v.WindowToken, 0);
        }

        internal static void ShowSnackBar(Context context, View view, int iResourceStringID, int iSnackbarLength)
        {
            Snackbar bar = Snackbar.Make(view, iResourceStringID, iSnackbarLength);

            bar.View.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(context, Resource.Color.primary_dark)));

            bar.Show();
        }

        internal static void ShowSnackBar(Context context, View view, string sText, int iSnackbarLength)
        {
            Snackbar bar = Snackbar.Make(view, sText, iSnackbarLength);

            bar.View.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(context, Resource.Color.primary_dark)));

            bar.Show();
        }

        internal static void ShowSnackBar_WithOKButtonToClose(Context context, View view, int iResourceStringID)
        {
            Snackbar bar = Snackbar.Make(view, iResourceStringID, Snackbar.LengthIndefinite);

            bar.View.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(context, Resource.Color.primary_dark)));

            bar.SetAction(Resource.String.snackbar_OK, (v) => { });
            bar.SetActionTextColor(new Android.Graphics.Color(ContextCompat.GetColor(context, Resource.Color.primary_light)));

            bar.Show();
        }

        public static void Switch_DayMode_NightMode(Activity context)
        {
            Switch_DayMode_NightMode(context, !Settings.NightMode);
        }

        public static void Switch_DayMode_NightMode(Activity context, bool bIsNightMode)
        {
            Settings.NightMode = bIsNightMode;

            if (Settings.NightMode)
                context.SetTheme(Resource.Style.TrajetTramTheme_nightMode);
            else
                context.SetTheme(Resource.Style.TrajetTramTheme_dayMode);

            context.Recreate();
        }

        public static View Inflate_Item_Sport_Level(Activity context, int iLvlMin = 1, int iLvlMax = 4)
        {
            if (context == null)
                return null;

            View v = context.LayoutInflater.Inflate(Resource.Layout.item_display_gpsLevel, null);

            ImageView imageviewEasy = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_easy);
            ImageView imageviewEasyEmpty = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_easyEmpty);
            ImageView imageviewMedium = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_medium);
            ImageView imageviewMediumEmpty = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_mediumEmpty);
            ImageView imageviewHard = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_hard);
            ImageView imageviewHardEmpty = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_hardEmpty);
            ImageView imageviewExtrem = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_extrem);
            ImageView imageviewExtremEmpty = v.FindViewById<ImageView>(Resource.Id.item_display_gpsLevel_imageview_extremEmpty);
            
            //low
            if (iLvlMin <= 1 && iLvlMax >= 1)
            {
                imageviewEasy.Visibility = ViewStates.Visible;
                imageviewEasyEmpty.Visibility = ViewStates.Gone;
            }
            else
            {
                imageviewEasy.Visibility = ViewStates.Gone;
                imageviewEasyEmpty.Visibility = ViewStates.Visible;
            }

            // Low Medium
            if (iLvlMin <= 2 && iLvlMax >= 2)
            {
                imageviewMedium.Visibility = ViewStates.Visible;
                imageviewMediumEmpty.Visibility = ViewStates.Gone;
            }
            else
            {
                imageviewMedium.Visibility = ViewStates.Gone;
                imageviewMediumEmpty.Visibility = ViewStates.Visible;
            }

            //Medium
            if (iLvlMin <= 3 && iLvlMax >= 3)
            {
                imageviewHard.Visibility = ViewStates.Visible;
                imageviewHardEmpty.Visibility = ViewStates.Gone;
            }
            else
            {
                imageviewHard.Visibility = ViewStates.Gone;
                imageviewHardEmpty.Visibility = ViewStates.Visible;
            }

            //Strong
            if (iLvlMin <= 4 && iLvlMax >= 4)
            {
                imageviewExtrem.Visibility = ViewStates.Visible;
                imageviewExtremEmpty.Visibility = ViewStates.Gone;
            }
            else
            {
                imageviewExtrem.Visibility = ViewStates.Gone;
                imageviewExtremEmpty.Visibility = ViewStates.Visible;
            }

            return v;
        }

        public static void InitLightSensorManagers(Activity context, out SensorManager sensorManager, out SensorEventListener sensorListener)
        {
            //Get Sensor Manager
            sensorManager = (SensorManager)context.GetSystemService(Context.SensorService);

            //Create LightSensorEventListener
            sensorListener = new SensorEventListener(context);
        }

        public static void RegisterLightSensor(SensorManager sensorManager, SensorEventListener sensorListener)
        {
            //Get Light Sensor
            Sensor lightSensor = sensorManager.GetDefaultSensor(SensorType.Light);

            //Register Activity to LightSensorEventListener to apply Night theme or Day theme automatically
            sensorManager.RegisterListener(sensorListener, lightSensor, SensorDelay.Game);
        }

        public static void UnregisterLightSensor(SensorManager sensorManager, SensorEventListener sensorListener)
        {
            //Unregister listener
            sensorManager.UnregisterListener(sensorListener);
        }

        public static void ManageNeedActivityLoginException(Activity context)
        {
            Toast.MakeText(context, Resource.String.toast_sessionExpired_needLoginAgain, ToastLength.Long);

            //Logout user
            Settings.UserToken = "";
            Settings.UserPassword = "";

            Intent intent = new Intent(context, typeof(Activity_Login));
            intent.PutExtra("DontStartActivityAfterLoginValid", true);

            context.StartActivity(intent);
        }

        public static void ChangeFABBackgroundColor(Context activity, FloatingActionButton fab, int iResColorBackground, int iResColorIconTint = -1)
        {
            fab.BackgroundTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[] { 0 } }, new int[] { ContextCompat.GetColor(activity, iResColorBackground) });

            if (iResColorIconTint > -1)
                fab.ImageTintList = new Android.Content.Res.ColorStateList(new int[][] { new int[] { 0 } }, new int[] { ContextCompat.GetColor(activity, iResColorIconTint)});
        }

        public static void SetDayNightModeMenu(ref IMenu menu)
        {
            if (!Settings.AutomaticDayNightMode && !Settings.NightMode)
                menu.FindItem(Resource.Id.dashboardActivity_menu_dayMode).SetChecked(true);
            else if (!Settings.AutomaticDayNightMode && Settings.NightMode)
                menu.FindItem(Resource.Id.dashboardActivity_menu_nightMode).SetChecked(true);
            else if (Settings.AutomaticDayNightMode)
                menu.FindItem(Resource.Id.dashboardActivity_menu_automaticNightMode).SetChecked(true);
        }

        public static bool ManageDayNightModeMenuClick(IMenuItem item, Activity context)
        {
            bool bRes = false;

            switch (item.ItemId)
            {
                case Resource.Id.dashboardActivity_menu_dayMode:
                    Settings.AutomaticDayNightMode = false;
                    if (Settings.NightMode)
                        DynamicUIBuild_Utils.Switch_DayMode_NightMode(context);
                    bRes = true;
                    break;

                case Resource.Id.dashboardActivity_menu_nightMode:
                    Settings.AutomaticDayNightMode = false;
                    if (!Settings.NightMode)
                        DynamicUIBuild_Utils.Switch_DayMode_NightMode(context);
                    bRes = true;
                    break;

                case Resource.Id.dashboardActivity_menu_automaticNightMode:
                    Settings.AutomaticDayNightMode = true;
                    Settings.NbMillisecondBeforeSwitchDayNightMode = 1;
                    bRes = true;
                    break;
            }

            if (bRes)
                context.InvalidateOptionsMenu();

            return bRes;
        }
    }
}
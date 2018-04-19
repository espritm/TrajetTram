using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Helpers;
using System.Reflection;
using System;

namespace Trajet_Tram.Droid.Fragments
{
    public class Fragment_Parameters : Android.Support.V4.App.Fragment
    {
        private TextView m_textviewNightMode;
        private Switch m_switchNightMode;
        private TextView m_textviewTime;
        private SeekBar m_seekbarTime;
        private TextView m_textviewDistance;
        private SeekBar m_seekbarDistance;
        private TextView m_textviewRadiusToDetectStop;
        private SeekBar m_seekbarRadiusToDetectStop;
        private TextView m_textviewLuxLight;
        private SeekBar m_seekbarLuxLight;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = null;

            try
            {
                view = inflater.Inflate(Resource.Layout.fragmentParameters, null);

                m_textviewNightMode = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_nightMode);
                m_switchNightMode = view.FindViewById<Switch>(Resource.Id.fragmentParameters_switch_nightMode);
                m_textviewTime = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_minimumTimeGPS);
                m_seekbarTime = view.FindViewById<SeekBar>(Resource.Id.fragmentParameters_seekbar_minimumTimeGPS);
                m_textviewDistance = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_minimumDistanceGPS);
                m_seekbarDistance = view.FindViewById<SeekBar>(Resource.Id.fragmentParameters_seekbar_minimumDistanceGPS);
                m_textviewRadiusToDetectStop = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_minimumRadiusToDetectStop);
                m_seekbarRadiusToDetectStop = view.FindViewById<SeekBar>(Resource.Id.fragmentParameters_seekbar_minimumRadiusToDetectStop);
                m_textviewLuxLight = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_luxLight);
                m_seekbarLuxLight = view.FindViewById<SeekBar>(Resource.Id.fragmentParameters_seekbar_luxLight);

                //Day/Night mode
                m_switchNightMode.Checked = Settings.NightMode;
                m_switchNightMode.CheckedChange += M_switchNightMode_CheckedChange;

                //Tracking GPS
                m_seekbarTime.StartTrackingTouch += M_seekbarTime_StartTrackingTouch;
                m_seekbarTime.StopTrackingTouch += M_seekbarTime_StopTrackingTouch;
                m_seekbarTime.ProgressChanged += M_seekbarTime_ProgressChanged;
                m_seekbarDistance.StartTrackingTouch += M_seekbarDistance_StartTrackingTouch;
                m_seekbarDistance.StopTrackingTouch += M_seekbarDistance_StopTrackingTouch;
                m_seekbarDistance.ProgressChanged += M_seekbarDistance_ProgressChanged;
                m_seekbarRadiusToDetectStop.StartTrackingTouch += M_seekbarRadiusToDetectStop_StartTrackingTouch;
                m_seekbarRadiusToDetectStop.StopTrackingTouch += M_seekbarRadiusToDetectStop_StopTrackingTouch;
                m_seekbarRadiusToDetectStop.ProgressChanged += M_seekbarRadiusToDetectStop_ProgressChanged;
                SetSeekbarGPS();

                //Lux Light to go night mode
                m_seekbarLuxLight.StartTrackingTouch += M_seekbarLuxLight_StartTrackingTouch;
                m_seekbarLuxLight.StopTrackingTouch += M_seekbarLuxLight_StopTrackingTouch;
                m_seekbarLuxLight.ProgressChanged += M_seekbarLuxLight_ProgressChanged;
                m_seekbarLuxLight.Progress = Settings.LuxLightToGoNightMode;
            }
            catch (System.Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (view != null)
                    DynamicUIBuild_Utils.ShowSnackBar(Activity, view, Resource.String.snackbar_errorHappened, Snackbar.LengthLong);
            }

            return view;
        }

        private void SetTextViewLuxLight()
        {
            try
            {
                m_textviewLuxLight.Text = string.Format(Resources.GetString(Resource.String.fragmentParameters_textview_luxLight), Settings.LuxLightToGoNightMode);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_seekbarLuxLight_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            try
            {
                int iValueSelected = e.SeekBar.Progress;

                Settings.LuxLightToGoNightMode = iValueSelected;
                SetTextViewLuxLight();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_seekbarLuxLight_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + e.SeekBar.Progress);
        }

        private void M_seekbarLuxLight_StartTrackingTouch(object sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        private void M_seekbarRadiusToDetectStop_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            try
            {
                int iValueSelected = e.SeekBar.Progress;

                //Minimum is 10 meter
                if (iValueSelected < 10)
                    iValueSelected = 10;

                Settings.MinimumRadiusToDetectStop = iValueSelected;
                SetTextViewGPS();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_seekbarRadiusToDetectStop_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + e.SeekBar.Progress);
        }

        private void M_seekbarRadiusToDetectStop_StartTrackingTouch(object sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        private void M_seekbarDistance_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + e.SeekBar.Progress);
        }

        private void M_seekbarDistance_StartTrackingTouch(object sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        private void M_seekbarTime_StopTrackingTouch(object sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + e.SeekBar.Progress);
        }

        private void M_seekbarTime_StartTrackingTouch(object sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        private void M_seekbarDistance_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            try
            {
                int iValueSelected = e.SeekBar.Progress;

                //Minimum is 1 meter
                if (iValueSelected == 0)
                    iValueSelected = 1;

                Settings.MinimumDistanceGPS = iValueSelected;
                SetTextViewGPS();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_seekbarTime_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            try
            {
                int iValueSelected = e.SeekBar.Progress;

                //Minimum is 1 second
                if (iValueSelected == 0)
                    iValueSelected = 1;

                Settings.MinimumTimeGPS = iValueSelected;
                SetTextViewGPS();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void SetTextViewGPS()
        {
            try
            {
                m_textviewTime.Text = string.Format(Resources.GetString(Resource.String.fragmentParameters_textview_minimumTimeGPS), Settings.MinimumTimeGPS, Settings.MinimumTimeGPS > 1 ? "s" : "");

                m_textviewDistance.Text = string.Format(Resources.GetString(Resource.String.fragmentParameters_textview_minimumDistanceGPS), Settings.MinimumDistanceGPS, Settings.MinimumDistanceGPS > 1 ? "s" : "");

                m_textviewRadiusToDetectStop.Text = string.Format(Resources.GetString(Resource.String.fragmentParameters_textview_minimumRadiusToDetectStop), Settings.MinimumRadiusToDetectStop, Settings.MinimumRadiusToDetectStop > 1 ? "s" : "");
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void SetSeekbarGPS()
        {
            try
            {
                m_seekbarDistance.Progress = (int)Settings.MinimumDistanceGPS;
                m_seekbarTime.Progress = Settings.MinimumTimeGPS;
                m_seekbarRadiusToDetectStop.Progress = Settings.MinimumRadiusToDetectStop;
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_switchNightMode_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + e.IsChecked.ToString());

                DynamicUIBuild_Utils.Switch_DayMode_NightMode(Activity, m_switchNightMode.Checked);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void RestaureDefault()
        {
            try
            {
                Settings.MinimumDistanceGPS = Settings._MinimumDistanceGPSDefault;
                Settings.MinimumTimeGPS = Settings._MinimumTimeGPSDefault;
                Settings.MinimumRadiusToDetectStop = Settings._MinimumRadiusToDetectStopDefault;
                SetSeekbarGPS();

                Settings.LuxLightToGoNightMode = Settings._LuxLightToGoNightModeDefault;
                SetTextViewLuxLight();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
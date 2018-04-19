using System;
using Android.App;
using Android.Runtime;
using Trajet_Tram.Helpers;
using Android.Hardware;
using Android.OS;

namespace Trajet_Tram.Droid.Utils
{
    public class SensorEventListener : Java.Lang.Object, ISensorEventListener
    {
        Activity m_activity;
        private float m_fLastSensorIlluminanceValue;
        private Handler m_handler;
        private System.Action m_workRunnable;

        public SensorEventListener(Activity context)
        {
            m_activity = context;

            m_handler = new Handler(Looper.MainLooper);
            m_workRunnable = () =>
            {
                SwitchIfNeeded();
            };
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (!Settings.AutomaticDayNightMode)
                return;

            m_fLastSensorIlluminanceValue = -1;
            try
            {
                m_fLastSensorIlluminanceValue = e.Values[0];
            }
            catch (Exception)
            { }
            if (m_fLastSensorIlluminanceValue == -1)
                return;

            try
            {
                m_handler.RemoveCallbacks(m_workRunnable);
                m_handler.PostDelayed(m_workRunnable, Settings.NbMillisecondBeforeSwitchDayNightMode);
            }
            catch(Exception)
            { }
        }

        public void SwitchIfNeeded()
        {
            if (m_fLastSensorIlluminanceValue == -1 || !Settings.AutomaticDayNightMode)
                return;

            if ((Settings.NightMode && m_fLastSensorIlluminanceValue >= Settings.LuxLightToGoNightMode + Settings.LuxLightGap) || (!Settings.NightMode && m_fLastSensorIlluminanceValue < Settings.LuxLightToGoNightMode - Settings.LuxLightGap))
            {
                DynamicUIBuild_Utils.Switch_DayMode_NightMode(m_activity);

                Settings.NbMillisecondBeforeSwitchDayNightMode = Settings._NbMillisecondBeforeSwitchDayNightModeDefault;
            }
        }
    }
}
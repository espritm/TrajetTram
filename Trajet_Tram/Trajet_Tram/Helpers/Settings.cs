

using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Trajet_Tram.Helpers
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants
        private const string _UserToken = @"UserToken";
        private const string _UserTokenDefault = "";

        private const string _UserMatricule = @"UserMatricule";
        private const string _UserMatriculeDefault = "";

        private const string _UserPassword = @"UserPassword";
        private const string _UserPasswordDefault = "";

        private const string _NightMode = @"nightMode";
        private const bool _NightModeDefault = false;

        private const string _AutomaticDayNightMode = @"automaticDayNightMode";
        private const bool _AutomaticDayNightModeDefault = true; //TODO : mettre à false pour les test auto

        private const string _AppVersionName = @"AppVersionName";
        private const string _AppVersionNameDefault = "";

        private const string _MinimumTimeGPS = @"MinimumTimeGPS"; 
        public const int _MinimumTimeGPSDefault = 10; //Seconds

        private const string _MinimumDistanceGPS = @"MinimumDistanceGPS";
        public const double _MinimumDistanceGPSDefault = 100.0; //meters 

        private const string _MinimumRadiusToDetectStop = @"MinimumRadiusToDetectStop";
        public const int _MinimumRadiusToDetectStopDefault = 300; //meters

        private const string _LuxLightToGoNightMode = @"LuxLightToGoNightMode";
        public const int _LuxLightToGoNightModeDefault = 101; //lux 

        private const string _LuxLightGap = @"LuxLightGap";
        public const int _LuxLightGapDefault = 10; //lux 

        private const string _NbSelectableDaysAfterToday = @"NbSelectableDaysAfterToday";
        public const int _NbSelectableDaysAfterTodayDefault = 2; 

        private const string _NbSelectableDaysBeforeToday = @"NbSelectableDaysBeforeToday";
        public const int _NbSelectableDaysBeforeTodayDefault = -2;

        private const string _Chapter3_RT_Document_Suffixe = @"Chapter3_RT_Document_Suffixe";
        public const string _Chapter3_RT_Document_SuffixeDefault = "_Chapter_3.pdf"; 

        private const string _NbMillisecondBeforeSwitchDayNightMode = @"nbMillisecondBeforeSwitchDayNightMode";
        public const int _NbMillisecondBeforeSwitchDayNightModeDefault = 4000;
        
        private const string _LogFilePath = @"LogFilePath";
        public const string _LogFilePathDefault = "";

        private const string _LatestArret = @"LatestArret";
        public const string _LatestArretDefault = "";

        #endregion

        public static string UserToken
        {
            get
            {
                return AppSettings.GetValueOrDefault(_UserToken, _UserTokenDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_UserToken, value);
            }
        }

        public static string UserMatricule
        {
            get
            {
                return AppSettings.GetValueOrDefault(_UserMatricule, _UserMatriculeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_UserMatricule, value);
            }
        }

        public static string UserPassword
        {
            get
            {
                return AppSettings.GetValueOrDefault(_UserPassword, _UserPasswordDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_UserPassword, value);
            }
        }

        public static bool NightMode
        {
            get
            {
                return AppSettings.GetValueOrDefault(_NightMode, _NightModeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_NightMode, value);
            }
        }

        public static bool AutomaticDayNightMode
        {
            get
            {
                return AppSettings.GetValueOrDefault(_AutomaticDayNightMode, _AutomaticDayNightModeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_AutomaticDayNightMode, value);
            }
        }

        public static string AppVersionName
        {
            get
            {
                return AppSettings.GetValueOrDefault(_AppVersionName, _AppVersionNameDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_AppVersionName, value);
            }
        }

        public static int MinimumTimeGPS
        {
            get
            {
                return AppSettings.GetValueOrDefault(_MinimumTimeGPS, _MinimumTimeGPSDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_MinimumTimeGPS, value);
            }
        }

        public static double MinimumDistanceGPS
        {
            get
            {
                return AppSettings.GetValueOrDefault(_MinimumDistanceGPS, _MinimumDistanceGPSDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_MinimumDistanceGPS, value);
            }
        }

        public static int MinimumRadiusToDetectStop
        {
            get
            {
                return AppSettings.GetValueOrDefault(_MinimumRadiusToDetectStop, _MinimumRadiusToDetectStopDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_MinimumRadiusToDetectStop, value);
            }
        }

        public static int LuxLightToGoNightMode
        {
            get
            {
                return AppSettings.GetValueOrDefault(_LuxLightToGoNightMode, _LuxLightToGoNightModeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_LuxLightToGoNightMode, value);
            }
        }

        public static int LuxLightGap
        {
            get
            {
                return AppSettings.GetValueOrDefault(_LuxLightGap, _LuxLightGapDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_LuxLightGap, value);
            }
        }

        public static int NbSelectableDaysAfterToday
        {
            get
            {
                return AppSettings.GetValueOrDefault(_NbSelectableDaysAfterToday, _NbSelectableDaysAfterTodayDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_NbSelectableDaysAfterToday, value);
            }
        }

        public static int NbSelectableDaysBeforeToday
        {
            get
            {
                return AppSettings.GetValueOrDefault(_NbSelectableDaysBeforeToday, _NbSelectableDaysBeforeTodayDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_NbSelectableDaysBeforeToday, value);
            }
        }

        public static string Chapter3_RT_Document_Suffixe
        {
            get
            {
                return AppSettings.GetValueOrDefault(_Chapter3_RT_Document_Suffixe, _Chapter3_RT_Document_SuffixeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_Chapter3_RT_Document_Suffixe, value);
            }
        }

        public static int NbMillisecondBeforeSwitchDayNightMode
        {
            get
            {
                return AppSettings.GetValueOrDefault(_NbMillisecondBeforeSwitchDayNightMode, _NbMillisecondBeforeSwitchDayNightModeDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_NbMillisecondBeforeSwitchDayNightMode, value);
            }
        }

        public static string LogFilePath
        {
            get
            {
                return AppSettings.GetValueOrDefault(_LogFilePath, _LogFilePathDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_LogFilePath, value);
            }
        }

        public static string LatestArret
        {
            get
            {
                return AppSettings.GetValueOrDefault(_LatestArret, _LatestArretDefault);
            }
            set
            {
                //if value has changed then save it!
                AppSettings.AddOrUpdateValue(_LatestArret, value);
            }
        }

    }
}

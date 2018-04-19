using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Graphics;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Helpers;
using System.Threading.Tasks;
using System.Reflection;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Trajet_Tram.Droid.Fragments;
using Android.Runtime;
using System.Collections.Generic;
using Trajet_Tram.Droid.Adapters;
using Android.Support.V4.Content;
using Trajet_Tram.BusinessLayer;
using System;
using Plugin.Geolocator.Abstractions;
using Android.Webkit;
using Android.Hardware;
using Plugin.Connectivity;
using Trajet_Tram.Exceptions;
using AndroidHUD;

namespace Trajet_Tram.Droid.Activities
{
    [Activity(Label = "", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class Activity_trajetDetails : AppCompatActivity
    {
        private Trajet m_trajet;
        private int m_itrajetLocalID;
        public RelativeLayout m_layoutRoot { get; private set; }
        private SwipeRefreshLayout m_refresher;
        private ListView m_listview;
        private TextView m_textviewEhouatRefNumber;
        private TextView m_textviewTrain;
        private TextView m_textviewLocoType;
        private TextView m_textviewTonnage;
        private TextView m_textviewRadio;
        private TextView m_textviewLocationStart;
        private TextView m_textviewTimeStart;
        private TextView m_textviewLocationEnd;
        private TextView m_textviewTimeEnd;
        private TextView m_textviewMinutesBeforeStart;
        private TextView m_textviewAltimeter;
        private ImageView m_imageviewAltimeter;
        private LinearLayout m_layoutAltimeter;
        private ListviewAdapter_Display_arret m_adapter;
        public bool m_bIsModeConduite = false;
        public bool m_bIsSimulatorActivated = true;
        public bool m_bHasGPSBeenInit = false;
        public LinearLayout m_layoutGoToNextStep;
        public LinearLayout m_layoutGPS;
        public LinearLayout m_layoutGPSlevel;
        public ImageView m_imageviewGPSStatus;
        public int m_iGPSlevelMax = 4;
        public LinearLayout m_layoutTimeBeforeStart;
        public RelativeLayout m_layoutNextStop;
        public TextView m_textviewNextStopTitle;
        public TextView m_textviewNextStopHour;
        public TextView m_textviewNextStopMinuteAvanceRetard;
        public Button m_btnValidPreparation;
        public Position m_currentGPSPosition;
        public Arret m_currentArret;
        public Arret m_nextArret;
        public WebView m_webviewDocument;
        public FloatingActionButton m_FABFullScreenDocument;
        public FloatingActionButton m_FABAddMarquePage;
        public ImageButton m_btnCloseDocument;
        public RelativeLayout m_layoutDocument;
        public RelativeLayout m_layoutListViewArret;
        public LinearLayout m_layoutTabbarDocumentsNotes;
        public bool m_bIsModeFullScreen;
        public bool m_bIsRestoring;
        private AltiManager m_altiManager = new AltiManager();
        public int m_iSimulatorPosition = 0;
        private SensorManager m_sensorManager;
        private SensorEventListener m_sensorListener;
        private Handler m_handler_UpdateTimeBeforeEhouatStartEveryMinuts;
        private System.Action m_workRunnable_UpdateTimeBeforeEhouatStartEveryMinuts;
        private Handler m_handler_DismissHandHUDIfNeeded;
        private System.Action m_workRunnable_DismissHandHUDIfNeeded;
        private bool m_bHasShownSecurityMessage = false;
        public List<LatLng> m_lsSimulatorPositionTramB = new List<LatLng> {
            new LatLng(45.20444145, 5.70121812), //Grenoble - Presqu'île
            new LatLng(45.19504201, 5.71112132),
            new LatLng(45.19115803, 5.71166441),
            new LatLng(45.19014910, 5.71521212),
            new LatLng(45.19014905, 5.71521223),
            new LatLng(45.18939411, 5.72044621),
            new LatLng(45.18979909, 5.72515211),
            new LatLng(45.19019521, 5.72831008),
            new LatLng(45.19132301, 5.73070510),
            new LatLng(45.19356500, 5.73221540),
            new LatLng(45.19728612, 5.73671204),
            new LatLng(45.20063304, 5.74113912),
            new LatLng(45.20028023, 5.74544709),
            new LatLng(45.19898411, 5.74986810),
            new LatLng(45.19219507, 5.75783621),
            new LatLng(45.19228805, 5.76445608),
            new LatLng(45.19229812, 5.77084311),
            new LatLng(45.18912805, 5.77437703),
            new LatLng(45.18474311, 5.77905521),
            new LatLng(45.18539313, 5.78550310),
            new LatLng(45.18752101, 5.78442605) }; //Plaine des Sports
                
        //Current Stop mode conduite
        public LinearLayout m_layoutCurrentStop;
        public LinearLayout m_layoutCurrentStopInfos;
        public TextView m_textviewCurrentStopName;
        public TextView m_textviewCurrentStopHorairePassage;

        public Activity_trajetDetails() :base()
        {
            m_handler_UpdateTimeBeforeEhouatStartEveryMinuts = new Handler(Looper.MainLooper);
            m_workRunnable_UpdateTimeBeforeEhouatStartEveryMinuts = () =>
            {
                try
                { 
                    //Update time before trajet starts
                    UpdateTimeBeforeStart();
                }
                catch (Exception e)
                {
                    MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, "m_workRunnable_UpdateTimeBeforeEhouatStartEveryMinuts");
                }
            };

            m_handler_DismissHandHUDIfNeeded = new Handler(Looper.MainLooper);
            m_workRunnable_DismissHandHUDIfNeeded = () =>
            {
                try
                {
                    //In case listview has not been reloaded correctly, instantly scroll to the last item and dismiss AndHUD.
                    m_listview.SetSelection(m_adapter.Count - 1);

                    AndHUD.Shared.Dismiss();
                }
                catch (Exception e)
                {
                    MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, "m_workRunnable_DismissHandHUDIfNeeded");
                }
            };
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                View v = CurrentFocus;
                if (v is EditText)
                {
                    Rect outRect = new Rect();
                    v.GetGlobalVisibleRect(outRect);
                    if (!outRect.Contains((int)ev.RawX, (int)ev.RawY))
                    {
                        v.ClearFocus();
                        DynamicUIBuild_Utils.HideKeyboard(this, v);
                    }
                }
            }
            return base.DispatchTouchEvent(ev);
        }

        private void SetTitle()
        {
            if (m_trajet == null)
                return;

            Title = m_trajet.m_sRefNumber;

            if (m_bIsModeConduite)
            {
                if (m_currentArret != null)
                    Title = m_currentArret.m_strajetRefNumber;

                Title += Resources.GetString(Resource.String.activity_trajetDetail_title_modeConduite);

                if (m_bIsSimulatorActivated)
                    Title += Resources.GetString(Resource.String.activity_trajetDetail_title_modeConduiteSimulator);
            }
            else
                Title += Resources.GetString(Resource.String.activity_trajetDetail_title_modePreparation);
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                //Prevent app going into sleep mode
                Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                
                MobileCenter_Helper.InitAndroidAnalytics();

                if (Settings.NightMode)
                    SetTheme(Resource.Style.TrajetTramTheme_actionBar_nightMode);
                else
                    SetTheme(Resource.Style.TrajetTramTheme_actionBar_dayMode);

                //Set our view from the "main" layout resource
                SetContentView(Resource.Layout.activitytrajetDetails);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);
                
                //Get the trajet's local ID
                m_itrajetLocalID = Intent.Extras.GetInt("itrajetLocalID");
                if (m_itrajetLocalID == 0)
                    Finish();

                //Get the mode (preparation or conduite)
                m_bIsModeConduite = Intent.Extras.GetBoolean("IsModeConduite");

                m_layoutRoot = FindViewById<RelativeLayout>(Resource.Id.activitytrajetDetails_layout_root);
                m_refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.activitytrajetDetails_refresher);
                m_listview = FindViewById<ListView>(Resource.Id.activitytrajetDetails_listview);
                m_textviewEhouatRefNumber = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_ehouatRefNumber);
                m_textviewTrain = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_train);
                m_textviewLocoType = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_locoType);
                m_textviewTonnage = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_tonnage);
                m_textviewRadio = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_radio);
                m_textviewLocationStart = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_locationStart);
                m_textviewTimeStart = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_timeStart);
                m_textviewLocationEnd = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_locationEnd);
                m_textviewTimeEnd = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_timeEnd);
                m_textviewMinutesBeforeStart = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_minutesBeforeStart);
                m_textviewAltimeter = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_altimeter);
                m_imageviewAltimeter = FindViewById<ImageView>(Resource.Id.activitytrajetDetails_imageview_altimeter);
                m_layoutAltimeter = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_layout_altimeter);
                m_layoutGoToNextStep = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_layout_GoToNextStep);
                m_layoutGPS = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_layout_GPS);
                m_layoutGPSlevel = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_layout_GPSlevel);
                m_imageviewGPSStatus = FindViewById<ImageView>(Resource.Id.activitytrajetDetails_imageview_GPSStatus);
                m_layoutTimeBeforeStart = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_layout_minutesBeforeStart);
                m_layoutNextStop = FindViewById<RelativeLayout>(Resource.Id.activitytrajetDetails_layout_nextStop);
                m_textviewNextStopTitle = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_nextStop_title);
                m_textviewNextStopHour = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_nextStop_hour);
                m_textviewNextStopMinuteAvanceRetard = FindViewById<TextView>(Resource.Id.activitytrajetDetails_textview_nextStop_miuntesAvanceRetard);
                m_btnValidPreparation = FindViewById<Button>(Resource.Id.activitytrajetDetails_btn_validePreparation);

                //Current Stop mode conduite
                m_layoutCurrentStop = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_modeConduite_layout_currentStop);
                m_layoutCurrentStopInfos = FindViewById<LinearLayout>(Resource.Id.activitytrajetDetails_modeConduite_layout_currentStopInfos);
                m_textviewCurrentStopName = FindViewById<TextView>(Resource.Id.activitytrajetDetails_arret_textview_name);
                m_textviewCurrentStopHorairePassage = FindViewById<TextView>(Resource.Id.activitytrajetDetails_arret_textview_horairePassage);
                
                m_layoutGoToNextStep.Click += M_layoutGoToNextStep_Click;
                m_layoutGPS.Click += M_layoutGPS_Click;
                m_btnValidPreparation.Click += M_btnValidPreparation_Click;
                m_listview.Scroll += M_listview_Scroll;

                m_adapter = new ListviewAdapter_Display_arret(this);
                m_listview.Adapter = m_adapter;
                m_listview.Divider = null;
                m_listview.DividerHeight = 0;

                m_refresher.Refresh += M_refresher_Refresh;
                m_refresher.Post(async () =>
                {
                    await InitUI();
                    
                    //Show Security Message if needed
                    ShowSecurityMessageIfNeeded();

                    //Start GPS tracking if needed
                    if (m_bIsModeConduite && !m_bIsSimulatorActivated)
                    {
                        await GpsHelper.StartListening(OnPositionChanged);
                        m_bHasGPSBeenInit = true;
                    }
                });

                //Init Light Sensor Manager
                DynamicUIBuild_Utils.InitLightSensorManagers(this, out m_sensorManager, out m_sensorListener);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ShowSecurityMessageIfNeeded()
        {
            if (!m_bIsModeConduite || m_bHasShownSecurityMessage)
                return;
            
            new Android.App.AlertDialog.Builder(this)
                .SetTitle(Resource.String.activity_trajetDetail_alertdialog_securityMessage_title)
                .SetMessage(Resource.String.activity_trajetDetail_alertdialog_securityMessage_message)
                .SetPositiveButton(Resource.String.activity_trajetDetail_alertdialog_securityMessage_OK, (s, args) =>
                {
                    //Do nothing
                })
                .SetCancelable(false)
                .Create().Show();

            m_bHasShownSecurityMessage = true;
        }

        private void M_listview_Scroll(object sender, AbsListView.ScrollEventArgs e)
        {
            try
            {
                if (e.TotalItemCount <= 0)
                    return;

                //If the listview is scrolled to bottom, dismiss andHUD (this is the end of the activity initialization)
                int iLastItemShownOnScreen = e.FirstVisibleItem + e.VisibleItemCount;
                
                if (iLastItemShownOnScreen == e.TotalItemCount)
                    AndHUD.Shared.Dismiss();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }
        
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                
                //Register Light Sensor Manager
                DynamicUIBuild_Utils.RegisterLightSensor(m_sensorManager, m_sensorListener);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();

                //Unregister Light Sensor Manager
                DynamicUIBuild_Utils.UnregisterLightSensor(m_sensorManager, m_sensorListener);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override async void OnDestroy()
        {
            try
            {
                base.OnDestroy();

                await GpsHelper.StopListening();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }
        
        private void OnPositionChanged(object sender, PositionEventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);

                //Log position to console...
                m_currentGPSPosition = e.Position;
                string sLog = "\nLat: " + m_currentGPSPosition?.Latitude.ToString() + " Long: " + m_currentGPSPosition?.Longitude.ToString();
                try
                {
                    sLog += "\nTime: " + m_currentGPSPosition?.Timestamp.ToString();
                    sLog += "\nHeading: " + m_currentGPSPosition?.Heading.ToString();
                    sLog += "\nSpeed: " + m_currentGPSPosition?.Speed.ToString();
                    sLog += "\nAccuracy: " + m_currentGPSPosition?.Accuracy.ToString();
                    sLog += "\nAltitude: " + m_currentGPSPosition?.Altitude.ToString();
                    sLog += "\nAltitudeAccuracy: " + m_currentGPSPosition?.AltitudeAccuracy.ToString();
                }
                catch (Exception) { }
                MobileCenter_Helper.TrackGPS(new FileAccessManager(), sLog);

                //DynamicUIBuild_Utils.ShowSnackBar(this, m_layoutRoot, "lat = " + m_currentGPSPosition?.Latitude.ToString() + "\nlon = " + m_currentGPSPosition?.Longitude.ToString(), Snackbar.LengthShort);

                int iGpsLVL = 0;
                if (m_currentGPSPosition?.Accuracy < 10)
                    iGpsLVL = 4;
                else if (m_currentGPSPosition?.Accuracy < 15)
                    iGpsLVL = 3;
                else if (m_currentGPSPosition?.Accuracy < 20)
                    iGpsLVL = 2;
                else
                    iGpsLVL = 1;

                //Altimeter
                m_altiManager.AddPoint(m_currentGPSPosition);

                UpdateGPSlevel(iGpsLVL);
                GoToNextStopIfNeeded();
                UpdateAvanceRetard();
                UpdateAltimeter();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UpdateAltimeter()
        {
            try
            {
                if (m_bIsModeConduite)
                {
                    m_layoutAltimeter.Visibility = ViewStates.Visible;

                    switch (m_altiManager.GetCurrentPente())
                    {
                        case Pente.downhill:
                            m_textviewAltimeter.Text = Resources.GetString(Resource.String.activitytrajetDetails_textview_altimeter_downhill);
                            m_imageviewAltimeter.SetImageResource(Resource.Drawable.ic_trending_down_black_18dp);
                            break;

                        case Pente.rising:
                            m_textviewAltimeter.Text = Resources.GetString(Resource.String.activitytrajetDetails_textview_altimeter_rising);
                            m_imageviewAltimeter.SetImageResource(Resource.Drawable.ic_trending_up_black_18dp);
                            break;

                        case Pente.straight:
                            m_textviewAltimeter.Text = Resources.GetString(Resource.String.activitytrajetDetails_textview_altimeter_straight);
                            m_imageviewAltimeter.SetImageResource(Resource.Drawable.ic_trending_flat_black_18dp);
                            break;

                        default:
                            m_textviewAltimeter.Text = Resources.GetString(Resource.String.activitytrajetDetails_textview_altimeter_unknown);
                            m_imageviewAltimeter.SetImageResource(Resource.Drawable.ic_help_black_18dp);
                            break;
                    }
                }
                else
                {
                    m_layoutAltimeter.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void GoToLatestStopPassedIfNeeded()
        {
            //Verify latest arret passed with that tablet (even if the app has been closed after the arret has been passed)
            Arret latestArret = string.IsNullOrWhiteSpace(Settings.LatestArret) ? null : JSON.DeserializeObject<Arret>(new FileAccessManager(), Settings.LatestArret, "GoToNextStopIfNeeded()");

            //If latest arret passed is of that trajet, go to this arret.
            if (latestArret != null && latestArret.m_iLocalIDtrajet == m_trajet.m_iLocalID)
            {
                m_trajet.GetArretFromLatLon(latestArret.m_lat, latestArret.m_lon, out m_currentArret, out m_nextArret);

                GoToNextStop();
            }
        }

        public void GoToNextStopIfNeeded()
        {
            try
            {
                if (m_currentGPSPosition == null)
                    return;

                int iPreviousArretLocalID = m_currentArret == null ? -1 : m_currentArret.m_iLocalID;
                
                //Recup le arret en fonction de la position gps
                m_trajet.GetArretFromLatLon(m_currentGPSPosition.Latitude, m_currentGPSPosition.Longitude, out m_currentArret, out m_nextArret);

                //update ihm si le arret courant est différent du précédent arret
                if (iPreviousArretLocalID != m_currentArret.m_iLocalID)
                {
                    Settings.LatestArret = JSON.SerializeObject<Arret>(new FileAccessManager(), m_currentArret, "GoToNextStopIfNeeded()");
                    GoToNextStop();
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public async void GoToNextStop()
        {
            try
            {
                //UpdateUI to show next stop
                UpdateLayoutNextStop();

                //Update UI to show current stop informations
                UpdateLayoutCurrentStop();

                //Update Title
                SetTitle();

                //Scroll the list in order to show the next stop at the bottom of the list
                View childItem = m_adapter.GetView(0, null, m_listview);
                childItem.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                m_listview.SmoothScrollBy(-childItem.MeasuredHeight, 500);
                m_layoutGoToNextStep.Enabled = false;

                await Task.Delay(500);

                //Update list of arret to remove old stop
                m_layoutGoToNextStep.Enabled = true;
                m_adapter.RemoveLastItemsUntilCurrentStop(m_currentArret);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_btnValidPreparation_Click(object sender, EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);

                //Update local database
                m_trajet.Prepare(new FileAccessManager());

                //Return to the list of trajets and set the result to OK in order to refresh it
                SetResult(Result.Ok);
                this.Finish();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_layoutGPS_Click(object sender, EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);

                int iStringResourceID = 0;

                if (Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled)
                {
                    if (m_currentGPSPosition == null)
                        iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPS_searching;
                    else
                        switch (m_iGPSlevelMax)
                        {
                            case 0:
                                iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPSlevel_noGPS;
                                break;
                            case 1:
                                iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPSlevel_low;
                                break;
                            case 2:
                                iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPSlevel_lowMedium;
                                break;
                            case 3:
                                iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPSlevel_medium;
                                break;
                            case 4:
                                iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPSlevel_strong;
                                break;
                        }
                }
                else
                    iStringResourceID = Resource.String.activity_trajetDetail_snackbar_GPS_off;

                DynamicUIBuild_Utils.ShowSnackBar(this, m_layoutRoot, iStringResourceID, Snackbar.LengthLong);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_layoutGoToNextStep_Click(object sender, EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (m_nextArret != null )//Empty arret. The ehouat is finished;
                    return;

                //Simule le parcours du tram B 
                LatLng currentSimulatorPosition = m_lsSimulatorPositionTramB[m_iSimulatorPosition++] ?? null;
                
                //Throw GPS On Changed
                if (currentSimulatorPosition != null)
                    OnPositionChanged(null, new PositionEventArgs(new Position { Latitude = currentSimulatorPosition.m_Lat, Longitude = currentSimulatorPosition.m_Lon, Altitude = new Random().Next(300,400), AltitudeAccuracy = 1 }));
                else
                    DynamicUIBuild_Utils.ShowSnackBar(this, m_layoutRoot, Resource.String.snackbar_simulatorEnded, Snackbar.LengthLong);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }
        
        private async void M_refresher_Refresh(object sender, EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);
                
                await InitUI();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            try
            {
                base.OnSaveInstanceState(outState);
                
                if (m_itrajetLocalID != 0)
                    outState.PutInt("itrajetLocalID", m_itrajetLocalID);

                outState.PutBoolean("IsModeConduite", m_bIsModeConduite);

                outState.PutBoolean("IsSimulatorActivated", m_bIsSimulatorActivated);

                if (m_currentArret != null)
                    outState.PutInt("CurrentArret_localID", m_currentArret.m_iLocalID);

                if (m_nextArret != null)
                    outState.PutInt("NextArret_localID", m_nextArret.m_iLocalID);
                
                outState.PutBoolean("bHasShownSecurityMessage", m_bHasShownSecurityMessage);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            try
            {
                base.OnRestoreInstanceState(savedInstanceState);
                m_bIsRestoring = true;

                m_itrajetLocalID = savedInstanceState.GetInt("itrajetLocalID");
                if (m_itrajetLocalID == 0)
                    Finish();

                m_bIsModeConduite = savedInstanceState.GetBoolean("IsModeConduite");

                m_bIsSimulatorActivated = savedInstanceState.GetBoolean("IsSimulatorActivated");

                int iCurrentArret_localID = savedInstanceState.GetInt("CurrentArret_localID", -1);
                m_currentArret = LocalDatabase.Get().GetArretFromLocalID(new FileAccessManager(), iCurrentArret_localID);
                
                int iNextArret_localID = savedInstanceState.GetInt("NextArret_localID", -1);
                m_nextArret = LocalDatabase.Get().GetArretFromLocalID(new FileAccessManager(), iNextArret_localID);
                
                m_bHasShownSecurityMessage = savedInstanceState.GetBoolean("bHasShownSecurityMessage");
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", " + item?.TitleFormatted?.ToString());

                DynamicUIBuild_Utils.ManageDayNightModeMenuClick(item, this);

                switch (item.ItemId)
                {
                    case Android.Resource.Id.Home:
                        OnBackPressed();
                        return true;

                    case Resource.Id.dashboardActivity_menu_simulator:
                        m_bIsSimulatorActivated = !m_bIsSimulatorActivated;
                        UpdateGpsLayout();

                        if (!m_bIsSimulatorActivated && !m_bHasGPSBeenInit)
                        {
                            GpsHelper.StartListening(OnPositionChanged);
                            m_bHasGPSBeenInit = true;
                        }
                        return true;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnOptionsItemSelected(item);
        }

        public void UpdateGpsLayout()
        {
            try
            {
                SetTitle();

                if (m_bIsModeConduite)
                {
                    if (m_bIsSimulatorActivated)
                    {
                        //Show arrow to Right to go to next point
                        m_layoutGoToNextStep.Visibility = ViewStates.Visible;
                        m_layoutGPS.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        //Show icon gps and update bars
                        m_layoutGoToNextStep.Visibility = ViewStates.Gone;
                        m_layoutGPS.Visibility = ViewStates.Visible;

                        UpdateGPSlevel();
                    }
                }
                else
                {
                    m_layoutGoToNextStep.Visibility = ViewStates.Gone;
                    m_layoutGPS.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UpdateGpsStatusIcon()
        {
            try
            {
                //If GPS is activated on phone
                if (Plugin.Geolocator.CrossGeolocator.Current.IsGeolocationEnabled)
                {
                    if (m_currentGPSPosition == null)
                        m_imageviewGPSStatus.SetImageResource(Resource.Drawable.ic_gps_not_fixed_white_24dp);
                    else
                        m_imageviewGPSStatus.SetImageResource(Resource.Drawable.ic_gps_fixed_white_24dp);
                }
                else
                    m_imageviewGPSStatus.SetImageResource(Resource.Drawable.ic_gps_off_white_24dp);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UpdateGPSlevel(int iLvlMax = 0)
        {
            try
            {
                UpdateGpsStatusIcon();

                m_iGPSlevelMax = iLvlMax;

                m_layoutGPSlevel.RemoveAllViews();
                m_layoutGPSlevel.AddView(DynamicUIBuild_Utils.Inflate_Item_Sport_Level(this, 0, m_iGPSlevelMax));
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UpdateLayoutNextStop()
        {
            try
            {
                UpdateEhouatInfos();

                if (m_bIsModeConduite)
                {
                    m_layoutNextStop.Visibility = ViewStates.Visible;

                    //Name of next stop
                    m_textviewNextStopTitle.Text = m_nextArret?.m_sName ?? "";

                    //Arrival hour or passage hour of next stop
                    if (m_nextArret != null && m_nextArret.m_sTime != "")
                        m_textviewNextStopHour.Text = m_nextArret.m_sTime;
                    else
                        m_textviewNextStopHour.Text = m_nextArret?.m_sStopTime ?? "";

                    //Avance or retard
                    UpdateAvanceRetard();
                }
                else
                    m_layoutNextStop.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UpdateAvanceRetard()
        {
            try
            {
                if (!m_bIsModeConduite)
                    return;

                TimeSpan diffBewteenArriveeTheoriqueAndNow;

                if (m_currentArret == null)
                    diffBewteenArriveeTheoriqueAndNow = m_nextArret.GetNbMinutsAvance(m_trajet.m_datetrajet);
                else
                    diffBewteenArriveeTheoriqueAndNow = m_currentArret.GetNbMinutsAvance(m_trajet.m_datetrajet);

                if (Convert.ToInt32(diffBewteenArriveeTheoriqueAndNow.TotalMinutes) == 0)
                {
                    m_textviewNextStopMinuteAvanceRetard.SetTextColor(Resources.GetColor(Resource.Color.green));
                    m_textviewNextStopMinuteAvanceRetard.Text = string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_nextStop_minutesAvanceRetard), "", ((int)diffBewteenArriveeTheoriqueAndNow.TotalMinutes));
                }
                else if (Convert.ToInt32(diffBewteenArriveeTheoriqueAndNow.TotalMinutes) > 0)
                {
                    m_textviewNextStopMinuteAvanceRetard.SetTextColor(Resources.GetColor(Resource.Color.green));
                    m_textviewNextStopMinuteAvanceRetard.Text = string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_nextStop_minutesAvanceRetard), "-", ((int)diffBewteenArriveeTheoriqueAndNow.TotalMinutes).ToString());
                }
                else
                {
                    m_textviewNextStopMinuteAvanceRetard.SetTextColor(Resources.GetColor(Resource.Color.red_error));
                    m_textviewNextStopMinuteAvanceRetard.Text = string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_nextStop_minutesAvanceRetard), "+", ((int)diffBewteenArriveeTheoriqueAndNow.TotalMinutes).ToString().Replace("-", ""));
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public void UpdateLayoutCurrentStop()
        {
            try
            {
                if (m_layoutCurrentStop == null)
                    return;

                if (m_bIsModeConduite)
                {
                    m_layoutCurrentStop.Visibility = ViewStates.Visible;

                    //canal radio
                    if (m_currentArret != null && m_currentArret.m_sRadioCanal != null && m_textviewRadio != null)
                        m_textviewRadio.Text = string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_radio), m_currentArret.m_sRadioCanal);
                    

                    //Activity Title
                    SetTitle();

                    //Infos
                    if (m_textviewCurrentStopName != null)
                        m_textviewCurrentStopName.Text = m_currentArret?.m_sName;
                    if (m_textviewCurrentStopHorairePassage != null)
                        m_textviewCurrentStopHorairePassage.Text = m_currentArret?.m_sTime;
                        
                    //Background & Text color
                    if (Settings.NightMode)
                    {
                        if (m_layoutCurrentStopInfos != null)
                            m_layoutCurrentStopInfos.SetBackgroundColor(Color.Transparent);
                        if (m_textviewCurrentStopName != null)
                            m_textviewCurrentStopName.SetTextColor(Color.White);
                        if (m_textviewCurrentStopHorairePassage != null)
                            m_textviewCurrentStopHorairePassage.SetTextColor(Color.White);
                    }
                    else
                    {
                        if (m_layoutCurrentStopInfos != null)
                            m_layoutCurrentStopInfos.SetBackgroundColor(Resources.GetColor(Resource.Color.white));
                        if (m_textviewCurrentStopName != null)
                            m_textviewCurrentStopName.SetTextColor(Color.Black);
                        if (m_textviewCurrentStopHorairePassage != null)
                            m_textviewCurrentStopHorairePassage.SetTextColor(Color.Black);
                    }
                }
                else
                    m_layoutCurrentStop.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            try
            {
                IMenuItem menuitemSimulator = menu.FindItem(Resource.Id.dashboardActivity_menu_simulator);
                menuitemSimulator?.SetVisible(m_bIsModeConduite);

                DynamicUIBuild_Utils.SetDayNightModeMenu(ref menu);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.menu_activityTrajetDetails, menu);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnCreateOptionsMenu(menu);
        }

        private void UpdateEhouatInfos()
        {
            try
            {
                if (m_currentArret == null)
                {
                    m_textviewEhouatRefNumber.Text = m_trajet.m_sRefNumber;
                    m_textviewTrain.Text = m_trajet.m_sTrainType;
                    m_textviewLocoType.Text = m_trajet.m_sLocoType;
                    m_textviewTonnage.Text = m_trajet.m_sWeight + "T";
                }
                else
                {
                    m_textviewEhouatRefNumber.Text = m_currentArret.m_strajetRefNumber;
                    m_textviewTrain.Text = m_trajet.m_sTrainType;
                    m_textviewLocoType.Text = m_currentArret.m_sLocoType;
                    m_textviewTonnage.Text = m_currentArret.m_sWeight + "T";
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UpdateTimeBeforeStart()
        {
            try
            {
                string sHourMinutLeftBeforeStart = m_trajet.GetLibelleHourMinutLeftBeforeStart();
                string stextviewMinutesBeforeStartText = sHourMinutLeftBeforeStart == "" ? "" : string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_minutesBeforeStart), sHourMinutLeftBeforeStart);

                //Time before start
                if (stextviewMinutesBeforeStartText == "")
                    m_layoutTimeBeforeStart.Visibility = ViewStates.Gone;
                else
                {
                    m_textviewMinutesBeforeStart.Text = stextviewMinutesBeforeStartText;
                    m_layoutTimeBeforeStart.Visibility = ViewStates.Visible;

                    //Will update again in 10 seconds
                    m_handler_UpdateTimeBeforeEhouatStartEveryMinuts.PostDelayed(m_workRunnable_UpdateTimeBeforeEhouatStartEveryMinuts, 10000);
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, "m_workRunnable_UpdateTimeBeforeEhouatStartEveryMinuts");
            }
        }

        private async Task InitUI()
        {
            m_refresher.Refreshing = true;
            AndHUD.Shared.Show(this, Resources.GetString(Resource.String.loading_in_progress), -1, MaskType.Black);
            m_handler_DismissHandHUDIfNeeded.RemoveCallbacks(m_workRunnable_DismissHandHUDIfNeeded);

            //Get trajet from local database
            try
            {
                m_trajet = await Trajet.GettrajetDetailsToShowOnScreen(new FileAccessManager(), m_itrajetLocalID);
                
                //To never get empty listview
                while (m_trajet.m_lsarret.Count < 22)
                    m_trajet.m_lsarret.Add(new Arret());
                
                m_trajet.m_lsarret.Reverse();
            }
            catch (Exception)
            {
                DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_errorHappened);

                if (m_trajet == null || m_trajet.m_lsarret == null)
                {
                    m_refresher.Refreshing = false;
                    return;
                }
            }

            try
            {                
                string stextviewLocationStartText = m_trajet.m_sLocationStart;
                string stextviewTimeStartText = m_trajet.m_sTimeStart;
                string stextviewLocationEndText = m_trajet.m_sLocationEnd;
                string stextviewTimeEndText = m_trajet.m_sTimeEnd;
                
                //update ui (add empty items if mode conduite to avoid showing white screen when there is not enouth Arret)
                List<Arret> tmp = new List<Arret>();
                if (m_bIsModeConduite)
                    for (int i = 0; i < 20; i++)
                        tmp.Add(new Arret());
                tmp.AddRange(m_trajet.m_lsarret);
                m_adapter.RefreshListArret(tmp);
                UpdateGpsLayout();
                UpdateAltimeter();
                
                //Start
                m_textviewLocationStart.Text = stextviewLocationStartText;
                m_textviewTimeStart.Text = stextviewTimeStartText;

                //End
                m_textviewLocationEnd.Text = stextviewLocationEndText;
                m_textviewTimeEnd.Text = stextviewTimeEndText;

                //Time before start
                UpdateTimeBeforeStart();
                
                //Canal radio.... always hiden for the moment (reunion début Mars 2018
                m_textviewRadio.Text = string.Format(Resources.GetString(Resource.String.activitytrajetDetails_textview_radio), m_trajet.m_lsarret[0]?.m_sRadioCanal);

                //Next stop and Current stop
                if (m_bIsModeConduite && m_nextArret == null)
                    m_nextArret = m_adapter[m_adapter.Count - 1];
                UpdateLayoutNextStop();
                UpdateLayoutCurrentStop();

                //Btn valid preparation
                if (m_bIsModeConduite)
                    m_btnValidPreparation.Visibility = ViewStates.Gone;
                else
                    m_btnValidPreparation.Visibility = ViewStates.Visible;
                
                //Disable listview scrolling
                if (m_bIsModeConduite)
                    m_listview.Enabled = false;
                else
                    m_listview.Enabled = true;

                //If activity has been reloading, remove all old Arret
                if (m_currentArret != null)
                    m_adapter.RemoveLastItemsUntilCurrentStop(m_currentArret);
                
                //In every case, scroll the list view to the very bottom
                m_listview.SmoothScrollToPosition(m_adapter.Count);
                                
                //Verify if a arret has already been passed in that trajet. If yes, go to it directly
                GoToLatestStopPassedIfNeeded();
            }
            catch (Exception e)
            {
                DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_errorHappened);
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            finally
            {
                m_refresher.Refreshing = false;
                m_handler_DismissHandHUDIfNeeded.PostDelayed(m_workRunnable_DismissHandHUDIfNeeded, 15000);
            }
        }
    }
}
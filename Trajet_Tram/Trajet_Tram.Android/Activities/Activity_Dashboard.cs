using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Support.V4.View;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Reflection;
using System;
using Android.Graphics;
using Trajet_Tram.Helpers;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Droid.Fragments;
using Trajet_Tram.Droid.Activities;
using Android.Support.V4.Content;
using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Exceptions;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Content.Res;
using Java.IO;
using System.IO;
using Android.Hardware;
using AndroidHUD;
using System.Threading.Tasks;

namespace Trajet_Tram.Droid
{
    [Activity(Label = "", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.SensorLandscape, LaunchMode = LaunchMode.SingleTop)]
    public class Activity_Dashboard : AppCompatActivity
    {
        private static string FRAGMENT_PARAMETER = "Fragment_Parameters";
        private static string FRAGMENT_LISTtrajetS = "Fragment_Listtrajets";
        private Android.Support.V7.Widget.Toolbar m_toolbar;
        private FrameLayout m_frameLayout;
        private DrawerLayout m_drawerLayout;
        private NavigationView m_navigationView;
        private FloatingActionButton m_FAB;
        private TextView m_textviewToolbarTitle;
        private LinearLayout m_layouttrajetNumber;
        private LinearLayout m_layoutCalendar;
        private TextInputEditText m_edittexttrajetNumber;
        private TextInputEditText m_edittextCalendar;
        private TextInputLayout m_textinputlayouttrajetNumber;
        private TextInputLayout m_textinputlayoutCalendar;
        private bool m_isCreatingNewtrajet = false;
        private DateTime m_selectedDateToCreatetrajet;
        private Fragment_Listtrajets m_fragmentListtrajets;
        private Fragment_Parameters m_fragmentParameters;
        private string m_sCurrentFragmentTag = "";
        private SensorManager m_sensorManager;
        private SensorEventListener m_sensorListener;

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                base.OnCreate(bundle);

                MobileCenter_Helper.InitAndroidAnalytics();

                if (Settings.NightMode)
                    SetTheme(Resource.Style.TrajetTramTheme_nightMode);
                else
                    SetTheme(Resource.Style.TrajetTramTheme_dayMode);

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.dashboardActivity);

                m_toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.dashboardActivity_toolbar);
                m_drawerLayout = FindViewById<DrawerLayout>(Resource.Id.dashboardActivity_drawerlayout);
                m_navigationView = FindViewById<NavigationView>(Resource.Id.dashboardActivity_navigationView);
                m_FAB = FindViewById<FloatingActionButton>(Resource.Id.dashboardActivity_FAB);
                m_textviewToolbarTitle = FindViewById<TextView>(Resource.Id.dashboardActivity_toolbar_textviewTitle);
                m_layouttrajetNumber = FindViewById<LinearLayout>(Resource.Id.dashboardActivity_layout_trajetNumber);
                m_layoutCalendar = FindViewById<LinearLayout>(Resource.Id.dashboardActivity_layout_calendar);
                m_edittexttrajetNumber = FindViewById<TextInputEditText>(Resource.Id.dashboardActivity_textInputEditText_trajetNumber);
                m_edittextCalendar = FindViewById<TextInputEditText>(Resource.Id.dashboardActivity_textInputEditText_calendar);
                m_textinputlayouttrajetNumber = FindViewById<TextInputLayout>(Resource.Id.dashboardActivity_textInputLayout_trajetNumber);
                m_textinputlayoutCalendar = FindViewById<TextInputLayout>(Resource.Id.dashboardActivity_textInputLayout_calendar);
                m_frameLayout = FindViewById<FrameLayout>(Resource.Id.dashboardActivity_frameLayout);

                //Navigation view
                m_navigationView.NavigationItemSelected += M_navigationView_NavigationItemSelected;
                ConfigureNavigationViewHeader();

                //Actionbar
                SetSupportActionBar(m_toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);

                //FAB action
                m_FAB.Click += M_FAB_Click;

                //Choose date action
                m_edittextCalendar.FocusChange += M_edittextCalendar_FocusChange;
                m_edittextCalendar.Click += M_edittextCalendar_Click;

                UpdateUIToShowListOrCreateNewtrajet();

                //Error management for EditText during trajet creation
                m_edittextCalendar.TextChanged += M_edittextCalendar_TextChanged;
                m_edittexttrajetNumber.TextChanged += M_edittexttrajetNumber_TextChanged;

                //Init Light Sensor Manager
                DynamicUIBuild_Utils.InitLightSensorManagers(this, out m_sensorManager, out m_sensorListener);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                AskForPermissions.askForWriteExternalStoragePermission(this);

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

        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            try
            {
                base.OnSaveInstanceState(savedInstanceState);

                savedInstanceState.PutString("sCurrentFragmentTag", m_sCurrentFragmentTag);
                savedInstanceState.PutBoolean("isCreatingNewtrajet", m_isCreatingNewtrajet);

                if (m_isCreatingNewtrajet)
                {
                    savedInstanceState.PutString("edittexttrajetNumberText", m_edittexttrajetNumber.Text);
                    savedInstanceState.PutString("selectedDateToCreatetrajet", m_edittextCalendar.Text.Trim() == "" ? "" : m_selectedDateToCreatetrajet.ToString());
                }
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

                m_isCreatingNewtrajet = savedInstanceState.GetBoolean("isCreatingNewtrajet");

                if (m_isCreatingNewtrajet)
                {
                    UpdateUIToShowListOrCreateNewtrajet();

                    string sEdittexttrajetNumberText = savedInstanceState.GetString("edittexttrajetNumberText");
                    string sSelectedDateToCreatetrajet = savedInstanceState.GetString("selectedDateToCreatetrajet");

                    if (sEdittexttrajetNumberText.Trim() != "")
                        m_edittexttrajetNumber.Text = sEdittexttrajetNumberText;

                    if (sSelectedDateToCreatetrajet.Trim() != "")
                        SetDateToCreatetrajetAndUpdateUI(DateTime.Parse(sSelectedDateToCreatetrajet));
                }
                else
                {
                    string sFragmToLoad = savedInstanceState.GetString("sCurrentFragmentTag");

                    if (sFragmToLoad == FRAGMENT_PARAMETER)
                        GoToFragment(FragmentEnum.Parameters);
                    else if (sFragmToLoad == FRAGMENT_LISTtrajetS)
                        GoToFragment(FragmentEnum.Listtrajets);
                    else
                        GoToFragment(FragmentEnum.RemoveFragments);
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            try
            {
                //From Activity_trajetDetails
                if (requestCode == 42 && resultCode == Result.Ok)
                {
                    await m_fragmentListtrajets.RefreshtrajetList();

                    //Show a validation message on screen
                    DynamicUIBuild_Utils.ShowSnackBar(this, m_drawerLayout, Resource.String.activity_trajetDetail_snackbar_btn_ValidPreparation, Snackbar.LengthShort);
                }

                //From AddtrajetForTest
                if (requestCode == 48 && resultCode == Result.Ok)
                {
                    AddtrajetForTest();
                }

                base.OnActivityResult(requestCode, resultCode, data);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            try
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
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.DispatchTouchEvent(ev);
        }

        public void GoToFragment(FragmentEnum position)
        {
            try
            {
                switch (position)
                {
                    case FragmentEnum.Listtrajets:
                        m_fragmentListtrajets = new Fragment_Listtrajets();

                        SupportFragmentManager.BeginTransaction().Replace(Resource.Id.dashboardActivity_frameLayout, m_fragmentListtrajets, FRAGMENT_LISTtrajetS).CommitAllowingStateLoss();

                        m_textviewToolbarTitle.Text = Resources.GetString(Resource.String.dashboardActivity_name);
                        m_navigationView.SetCheckedItem(Resource.Id.leftmenu_dashboard);
                        m_FAB.Visibility = ViewStates.Visible;

                        m_sCurrentFragmentTag = FRAGMENT_LISTtrajetS;
                        break;

                    case FragmentEnum.Parameters:
                        m_fragmentParameters = new Fragment_Parameters();

                        SupportFragmentManager.BeginTransaction().Replace(Resource.Id.dashboardActivity_frameLayout, m_fragmentParameters, FRAGMENT_PARAMETER).CommitAllowingStateLoss();

                        m_textviewToolbarTitle.Text = Resources.GetString(Resource.String.leftmenu_parameters);
                        m_navigationView.SetCheckedItem(Resource.Id.leftmenu_parameters);
                        m_FAB.Visibility = ViewStates.Gone;

                        m_sCurrentFragmentTag = FRAGMENT_PARAMETER;
                        break;

                    case FragmentEnum.RemoveFragments:
                        m_frameLayout.RemoveAllViews();
                        if (m_sCurrentFragmentTag != "")
                            SupportFragmentManager.BeginTransaction().Remove(SupportFragmentManager.FindFragmentByTag(m_sCurrentFragmentTag));

                        m_textviewToolbarTitle.Text = "";
                        m_navigationView.SetCheckedItem(Resource.Id.leftmenu_dashboard);
                        m_FAB.Visibility = ViewStates.Visible;

                        m_sCurrentFragmentTag = "";
                        break;
                }

                InvalidateOptionsMenu();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override void OnBackPressed()
        {
            try
            {
                if (m_sCurrentFragmentTag == FRAGMENT_PARAMETER)
                    GoToFragment(FragmentEnum.Listtrajets);
                else if (m_isCreatingNewtrajet)
                {
                    m_isCreatingNewtrajet = !m_isCreatingNewtrajet;

                    UpdateUIToShowListOrCreateNewtrajet();
                }
                else if (m_drawerLayout.IsDrawerOpen(GravityCompat.Start))
                    m_drawerLayout.CloseDrawers();
                else
                    base.OnBackPressed();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittexttrajetNumber_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_trajetNumber_Error();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextCalendar_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_Calendar_Error();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextCalendar_Click(object sender, EventArgs e)
        {
            try
            {
                m_edittextCalendar.Enabled = false;

                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_edittextCalendar_Click");

                DateTime maxDate = DateTime.Now.Date.AddDays(Settings.NbSelectableDaysAfterToday);
                DateTime minDate = DateTime.Now.Date.AddDays(Settings.NbSelectableDaysBeforeToday);
                DateTime firstJanuary1970 = new DateTime(1970, 1, 1, 0, 0, 0);

                //Prompt a calendar
                DatePickerDialog datePicker = new DatePickerDialog(this, Resource.Style.DatePicker_Red_dayMode, OnDateSetDateBegin, DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.Now.Day);
                datePicker.UpdateDate(DateTime.Now);
                datePicker.DatePicker.MaxDate = (long)maxDate.Subtract(firstJanuary1970).TotalMilliseconds;
                datePicker.DatePicker.MinDate = (long)minDate.Subtract(firstJanuary1970).TotalMilliseconds;
                datePicker.DismissEvent += (senser, eventArgs) => { m_edittextCalendar.Enabled = true; };
                datePicker.Show();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void OnDateSetDateBegin(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "OnDateSetDateBegin");

                SetDateToCreatetrajetAndUpdateUI(e.Date.Date);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void SetDateToCreatetrajetAndUpdateUI(DateTime date)
        {
            try
            {
                m_selectedDateToCreatetrajet = date;

                RefreshDisplayDateBegin();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void RefreshDisplayDateBegin()
        {
            try
            {
                if (m_selectedDateToCreatetrajet == null)
                    m_edittextCalendar.Text = "";
                else
                    m_edittextCalendar.Text = m_selectedDateToCreatetrajet.ToString(Resources.GetString(Resource.String.do_not_translate_dateTime_toString_DayMonthYear));
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextCalendar_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                    M_edittextCalendar_Click(sender, null);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_FAB_Click(object sender, System.EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_FAB_Click");

                m_isCreatingNewtrajet = !m_isCreatingNewtrajet;

                UpdateUIToShowListOrCreateNewtrajet();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UpdateUIToShowListOrCreateNewtrajet()
        {
            try
            {
                InvalidateOptionsMenu();

                if (m_isCreatingNewtrajet)
                {
                    //Remove Fragment
                    GoToFragment(FragmentEnum.RemoveFragments);

                    //Toolbar and buttons visibility
                    m_textviewToolbarTitle.Visibility = ViewStates.Gone;
                    m_FAB.Visibility = ViewStates.Gone;
                    m_layouttrajetNumber.Visibility = ViewStates.Visible;
                    m_layoutCalendar.Visibility = ViewStates.Visible;

                    //Today date is preselected
                    SetDateToCreatetrajetAndUpdateUI(DateTime.Now.Date);

                    //Left Menu icon
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_clear_white_24dp);
                }
                else
                {
                    //Refresh UI
                    if (m_edittextCalendar.Text.Trim() != "")
                        m_edittextCalendar.Text = "";
                    m_textinputlayoutCalendar.ErrorEnabled = false;
                    m_textinputlayoutCalendar.Error = null;
                    if (m_edittexttrajetNumber.Text.Trim() != "")
                        m_edittexttrajetNumber.Text = "";
                    m_textinputlayouttrajetNumber.ErrorEnabled = false;
                    m_textinputlayouttrajetNumber.Error = null;
                    m_selectedDateToCreatetrajet = new DateTime();

                    //Load fragment Listtrajets
                    GoToFragment(FragmentEnum.Listtrajets);

                    //Toolbar and buttons visibility
                    m_textviewToolbarTitle.Visibility = ViewStates.Visible;
                    m_FAB.Visibility = ViewStates.Visible;
                    m_layoutCalendar.Visibility = ViewStates.Gone;
                    m_layouttrajetNumber.Visibility = ViewStates.Gone;

                    //Left Menu icon
                    SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu_white_24dp);
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_navigationView_NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            try
            {
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.leftmenu_dashboard:
                        GoToFragment(FragmentEnum.Listtrajets);
                        break;

                    case Resource.Id.leftmenu_parameters:
                        GoToFragment(FragmentEnum.Parameters);
                        break;

                    case Resource.Id.leftmenu_disconnect:
                        Settings.UserToken = "";
                        Settings.UserPassword = "";
                        StartActivity(typeof(Activity_Login));
                        Finish();
                        break;
                }
                m_drawerLayout.CloseDrawer(GravityCompat.Start);
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ConfigureNavigationViewHeader()
        {
            try
            {
                View viewHeader = LayoutInflater.Inflate(Resource.Layout.dashboardActivity_navigationView_header, null);

                ImageView imageviewUserAvatar = viewHeader.FindViewById<ImageView>(Resource.Id.dashboardActivity_navigationView_header_imageview_userAvatar);
                TextView textviewUserMatricule = viewHeader.FindViewById<TextView>(Resource.Id.dashboardActivity_navigationView_header_textview_userMatricule);

                imageviewUserAvatar.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                Android.Support.V4.Graphics.Drawable.DrawableCompat.SetTint(imageviewUserAvatar.Drawable, ContextCompat.GetColor(Application.Context, Resource.Color.primary_dark));
                textviewUserMatricule.Text = Settings.UserMatricule;

                m_navigationView.AddHeaderView(viewHeader);
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
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "OnOptionsItemSelected " + item?.TitleFormatted?.ToString());

                DynamicUIBuild_Utils.ManageDayNightModeMenuClick(item, this);

                switch (item.ItemId)
                {
                    //Handle sandwich menu icon click
                    case Android.Resource.Id.Home:
                        ActionToDoWhenHomePressed();
                        break;

                    case Resource.Id.dashboardActivity_menu_save:
                        AddNewtrajet();
                        break;

                    case Resource.Id.dashboardActivity_menu_addtrajetTest:
                        AddtrajetForTest();
                        break;

                    case Resource.Id.dashboardActivity_menu_restoreDefault:
                        m_fragmentParameters.RestaureDefault();
                        break;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnOptionsItemSelected(item);
        }

        private async void AddNewtrajet()
        {
            try
            {
                if (IsEditText_trajetNumber_Error() || IsEditText_Calendar_Error())
                    return;

                AndHUD.Shared.Show(this, Resources.GetString(Resource.String.loading_in_progress), -1, MaskType.Black);

                await Task.Delay(2000);

                DynamicUIBuild_Utils.ShowSnackBar(this, m_drawerLayout, "Pas de webservice dans cette version :)\nUtiliser la trajet de test.", Snackbar.LengthLong);

                AndHUD.Shared.Dismiss();
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ActionToDoWhenHomePressed()
        {
            try
            {
                if (m_isCreatingNewtrajet)
                {
                    m_isCreatingNewtrajet = !m_isCreatingNewtrajet;

                    UpdateUIToShowListOrCreateNewtrajet();
                }
                else
                {
                    //If menu is open, close it. Else, open it.
                    if (m_drawerLayout.IsDrawerOpen(GravityCompat.Start))
                        m_drawerLayout.CloseDrawers();
                    else
                        m_drawerLayout.OpenDrawer(GravityCompat.Start);
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            try
            {
                MenuInflater.Inflate(Resource.Menu.menu_activityDashboard, menu);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            try
            {
                IMenuItem menuitemSave = menu.FindItem(Resource.Id.dashboardActivity_menu_save);
                if (m_isCreatingNewtrajet)
                    menuitemSave?.SetVisible(true);
                else
                    menuitemSave?.SetVisible(false);

                IMenuItem menuitemDayNightMode = menu.FindItem(Resource.Id.dashboardActivity_menu_nightdaymode);
                if (m_sCurrentFragmentTag == FRAGMENT_PARAMETER)
                    menuitemDayNightMode?.SetVisible(false);
                else
                {
                    menuitemDayNightMode?.SetVisible(true);
                    DynamicUIBuild_Utils.SetDayNightModeMenu(ref menu);
                }

                IMenuItem menuitemRestoreDefault = menu.FindItem(Resource.Id.dashboardActivity_menu_restoreDefault);
                if (m_sCurrentFragmentTag == FRAGMENT_PARAMETER)
                    menuitemRestoreDefault?.SetVisible(true);
                else
                    menuitemRestoreDefault?.SetVisible(false);

                IMenuItem menuItemAddtrajetForTest = menu.FindItem(Resource.Id.dashboardActivity_menu_addtrajetTest);
                menuItemAddtrajetForTest?.SetVisible(true);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

                return base.OnPrepareOptionsMenu(menu);
        }

        private bool IsEditText_trajetNumber_Error()
        {
            try
            {
                if (m_edittexttrajetNumber.Text.Trim() == "")
                {
                    m_textinputlayouttrajetNumber.ErrorEnabled = true;
                    m_textinputlayouttrajetNumber.Error = Resources.GetString(Resource.String.dashboardActivity_error_trajetNumberEmpty);
                    return true;
                }
                else
                {
                    m_textinputlayouttrajetNumber.ErrorEnabled = false;
                    m_textinputlayouttrajetNumber.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }

        private bool IsEditText_Calendar_Error()
        {
            try
            {
                if (m_edittextCalendar.Text.Trim() == "")
                {
                    m_textinputlayoutCalendar.ErrorEnabled = true;
                    m_textinputlayoutCalendar.Error = Resources.GetString(Resource.String.dashboardActivity_error_trajetDateEmpty);
                    return true;
                }
                else if (m_selectedDateToCreatetrajet == null || m_selectedDateToCreatetrajet > DateTime.Now.Date.AddDays(Settings.NbSelectableDaysAfterToday))
                {
                    m_textinputlayoutCalendar.ErrorEnabled = true;
                    m_textinputlayoutCalendar.Error = string.Format(Resources.GetString(Resource.String.dashboardActivity_error_trajetDateIsAfterDateAllowed), Settings.NbSelectableDaysAfterToday);
                    return true;
                }
                else if (m_selectedDateToCreatetrajet == null || m_selectedDateToCreatetrajet < DateTime.Now.Date.AddDays(Settings.NbSelectableDaysBeforeToday))
                {
                    m_textinputlayoutCalendar.ErrorEnabled = true;
                    m_textinputlayoutCalendar.Error = string.Format(Resources.GetString(Resource.String.dashboardActivity_error_trajetDateIsBeforeDateAllowed), Settings.NbSelectableDaysBeforeToday.ToString().Replace("-", ""));
                    return true;
                }
                else
                {
                    m_textinputlayoutCalendar.ErrorEnabled = false;
                    m_textinputlayoutCalendar.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }




        private Trajet AddtrajetForTest()
        {
            Trajet test = new Trajet();
            test.m_sRefNumber = "B";
            test.m_datetrajet = DateTime.Now;
            test.m_sLocationStart = "Grenoble - Presqu'île";
            test.m_sLocationEnd = "Plaine des Sports";
            test.m_sTimeStart = "6:57";
            test.m_sTimeEnd = "7:34";
            test.m_sTrainType = "Tramway";
            test.m_sLocoType = "Electrique";
            test.m_sWeight = "56";
            test.m_lsarret = new List<Arret>();
            test.m_lsarret.Add(new Arret("Grenoble - Presqu'île", "0", "", "", 0, "6:57", "", "", "", 45.204441, 5.701218, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Cité Internationale", "0", "", "", 0, "7:00", "", "", "", 45.195042, 5.711121, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Palais de Justice", "0", "", "", 0, "7:03", "", "", "", 45.191158, 5.711664, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Saint Bruno", "0", "", "", 0, "7:05", "", "", "", 45.190149, 5.715212, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Gare", "0", "", "0", 5, "7:07", "", "", "V2", 45.190149, 5.715212, 7, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Alsace lorraine", "0", " ", "30", 0, "7:09", "", "", "V2", 45.189394, 5.720446, 2, false, "", "S", false));
            test.m_lsarret.Add(new Arret("Victor Hugo", "0", "", "70", 2, "7:11", "", "", "V2", 45.189799, 5.725152, 6, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Maison du tourisme", "0", "", "70", 8, "7:12", "", "", "V2", 45.190195, 5.728310, 2, false, "", "S", false));
            test.m_lsarret.Add(new Arret("Sainte Claire les halles", "0", "A2", "30", 0, "7:14", "", "", "BV2", 45.191323, 5.730705, 2, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Notre Dame Musée", "0", "", "0", 12, "7:15", "", "", "V2", 45.193565, 5.732215, 19, false, "", "S", false));
            test.m_lsarret.Add(new Arret("Île Verte", "0", "", "", 0, "7:16", "", "", "", 45.197286, 5.736712, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("La Tronche Hôpital", "0", "", "", 0, "7:17", "", "", "", 45.200633, 5.741139, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Michallon", "0", "", "", 0, "7:19", "", "", "", 45.200280, 5.745447, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Grand Sablon", "0", "", "", 0, "7:21", "", "", "", 45.198984, 5.749868, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Les Taillées-Universités", "0", "", "", 0, "7:23", "", "", "", 45.192195, 5.757836, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Gabriel Fauré", "0", "", "", 0, "7:25", "", "", "", 45.192288, 5.764456, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Bibliothèques Universitaires", "0", "", "", 0, "7:27", "", "", "", 45.192298, 5.770843, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Condillac-Universités", "0", "", "", 0, "7:29", "", "", "", 45.189128, 5.774377, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Mayencin Champ Roman", "0", "", "", 0, "7:31", "", "", "", 45.184743, 5.779055, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Gières Gare-Universités", "0", "", "", 0, "7:33", "", "", "", 45.185393, 5.785503, 0, true, "", "S", false));
            test.m_lsarret.Add(new Arret("Plaine des Sports", "0", "", "", 0, "7:34", "", "", "", 45.187521, 5.784426, 0, true, "", "S", false));
            
                //Recup pdf files from asset
                AssetManager assetManager = Assets;
                List<string> lsfiles = new List<string>();
                try
                {
                    lsfiles = new List<string>(assetManager.List("PDF_forDemo"));
                }
                catch (Java.IO.IOException)
                { }
                catch (System.IO.IOException)
                { }

                //save pdf files to local storage : dans le dossier Downloads
                string outDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath + "/pdf_Trajet_Tram";
                try
                {
                    if (!AskForPermissions.askForWriteExternalStoragePermission(this, 48))
                        return null;

                    if (!System.IO.Directory.Exists(outDir))
                        System.IO.Directory.CreateDirectory(outDir);
                    foreach (string sFileName in lsfiles)
                    {
                        Stream input = null;
                        FileStream output = null;
                        try
                        {
                            input = assetManager.Open("PDF_forDemo/" + sFileName);

                            if (System.IO.File.Exists(outDir + "/" + sFileName))
                                System.IO.File.Delete(outDir + "/" + sFileName);

                            output = System.IO.File.Create(outDir + "/" + sFileName);

                            input.CopyTo(output);
                            output.Close();
                        }
                        catch (Java.IO.IOException)
                        { }
                        catch (System.IO.IOException)
                        { }
                    }
                                        
                }
                catch (Exception e)
                {
                    MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_frameLayout, Resource.String.snackbar_errorHappened);
                }

                LocalDatabase.Get().Addtrajet(new FileAccessManager(), test);
                foreach (Arret p in test.m_lsarret)
                {
                    p.m_iLocalIDtrajet = test.m_iLocalID;
                    LocalDatabase.Get().AddArret(new FileAccessManager(), p);
                }

                //Refresh list if needed
                m_fragmentListtrajets?.RefreshtrajetList();

                return test;
        }
    }
}



using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Exceptions;
using Trajet_Tram.Helpers;
using Plugin.Connectivity;
using System;
using System.Reflection;

namespace Trajet_Tram.Droid.Activities
{
    [Activity(Label = "", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.SensorLandscape, LaunchMode = LaunchMode.SingleTop)]
    public class Activity_Login : AppCompatActivity
    {
        private RelativeLayout m_layoutRoot;
        private CardView m_cardviewConnection;
        private CardView m_cardviewNewPassword;
        private TextInputEditText m_edittextConnectionMatriculeNumber;
        private TextInputEditText m_edittextConnectionPassword;
        private TextInputEditText m_edittextNewPasswordMatriculeNumber;
        private TextInputEditText m_edittextNewPasswordEmail;
        private TextInputLayout m_textinputlayoutConnectionMatriculeNumber;
        private TextInputLayout m_textinputlayoutConnectionPassword;
        private TextInputLayout m_textinputlayoutNewPasswordMatriculeNumber;
        private TextInputLayout m_textinputlayoutNewPasswordEmail;
        private Button m_btnConnectionValid;
        private Button m_btnConnectionAskNewPassword;
        private Button m_btnNewPasswordSend;
        private Button m_btnNewPasswordCancel;
        private bool m_isAskingNewPassword = false;
        private SensorManager m_sensorManager;
        private SensorEventListener m_sensorListener;
        private bool m_bDontStartActivityAfterLoginValid;


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

                m_bDontStartActivityAfterLoginValid = Intent.Extras?.GetBoolean("DontStartActivityAfterLoginValid", false) ?? false;

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.activityLogin);

                m_layoutRoot = FindViewById<RelativeLayout>(Resource.Id.activityLogin_relativeLayout_root);
                m_cardviewConnection = FindViewById<CardView>(Resource.Id.activityLogin_cardview_connection);
                m_cardviewNewPassword = FindViewById<CardView>(Resource.Id.activityLogin_cardview_newPassword);
                m_edittextConnectionMatriculeNumber = FindViewById<TextInputEditText>(Resource.Id.activityLogin_textInputEditText_matriculeNumber);
                m_edittextConnectionPassword = FindViewById<TextInputEditText>(Resource.Id.activityLogin_textInputEditText_password);
                m_edittextNewPasswordMatriculeNumber = FindViewById<TextInputEditText>(Resource.Id.activityLogin_textInputEditText_newPassword_matriculeNumber);
                m_edittextNewPasswordEmail = FindViewById<TextInputEditText>(Resource.Id.activityLogin_textInputEditText_newPassword_email);
                m_textinputlayoutConnectionMatriculeNumber = FindViewById<TextInputLayout>(Resource.Id.activityLogin_textInputLayout_matriculeNumber);
                m_textinputlayoutConnectionPassword = FindViewById<TextInputLayout>(Resource.Id.activityLogin_textInputLayout_password);
                m_textinputlayoutNewPasswordMatriculeNumber = FindViewById<TextInputLayout>(Resource.Id.activityLogin_textInputLayout_newPassword_matriculeNumber);
                m_textinputlayoutNewPasswordEmail = FindViewById<TextInputLayout>(Resource.Id.activityLogin_textInputLayout_newPassword_email);
                m_btnConnectionValid = FindViewById<Button>(Resource.Id.activityLogin_btn_valid);
                m_btnConnectionAskNewPassword = FindViewById<Button>(Resource.Id.activityLogin_btn_askNewPassword);
                m_btnNewPasswordSend = FindViewById<Button>(Resource.Id.activityLogin_btn_newPassword_valid);
                m_btnNewPasswordCancel = FindViewById<Button>(Resource.Id.activityLogin_btn_newPassword_cancel);

                //Button behaviors
                m_btnConnectionAskNewPassword.Click += M_btnConnectionAskNewPassword_Click;
                m_btnNewPasswordCancel.Click += M_btnNewPasswordCancel_Click;
                m_btnConnectionValid.Click += M_btnConnectionValid_Click;
                m_btnNewPasswordSend.Click += M_btnNewPasswordSend_Click;

                //Init views
                UpdateUIToShowConnectionOrNewPassword();

                //If has ever been connected
                if (Settings.UserMatricule != "")
                    m_edittextConnectionMatriculeNumber.Text = Settings.UserMatricule;

                //EditText TextChanged for error management
                m_edittextConnectionMatriculeNumber.TextChanged += M_edittextConnectionMatriculeNumber_TextChanged;
                m_edittextConnectionPassword.TextChanged += M_edittextConnectionPassword_TextChanged;
                m_edittextNewPasswordMatriculeNumber.TextChanged += M_edittextNewPasswordMatriculeNumber_TextChanged;
                m_edittextNewPasswordEmail.TextChanged += M_edittextNewPasswordEmail_TextChanged;

                //Init Light Sensor Manager
                DynamicUIBuild_Utils.InitLightSensorManagers(this, out m_sensorManager, out m_sensorListener);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextNewPasswordEmail_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_AskNewPasswordEmail_Error();
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

        private void M_btnConnectionValid_Click(object sender, System.EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_btnConnectionValid_Click");

                if (!CrossConnectivity.Current.IsConnected)
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_internetRequired);
                    return;
                }

                if (IsEditText_ConnectionMatriculeNumber_Error() || IsEditText_ConnectionPassword_Error())
                    return;

                //Appel webservice
                string sToken = "";
                try
                {
                    sToken = "mon super token";

                    if (string.IsNullOrWhiteSpace(sToken))
                        throw new System.Exception("res is null after login api");
                }
                catch (InvalidLogin)
                {
                    //Invalid email or password
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.activityLogin_error_matriculeNumber_or_password_invalid);
                    m_textinputlayoutConnectionMatriculeNumber.ErrorEnabled = true;
                    m_textinputlayoutConnectionPassword.ErrorEnabled = true;
                    m_textinputlayoutConnectionMatriculeNumber.Error = Resources.GetString(Resource.String.activityLogin_error_matriculeNumber_or_password_invalid);
                    m_textinputlayoutConnectionPassword.Error = Resources.GetString(Resource.String.activityLogin_error_matriculeNumber_or_password_invalid);

                    return;
                }
                catch (OnlineException except)
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_errorHappened);

                    MobileCenter_Helper.ReportError(new FileAccessManager(), except, GetType().Name, MethodBase.GetCurrentMethod().Name +
                        "\nmatricule number = " + m_edittextConnectionMatriculeNumber.Text +
                        "\npassword = " + m_edittextConnectionPassword.Text);

                    return;
                }
                catch (Java.Lang.Exception ex)
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_errorHappened);

                    MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name +
                        "\nmatricule number = " + m_edittextConnectionMatriculeNumber.Text +
                        "\npassword = " + m_edittextConnectionPassword.Text);

                    return;
                }
                catch (System.Exception exc)
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_errorHappened);

                    MobileCenter_Helper.ReportError(new FileAccessManager(), exc, GetType().Name, MethodBase.GetCurrentMethod().Name +
                        "\nmatricule number = " + m_edittextConnectionMatriculeNumber.Text +
                        "\npassword = " + m_edittextConnectionPassword.Text);

                    return;
                }


                //if webservice is ok
                Settings.UserToken = sToken;
                Settings.UserMatricule = m_edittextConnectionMatriculeNumber.Text;
                Settings.UserPassword = m_edittextConnectionPassword.Text;

                //Go to dashboard
                if (!m_bDontStartActivityAfterLoginValid)
                    StartActivity(typeof(Activity_Dashboard));

                Finish();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_btnNewPasswordSend_Click(object sender, System.EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_btnNewPasswordSend_Click");

                if (!CrossConnectivity.Current.IsConnected)
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.snackbar_internetRequired);
                    return;
                }

                if (IsEditText_AskNewPasswordMatriculeNumber_Error() || IsEditText_AskNewPasswordEmail_Error())
                    return;

                //appel webservice
                bool bRes = true;

                //if webservice is ok
                if (bRes)
                {
                    //Show snackbar to inform user and redirect him to the connection cardview
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(this, m_layoutRoot, Resource.String.activityLogin_success_matriculeNumber_sent);

                    m_isAskingNewPassword = !m_isAskingNewPassword;
                    UpdateUIToShowConnectionOrNewPassword();
                }
                else
                {
                    //If matricule is wrong, show snackbar and error message
                    DynamicUIBuild_Utils.ShowSnackBar(this, m_layoutRoot, Resource.String.activityLogin_error_matriculeNumber_unknown, Snackbar.LengthLong);

                    m_textinputlayoutNewPasswordMatriculeNumber.ErrorEnabled = true;
                    m_textinputlayoutNewPasswordMatriculeNumber.Error = Resources.GetString(Resource.String.activityLogin_error_matriculeNumber_unknown);
                }
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextNewPasswordMatriculeNumber_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_AskNewPasswordMatriculeNumber_Error();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextConnectionPassword_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_ConnectionPassword_Error();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_edittextConnectionMatriculeNumber_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                IsEditText_ConnectionMatriculeNumber_Error();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void M_btnNewPasswordCancel_Click(object sender, System.EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_btnNewPasswordCancel_Click");

                m_isAskingNewPassword = !m_isAskingNewPassword;
                UpdateUIToShowConnectionOrNewPassword();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, "M_btnNewPasswordCancel_Click");
            }
        }

        private void M_btnConnectionAskNewPassword_Click(object sender, System.EventArgs e)
        {
            try
            {
                MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, "M_btnConnectionAskNewPassword_Click");
                
                m_isAskingNewPassword = !m_isAskingNewPassword;
                UpdateUIToShowConnectionOrNewPassword();
            }
            catch (Exception ex)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), ex, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void UpdateUIToShowConnectionOrNewPassword()
        {
            try
            {
                if (m_isAskingNewPassword)
                {
                    m_cardviewNewPassword.Visibility = ViewStates.Visible;
                    m_cardviewConnection.Visibility = ViewStates.Gone;

                    //If user entered a matricule number, report it into the other cardview
                    if (m_edittextConnectionMatriculeNumber.Text.Trim() != "")
                        m_edittextNewPasswordMatriculeNumber.Text = m_edittextConnectionMatriculeNumber.Text;

                    //Reset password edittext
                    m_edittextConnectionPassword.Text = "";
                    m_textinputlayoutConnectionPassword.ErrorEnabled = false;
                    m_textinputlayoutConnectionPassword.Error = null;

                }
                else
                {
                    m_cardviewConnection.Visibility = ViewStates.Visible;
                    m_cardviewNewPassword.Visibility = ViewStates.Gone;

                    //If user entered a matricule number, report it into the other cardview
                    if (m_edittextNewPasswordMatriculeNumber.Text.Trim() != "")
                        m_edittextConnectionMatriculeNumber.Text = m_edittextNewPasswordMatriculeNumber.Text;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        private bool IsEditText_ConnectionMatriculeNumber_Error()
        {
            try
            {
                if (m_edittextConnectionMatriculeNumber.Text.Trim() == "")
                {
                    m_textinputlayoutConnectionMatriculeNumber.ErrorEnabled = true;
                    m_textinputlayoutConnectionMatriculeNumber.Error = Resources.GetString(Resource.String.activityLogin_error_matriculeNumber_missing);
                    return true;
                }
                else
                {
                    m_textinputlayoutConnectionMatriculeNumber.ErrorEnabled = false;
                    m_textinputlayoutConnectionMatriculeNumber.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }

        private bool IsEditText_ConnectionPassword_Error()
        {
            try
            {
                if (m_edittextConnectionPassword.Text == "")
                {
                    m_textinputlayoutConnectionPassword.ErrorEnabled = true;
                    m_textinputlayoutConnectionPassword.Error = Resources.GetString(Resource.String.activityLogin_error_password_missing);
                    return true;
                }
                else
                {
                    m_textinputlayoutConnectionPassword.ErrorEnabled = false;
                    m_textinputlayoutConnectionPassword.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }

        private bool IsEditText_AskNewPasswordMatriculeNumber_Error()
        {
            try
            {
                if (m_edittextNewPasswordMatriculeNumber.Text.Trim() == "")
                {
                    m_textinputlayoutNewPasswordMatriculeNumber.ErrorEnabled = true;
                    m_textinputlayoutNewPasswordMatriculeNumber.Error = Resources.GetString(Resource.String.activityLogin_error_matriculeNumber_missing);
                    return true;
                }
                else
                {
                    m_textinputlayoutNewPasswordMatriculeNumber.ErrorEnabled = false;
                    m_textinputlayoutNewPasswordMatriculeNumber.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }

        private bool IsEditText_AskNewPasswordEmail_Error()
        {
            try
            {
                if (!Helpers.Utils.IsEmailValid(m_edittextNewPasswordEmail.Text))
                {
                    m_textinputlayoutNewPasswordEmail.ErrorEnabled = true;
                    m_textinputlayoutNewPasswordEmail.Error = Resources.GetString(Resource.String.activityLogin_error_email_invalid);
                    return true;
                }
                else
                {
                    m_textinputlayoutNewPasswordEmail.ErrorEnabled = false;
                    m_textinputlayoutNewPasswordEmail.Error = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }

            return false;
        }
    }
}
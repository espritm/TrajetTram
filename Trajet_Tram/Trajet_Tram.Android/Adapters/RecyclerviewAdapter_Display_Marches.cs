using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Droid.Activities;
using Trajet_Tram.Droid.Fragments;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Droid.ViewHolders;
using Trajet_Tram.Helpers;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.Droid.Adapters
{
    public class RecyclerviewAdapter_Display_trajets : RecyclerView.Adapter
    {
        Activity m_context;
        Android.Support.V4.App.Fragment m_Parent;
        List<Trajet> m_lstrajet = new List<Trajet>();

        public RecyclerviewAdapter_Display_trajets(Activity context, Android.Support.V4.App.Fragment parent)
        {
            m_context = context;
            m_Parent = parent;
        }

        public void UpdateListetrajets(List<Trajet> lstrajet)
        {
            m_lstrajet = lstrajet;
            NotifyDataSetChanged();
        }

        public override int ItemCount
        {
            get { return m_lstrajet == null ? 0 : m_lstrajet.Count; }
        }

        private void OnCardClicked(int position)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", position = " + position + ", trajetClicked = " + m_lstrajet?[position]?.m_sRefNumber ?? "null");

            Trajet trajetClicked = null;
            try
            {
                trajetClicked = m_lstrajet[position];
            }
            catch (Exception)
            { }

            //If trajet has been prepared, click on card start mode conduite. else, start mode preparation.
            if (trajetClicked == null || !trajetClicked.m_bHasBeenPrepared)
                GoToActivitytrajet(position, false);
            else
            {
                VerifyGPSAndLaunchConduiteMode(position);
            }
        }

        private void VerifyGPSAndLaunchConduiteMode(int position)
        {
            if (!AskForPermissions.askForGPSPermission(m_context))
                return;

            if (!CrossGeolocator.Current.IsGeolocationEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Le GPS de la tablette est désactivé !");
                sb.AppendLine("Cliquer ici pour ouvrir les paramètres GPS");
                AndroidHUD.AndHUD.Shared.ShowImage(m_context, Resource.Drawable.ic_gps_off_BIG, sb.ToString(), AndroidHUD.MaskType.Black, null, () =>
                {
                    m_context.StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                    AndroidHUD.AndHUD.Shared.Dismiss(m_context);
                }, () =>
                {
                    m_context.StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
                    AndroidHUD.AndHUD.Shared.Dismiss(m_context);
                });
            }
            else
            {
                GoToActivitytrajet(position, true);
            }
        }

        private void OnBtnOpenClicked(int position)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", position=" + position + ", trajetClicked = " + m_lstrajet?[position]?.m_sRefNumber ?? "null");
            VerifyGPSAndLaunchConduiteMode(position);
        }

        private void OnBtnModifiedClicked(int position)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", position=" + position + ", trajetClicked = " + m_lstrajet?[position]?.m_sRefNumber ?? "null");

            GoToActivitytrajet(position, false);
        }

        private void OnBtnDeletedClicked(int position)
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(m_context);
            alert.SetTitle(m_Parent.Resources.GetString(Resource.String.recyclerviewAdapter_display_trajets_deleteConfirmationTitle));
            alert.SetMessage(m_Parent.Resources.GetString(Resource.String.recyclerviewAdapter_display_trajets_deleteConfirmationContent));
            alert.SetPositiveButton(m_Parent.Resources.GetString(Resource.String.recyclerviewAdapter_display_trajets_deleteConfirmationOK), async (senderAlert, args) =>
            {
                if (TryDeletetrajet(position))
                {
                    MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name + ", position=" + position + ", trajetClicked = " + m_lstrajet?[position]?.m_sRefNumber ?? "null");
                    await ((Fragment_Listtrajets)m_Parent).RefreshtrajetList();
                }
                else
                {
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(m_context, m_Parent.View, Resource.String.recyclerviewAdapter_display_trajets_deleteError);
                }
            });

            alert.SetNegativeButton(m_Parent.Resources.GetString(Resource.String.recyclerviewAdapter_display_trajets_deleteConfirmationCancel), (senderAlert, args) =>
            {
                //On ne fait rien
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private bool TryDeletetrajet(int position)
        {
            Trajet trajetClicked = null;
            try
            {
                trajetClicked = m_lstrajet[position];
                if (trajetClicked != null)
                {
                    LocalDatabase.Get().RemovetrajetAndItsarret(new FileAccessManager(), trajetClicked);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private void GoToActivitytrajet(int position, bool bIsModeConduite)
        {
            Trajet trajetClicked = null;
            try
            {
                trajetClicked = m_lstrajet[position];
            }
            catch (Exception)
            { }

            if (trajetClicked == null)
                return;

            //If user has clicked on btn StartModeConduite in state disabled : show an error message to explain he has to prepare it before.
            if (bIsModeConduite && !trajetClicked.m_bHasBeenPrepared)
            {
                DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(m_context, m_Parent.View, Resource.String.item_display_trajets_btn_open_errorMessage);
                return;
            }

            //Go to activity_trajet
            Intent intent = new Intent(m_context, typeof(Activity_trajetDetails));
            /*intent.PutExtra("RefNumber", trajetClicked.m_sRefNumber);
            intent.PutExtra("DateOftrajet", trajetClicked.m_datetrajet.ToString("yyyy-MM-dd"));*/
            intent.PutExtra("itrajetLocalID", trajetClicked.m_iLocalID);
            intent.PutExtra("IsModeConduite", bIsModeConduite);

            //If start in preparation mode, wait a result in order to refresh the list if user has modified his trajet.
            if (bIsModeConduite)
                m_context.StartActivity(intent);
            else
                m_context.StartActivityForResult(intent, 42);
        }

        public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            try
            {
                ViewHolder_Display_trajets viewHolder = (ViewHolder_Display_trajets)holder;

                Trajet currenttrajet = m_lstrajet[position];

                await SetViewAsync(currenttrajet, viewHolder);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            ViewHolder_Display_trajets viewHolder = null;

            try
            {
                View itemView = m_context.LayoutInflater.Inflate(Resource.Layout.item_display_trajets, parent, false);

                viewHolder = new ViewHolder_Display_trajets(itemView, OnCardClicked, OnBtnModifiedClicked, OnBtnOpenClicked, OnBtnDeletedClicked);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
            return viewHolder;
        }

        private async Task SetViewAsync(Trajet currenttrajet, ViewHolder_Display_trajets viewHolder)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    //Clean cardview and show progressbar
                    if (m_context != null)
                    {
                        m_context.RunOnUiThread(() =>
                        {
                            viewHolder.m_progressbar.Visibility = ViewStates.Visible;
                            viewHolder.m_textviewRefNumber.Text = "";
                            viewHolder.m_textviewDate.Text = "";
                            viewHolder.m_textviewHourStart.Text = "";
                            viewHolder.m_textviewLocationStart.Text = "";
                            viewHolder.m_textviewHourEnd.Text = "";
                            viewHolder.m_textviewLocationEnd.Text = "";
                        });
                    }

                    //Get values to show.. May take a few time and it's done in background
                    //RefNumber
                    string textviewRefNumberText = currenttrajet.m_sRefNumber;

                    //Date 
                    string textviewDateText = currenttrajet.m_datetrajet.ToString(m_context.Resources.GetString(Resource.String.do_not_translate_dateTime_toString_DayMonthYear));

                    //Hour start
                    string textviewHourStartText = currenttrajet.m_sTimeStart;

                    //Location start
                    string textviewLocationStartText = currenttrajet.m_sLocationStart;

                    //Hour end
                    string textviewHourEndText = currenttrajet.m_sTimeEnd;

                    //Location end
                    string textviewLocationEndText = currenttrajet.m_sLocationEnd;

                    //Locations text color
                    Android.Graphics.Color textviewLocationTextColor;
                    if (Settings.NightMode)
                        textviewLocationTextColor = new Color(ContextCompat.GetColor(Application.Context, Android.Resource.Color.White));
                    else
                        textviewLocationTextColor = new Color(ContextCompat.GetColor(Application.Context, Android.Resource.Color.Black));

                    //Btn Start drive mode enable (or not)
                    bool btnOpenEnabled = currenttrajet.m_bHasBeenPrepared;
                    Color btnOpenTextColor = btnOpenEnabled ? m_context.Resources.GetColor(Resource.Color.red_primary) : m_context.Resources.GetColor(Resource.Color.red_primary_disabled);

                    if (m_context == null || !m_Parent.IsAdded)
                        return;

                    //Update cardview with values
                    m_context.RunOnUiThread(() =>
                    {
                        //RefNumber
                        viewHolder.m_textviewRefNumber.Text = textviewRefNumberText;

                        //Date 
                        viewHolder.m_textviewDate.Text = textviewDateText;

                        //Hour start
                        viewHolder.m_textviewHourStart.Text = textviewHourStartText;

                        //Location start
                        viewHolder.m_textviewLocationStart.Text = textviewLocationStartText;
                        viewHolder.m_textviewLocationStart.SetTextColor(textviewLocationTextColor);

                        //Hour end
                        viewHolder.m_textviewHourEnd.Text = textviewHourEndText;

                        //Location end
                        viewHolder.m_textviewLocationEnd.Text = textviewLocationEndText;
                        viewHolder.m_textviewLocationEnd.SetTextColor(textviewLocationTextColor);

                        //Btn Start drive mode enable (or not) (do not disabled the button in order to show error message to user)
                        viewHolder.m_btnOpen.SetTextColor(btnOpenTextColor);

                        //Remove progress bar
                        viewHolder.m_progressbar.Visibility = ViewStates.Gone;
                    });
                }
                catch (Exception e)
                {
                    MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
                }
            });
        }
    }
}
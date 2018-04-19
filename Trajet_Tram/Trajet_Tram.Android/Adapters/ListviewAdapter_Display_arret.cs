using System;
using Android.Views;
using Android.Widget;
using Trajet_Tram.BusinessLayer;
using System.Collections.Generic;
using Android.App;
using Android.Support.V4.Content;
using Trajet_Tram.Helpers;
using System.Reflection;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Droid.Activities;

namespace Trajet_Tram.Droid.Adapters
{
    public class ListviewAdapter_Display_arret : BaseAdapter<Arret>
    {
        public List<Arret> m_lsArret { get; private set; }
        Activity m_context;
        private List<View> m_lsView = new List<View>();
        
        public ListviewAdapter_Display_arret(Activity context)
        {
            m_context = context;
            m_lsArret = new List<Arret>();
        }

        public void RefreshListArret(List<Arret> lsArret)
        {
            m_lsArret = new List<Arret>();
            m_lsArret.AddRange(lsArret);
            NotifyDataSetChanged();
        }

        public override Arret this[int position]
        {
            get
            {
                Arret Arret = null;
                try
                {
                    Arret = m_lsArret[position];
                }
                catch (Exception e)
                {
                    MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name +
                        "\nm_lsArret = " + m_lsArret == null ? "null" : "not null" +
                        "\nm_lsArret.Count = " + m_lsArret == null ? "null" : m_lsArret.Count +
                        "\nposition = " + position +
                        "\nArret = " + Arret == null ? "null" : Arret.ToString());

                    Activity_trajetDetails parentActivity = null;
                    try
                    {
                        parentActivity = (Activity_trajetDetails)m_context;
                    }
                    catch (Exception)
                    {
                    }

                    if (parentActivity != null && parentActivity.m_layoutRoot != null)
                        DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(parentActivity, parentActivity.m_layoutRoot, Resource.String.snackbar_errorHappened);
                }

                return Arret;
            }
        }

        public override int Count
        {
            get { return m_lsArret.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = null;
            View hilightView = null;
            Arret currentArret = null;
            Arret nextArret = null;
            TextView textviewName = null;
            TextView textviewHorairePassage = null;

            try
            {
                currentArret = this[position];
                if (position + 1 < Count)
                    nextArret = this[position + 1];
                
                view = m_context.LayoutInflater.Inflate(Resource.Layout.item_display_arret, null);
                hilightView = view.FindViewById<View>(Resource.Id.item_display_arret_layout_hilight);
                textviewName = view.FindViewById<TextView>(Resource.Id.item_display_arret_textview_name);
                textviewHorairePassage = view.FindViewById<TextView>(Resource.Id.item_display_arret_textview_horairePassage);
                
                //Name Bold if it is a stop, normal if it is a simple Arret. And also show or not the hilightView
                textviewName.Text = currentArret.m_sName;
                textviewName.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
                hilightView.Visibility = ViewStates.Gone;
                

                m_lsView.Add(view);
            }
            catch(Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType()?.Name, MethodBase.GetCurrentMethod()?.Name +
                    "\nview = " + view == null ? "null" : "not null" +
                    "\ntextviewName = " + textviewName == null ? "null" : "not null" +
                    "\ntextviewHorairePassage = " + textviewHorairePassage == null ? "null" : "not null" +
                    "\ncurrentArret = " + currentArret == null ? "null" : currentArret.ToString() +
                    "\nnextArret = " + nextArret == null ? "null" : nextArret.ToString());

                Activity_trajetDetails parentActivity = null;
                try
                {
                    parentActivity = (Activity_trajetDetails)m_context;
                }
                catch(Exception)
                {
                }

                if (parentActivity != null && parentActivity.m_layoutRoot != null)
                    DynamicUIBuild_Utils.ShowSnackBar_WithOKButtonToClose(parentActivity, parentActivity.m_layoutRoot, Resource.String.snackbar_errorHappened);
            }

            return view;
        }

        public void RemoveLastItemsUntilCurrentStop(Arret currentStop)
        {
            //If currentStop is not in the list, return (avoid to delete all...)
            if (m_lsArret.Find(s => s.m_iLocalID == currentStop.m_iLocalID) == null)
                return;

            //While last item of the list is different from currentStop, remove it.
            while (m_lsArret[m_lsArret.Count - 1].m_iLocalID != currentStop.m_iLocalID)
                m_lsArret.RemoveAt(m_lsArret.Count - 1);

            //Remove last item : at this point it is currentStop
            m_lsArret.RemoveAt(m_lsArret.Count - 1);

            NotifyDataSetChanged();
        }
    }
}
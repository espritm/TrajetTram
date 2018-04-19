
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Droid.Adapters;
using Trajet_Tram.Droid.Utils;
using Trajet_Tram.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Trajet_Tram.Droid.Fragments
{
    public class Fragment_Listtrajets : Android.Support.V4.App.Fragment
    {
        private SwipeRefreshLayout m_refresher;
        private RecyclerView m_recyclerView;
        private TextView m_textviewEmptyList;
        private RecyclerviewAdapter_Display_trajets m_adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = null;
            
            try
            {
                view = inflater.Inflate(Resource.Layout.fragmentListTrajets, null);

                m_refresher = view.FindViewById<SwipeRefreshLayout>(Resource.Id.fragmentParameters_refresher);
                m_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.fragmentParameters_recyclerView);
                m_textviewEmptyList = view.FindViewById<TextView>(Resource.Id.fragmentParameters_textview_emptyList);

                //trajets list
                m_recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
                m_adapter = new RecyclerviewAdapter_Display_trajets(Activity, this);
                m_recyclerView.SetAdapter(m_adapter);

                m_refresher.Refresh += M_refresher_Refresh;
                
                m_refresher.Post(async () =>
                {
                    m_refresher.Refreshing = true;

                    await RefreshtrajetList();
                });
            }
            catch (System.Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (view != null)
                    DynamicUIBuild_Utils.ShowSnackBar(Activity, view, Resource.String.snackbar_errorHappened, Snackbar.LengthLong);
            }

            return view;
        }

        private async void M_refresher_Refresh(object sender, System.EventArgs e)
        {
            MobileCenter_Helper.TrackEvent(new FileAccessManager(), GetType().Name, MethodBase.GetCurrentMethod().Name);

            await RefreshtrajetList();
        }

        public async Task RefreshtrajetList()
        {
            try
            {
                m_refresher.Refreshing = true;

                List<Trajet> lstrajetsToShow = await Trajet.GetAlltrajetsToShowOnScreen(new FileAccessManager());

                m_adapter.UpdateListetrajets(lstrajetsToShow);

                if (lstrajetsToShow.Count == 0)
                    m_textviewEmptyList.Visibility = ViewStates.Visible;
                else
                    m_textviewEmptyList.Visibility = ViewStates.Gone;

                m_refresher.Refreshing = false;
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(new FileAccessManager(), e, GetType().Name, MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
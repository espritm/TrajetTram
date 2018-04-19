using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace Trajet_Tram.Droid.ViewHolders
{
    public class ViewHolder_Display_trajets : RecyclerView.ViewHolder
    {
        public CardView m_cardview { get; set; }
        public ProgressBar m_progressbar { get; set; }
        public TextView m_textviewRefNumber { get; set; }
        public TextView m_textviewDate { get; set; }
        public TextView m_textviewHourStart { get; set; }
        public TextView m_textviewLocationStart { get; set; }
        public TextView m_textviewHourEnd { get; set; }
        public TextView m_textviewLocationEnd { get; set; }
        public Button m_btnModify { get; set; }
        public Button m_btnOpen { get; set; }
        public ImageButton m_btnDelete { get; set; }

        public ViewHolder_Display_trajets(View itemView, Action<int> onCardClickListener, Action<int> onButtonModifyClickListener, Action<int> onButtonOpenClickListener, Action<int> onButtonDeleteClickListener)
            :base(itemView)
        {
            m_cardview = itemView.FindViewById<CardView>(Resource.Id.item_display_trajets_cardview);
            m_progressbar = itemView.FindViewById<ProgressBar>(Resource.Id.item_display_trajets_progressbar);
            m_textviewRefNumber = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_refNumber);
            m_textviewDate = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_date);
            m_textviewHourStart = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_timeStart);
            m_textviewLocationStart = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_locationStart);
            m_textviewHourEnd = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_timeEnd);
            m_textviewLocationEnd = itemView.FindViewById<TextView>(Resource.Id.item_display_trajets_textview_locationEnd);
            m_btnModify = itemView.FindViewById<Button>(Resource.Id.item_display_trajets_btn_modify);
            m_btnOpen = itemView.FindViewById<Button>(Resource.Id.item_display_trajets_btn_open);
            m_btnDelete = itemView.FindViewById<ImageButton>(Resource.Id.item_display_trajets_btn_delete);

            m_cardview.Click += (sender, e) => onCardClickListener(base.AdapterPosition);
            m_btnModify.Click += (sender, e) => onButtonModifyClickListener(base.AdapterPosition);
            m_btnOpen.Click += (sender, e) => onButtonOpenClickListener(base.AdapterPosition);
            m_btnDelete.Click += (sender, e) => onButtonDeleteClickListener(base.AdapterPosition);
        }
    }
}
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;

namespace ScoreKeeper_Android.Activities
{
    public class GameDetail
    {
        public string Player { get; set; }
        public int Score { get; set; }
        public string Date { get; set; }
        public string Details { get; set; }
    }

    public class GameDetailsAdapter : RecyclerView.Adapter
    {
        private List<GameDetailGroup> gameDetails;

        public GameDetailsAdapter(List<GameDetailGroup> gameDetails)
        {
            this.gameDetails = gameDetails;
        }

        public override int ItemCount => gameDetails?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is GameDetailViewHolder detailViewHolder)
            {
                var gameDetailGroup = gameDetails[position];
                detailViewHolder.GameDateTextView.Text = gameDetailGroup.Date;

                // Clear previous views
                detailViewHolder.PlayersContainer.RemoveAllViews();

                // Dynamically add player details
                foreach (var gameDetail in gameDetailGroup.GameDetails)
                {
                    var playerView = LayoutInflater.From(detailViewHolder.ItemView.Context).Inflate(Resource.Layout.item_game_details, null);
                    playerView.FindViewById<TextView>(Resource.Id.gamePlayerTextView).Text = gameDetail.Player;
                    playerView.FindViewById<TextView>(Resource.Id.gameScoreTextView).Text = gameDetail.Score.ToString();

                    detailViewHolder.PlayersContainer.AddView(playerView);
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.game_card, parent, false);
            return new GameDetailViewHolder(itemView);
        }

        public void UpdateGameDetails(List<GameDetailGroup> gameDetails)
        {
            this.gameDetails = gameDetails;
            NotifyDataSetChanged();
        }

        private class GameDetailViewHolder : RecyclerView.ViewHolder
        {
            public TextView GameDateTextView { get; }
            public LinearLayout PlayersContainer { get; }

            public GameDetailViewHolder(View itemView) : base(itemView)
            {
                GameDateTextView = itemView.FindViewById<TextView>(Resource.Id.gameDateTextView);
                PlayersContainer = itemView.FindViewById<LinearLayout>(Resource.Id.playersContainer);
            }
        }
    }

    public class GameDetailGroup
    {
        public string Date { get; set; }
        public List<GameDetail> GameDetails { get; set; }
    }
}
using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoreKeeper_Android.Activities
{
    public class Player
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Team { get; set; }
        public int Points { get; set; }
        public int ChangePoints { get; set; }
        public int PreviousPoints { get; set; }
        public bool StartPlayer { get; set; }

        public Player(string name, int points)
        {
            Name = name;
            Alias = name;
            Points = points;
            PreviousPoints = points;
            ChangePoints = 0;
            Team = "";
            StartPlayer = false;
        }

        public void ChangeAlias(string newAlias)
        {
            Alias = newAlias;
        }

        public void ChangeTeam(string newTeam)
        {
            Team = newTeam;
        }

        public void AddPoints(int newPoints)
        {
            Points += newPoints;
        }

        public void ChangeInPoints()
        {
            ChangePoints = Points - PreviousPoints;
            PreviousPoints = Points;
        }
    }
    public abstract class BaseGameActivity : AppCompatActivity
    {
        protected int NumberOfPlayers { get; private set; }
        protected List<Player> Players { get; private set; } = new List<Player>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            GetNumberOfPlayers(GetMinPlayers(), GetMaxPlayers());
        }

        private void GetNumberOfPlayers(int minPlayers, int maxPlayers)
        {
            Intent intent = new Intent(this, typeof(PlayersSelectionActivity));
            intent.PutExtra("MinPlayers", minPlayers);
            intent.PutExtra("MaxPlayers", maxPlayers);
            intent.PutExtra("CurrentNumberOfPlayers", NumberOfPlayers);

            StartActivityForResult(intent, 1);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1 && resultCode == Result.Ok)
            {
                NumberOfPlayers = data.GetIntExtra("NumberOfPlayers", 3);
                Toast.MakeText(this, $"Starting game for {NumberOfPlayers} players", ToastLength.Short).Show();

                var playerNames = data.GetStringArrayListExtra("PlayerNames").ToArray();

                // Initialize the Players list with Player instances
                Players = playerNames.Select(name => new Player(name, 0)).ToList();

                SetContentView(GetLayoutResourceId());

                SelectStartPlayer();

                PopulateGameView(NumberOfPlayers, Players);
            }
        }

        protected abstract int GetLayoutResourceId();
        protected abstract void PopulateGameView(int numberOfPlayers, List<Player> players);
        protected abstract int GetMinPlayers();
        protected abstract int GetMaxPlayers();
        protected TextView AddTextView(TableRow row, string text, float weight, GravityFlags gravity)
        {
            TextView textView = new TextView(this)
            {
                Text = text
            };
            TableRow.LayoutParams layoutParams = new TableRow.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, weight)
            {
                Gravity = gravity // Set the gravity
            };
            textView.LayoutParameters = layoutParams;
            row.AddView(textView);

            return textView; // Return the TextView instance
        }

        protected void UpdatePlayers(List<Player> updatedPlayers)
        {
            Players = updatedPlayers;
        }

        protected void SelectStartPlayer()
        {
            if (Players != null && Players.Count > 0)
            {
                Random random = new Random();
                int randomIndex = random.Next(Players.Count);

                // Set the StartPlayer property for the randomly selected player
                Players[randomIndex].StartPlayer = true;
            }
        }

        protected void ClearStartPlayer()
        {
            // Find and clear the current start player
            Player currentStartPlayer = Players.Find(player => player.StartPlayer);
            if (currentStartPlayer != null)
            {
                currentStartPlayer.StartPlayer = false;
            }
        }
    }
}
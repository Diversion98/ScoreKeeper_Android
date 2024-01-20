using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Collections.Generic;
using System.Linq;

namespace ScoreKeeper_Android.Activities
{
    public abstract class BaseGameActivity : AppCompatActivity
    {
        protected int NumberOfPlayers { get; private set; }
        protected string[] PlayerNames { get; private set; }

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

                PlayerNames = data.GetStringArrayListExtra("PlayerNames").ToArray();

                SetContentView(GetLayoutResourceId());

                PopulateGameView(NumberOfPlayers, PlayerNames);
            }
        }

        protected abstract int GetLayoutResourceId();
        protected abstract void PopulateGameView(int numberOfPlayers, string[] playerNames);
        protected abstract int GetMinPlayers();
        protected abstract int GetMaxPlayers();
        protected TextView AddTextView(TableRow row, string text, float weight, GravityFlags gravity)
        {
            TextView textView = new TextView(this)
            {
                Text = text
            };
            TableRow.LayoutParams layoutParams = new TableRow.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent,
                weight)
            {
                Gravity = gravity // Set the gravity
            };
            textView.LayoutParameters = layoutParams;
            row.AddView(textView);

            return textView; // Return the TextView instance
        }
    }
}
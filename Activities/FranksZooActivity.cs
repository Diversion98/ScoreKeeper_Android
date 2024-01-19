using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "FranksZooActivity")]
    public class FranksZooActivity : AppCompatActivity
    {
        private int numberOfPlayers = 3; // Default number of players

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.franksZooActivity);

            // Initiate player selection process immediately
            GetNumberOfPlayers();
        }

        private void GetNumberOfPlayers()
        {
            Intent intent = new Intent(this, typeof(PlayersSelectionActivity));

            // Pass min and max players as extras
            intent.PutExtra("MinPlayers", 3);
            intent.PutExtra("MaxPlayers", 7);

            StartActivityForResult(intent, 1);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1 && resultCode == Result.Ok)
            {
                numberOfPlayers = data.GetIntExtra("NumberOfPlayers", 3); // Default to 3 if not provided

                // Now you have the number of players selected by the user
                Toast.MakeText(this, $"Starting game for {numberOfPlayers} players", ToastLength.Short).Show();

                // TODO: Handle the logic for starting the game with the selected number of players
                // You may want to prompt the user for player names here or proceed as needed
            }
        }
    }
}
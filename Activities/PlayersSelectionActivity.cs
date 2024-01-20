using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System.Collections.Generic;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "PlayersSelectionActivity")]
    public class PlayersSelectionActivity : AppCompatActivity
    {
        private int minPlayers = 2; // Set the minimum number of players required for the game
        private int maxPlayers = 8; // Set the maximum number of players allowed for the game
        private int currentNumberOfPlayers = 2; // Default number of players
        private LinearLayout playerNamesContainer;
        private List<EditText> playerNameEditTexts;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.playersSelectionActivity);

            minPlayers = Intent.GetIntExtra("MinPlayers", 2);
            maxPlayers = Intent.GetIntExtra("MaxPlayers", 8);
            currentNumberOfPlayers = minPlayers;

            TextView playerCountTextView = FindViewById<TextView>(Resource.Id.playerCountTextView);
            TextView numberOfPlayersTextView = FindViewById<TextView>(Resource.Id.numberOfPlayersTextView);
            Button minusButton = FindViewById<Button>(Resource.Id.minusButton);
            Button plusButton = FindViewById<Button>(Resource.Id.plusButton);

            // Initialize player count display
            UpdatePlayerCountDisplay(numberOfPlayersTextView);

            // Event handlers for +/- buttons
            minusButton.Click += (sender, e) => DecreasePlayerCount(numberOfPlayersTextView);
            plusButton.Click += (sender, e) => IncreasePlayerCount(numberOfPlayersTextView);

            // Initialize player names container
            playerNamesContainer = FindViewById<LinearLayout>(Resource.Id.playerNamesContainer);
            playerNameEditTexts = new List<EditText>();

            // Initialize player name input fields based on the initial value of currentNumberOfPlayers
            UpdatePlayerNameFields();

            // Event handler for the "Start Game" button
            Button startGameButton = FindViewById<Button>(Resource.Id.startGameButton);
            startGameButton.Click += (sender, e) => StartGame();
        }

        private void UpdatePlayerCountDisplay(TextView numberOfPlayersTextView)
        {
            numberOfPlayersTextView.Text = currentNumberOfPlayers.ToString();
        }

        private void UpdatePlayerNameFields()
        {
            // Add or remove player name input fields based on the currentNumberOfPlayers
            while (playerNameEditTexts.Count < currentNumberOfPlayers)
            {
                // Add new player name field
                EditText playerNameEditText = new EditText(this)
                {
                    Hint = $"Player {playerNameEditTexts.Count + 1}",
                    LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
                };

                playerNameEditTexts.Add(playerNameEditText);
                playerNamesContainer.AddView(playerNameEditText);
            }

            while (playerNameEditTexts.Count > currentNumberOfPlayers)
            {
                // Remove the latest player name field
                EditText removedEditText = playerNameEditTexts[^1];
                playerNameEditTexts.Remove(removedEditText);
                playerNamesContainer.RemoveView(removedEditText);
            }
        }

        private void IncreasePlayerCount(TextView numberOfPlayersTextView)
        {
            if (currentNumberOfPlayers < maxPlayers)
            {
                currentNumberOfPlayers++;
                UpdatePlayerCountDisplay(numberOfPlayersTextView);
                UpdatePlayerNameFields();
            }
            else
            {
                Toast.MakeText(this, "Cannot exceed the maximum number of players", ToastLength.Short).Show();
            }
        }

        private void DecreasePlayerCount(TextView numberOfPlayersTextView)
        {
            if (currentNumberOfPlayers > minPlayers)
            {
                currentNumberOfPlayers--;
                UpdatePlayerCountDisplay(numberOfPlayersTextView);
                UpdatePlayerNameFields();
            }
            else
            {
                Toast.MakeText(this, "Cannot go below the minimum number of players", ToastLength.Short).Show();
            }
        }

        [System.Obsolete]
        public override void OnBackPressed()
        {
            // Override the back button behavior
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }

        private void StartGame()
        {
            // Check if all player names are filled in
            List<string> playerNames = new List<string>();
            foreach (var editText in playerNameEditTexts)
            {
                string playerName = editText.Text.Trim();
                if (string.IsNullOrWhiteSpace(playerName))
                {
                    Toast.MakeText(this, "One or more player names are invalid. Please fill in all player names.", ToastLength.Short).Show();
                    return;
                }
                playerNames.Add(playerName);
            }

            // Pass the selected number of players and their names back to the calling activity
            Intent intent = new Intent();
            intent.PutExtra("NumberOfPlayers", currentNumberOfPlayers);
            intent.PutStringArrayListExtra("PlayerNames", playerNames);
            SetResult(Result.Ok, intent);

            // Finish the activity
            Finish();
        }
    }
}

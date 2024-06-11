using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using AlertDialog = Android.App.AlertDialog;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "Player Selection")]
    public class PlayersSelectionActivity : AppCompatActivity
    {
        private int minPlayers = 2; // Set the minimum number of players required for the game
        private int maxPlayers = 8; // Set the maximum number of players allowed for the game
        private int currentNumberOfPlayers = 2; // Default number of players
        private LinearLayout playerNamesContainer;
        private List<Spinner> playerNameSpinners;
        private List<string> playerNames;
        private DatabaseHelper dbHelper;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme);
            SetContentView(Resource.Layout.playersSelectionActivity);

            dbHelper = new DatabaseHelper(this);

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
            playerNameSpinners = new List<Spinner>();

            // Fetch player names from the database
            playerNames = (List<string>)dbHelper.GetAllPlayers();

            // Initialize player name input fields based on the initial value of currentNumberOfPlayers
            UpdatePlayerNameFields();

            Button addNewPlayerButton = FindViewById<Button>(Resource.Id.addNewPlayerButton);
            addNewPlayerButton.Click += AddNewPlayer;

            // Event handler for the "Start Game" button
            Button startGameButton = FindViewById<Button>(Resource.Id.startGameButton);
            startGameButton.Click += (sender, e) => StartGame();
        }

        private void PlayerNameSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            RefreshPlayerNameSpinners();
        }

        private void AddNewPlayer(object sender, EventArgs e)
        {
            // Create an AlertDialog Builder
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            dialogBuilder.SetTitle("Add New Player");

            // Set up the input
            EditText input = new EditText(this);
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent,
                LinearLayout.LayoutParams.WrapContent
            );
            input.LayoutParameters = layoutParams;
            input.Hint = "Enter player name";
            dialogBuilder.SetView(input);

            // Set up the buttons
            dialogBuilder.SetPositiveButton("Add", (dialog, whichButton) =>
            {
                string playerName = input.Text.Trim();
                if (!string.IsNullOrEmpty(playerName))
                {
                    // Capitalize the first letter of the player's name
                    playerName = char.ToUpper(playerName[0]) + playerName.Substring(1);

                    dbHelper.AddPlayer(playerName);
                    playerNames.Add(playerName); // Add the new player to the list
                    RefreshPlayerNameSpinners(); // Refresh the player name spinners
                    Toast.MakeText(this, $"Player {playerName} added.", ToastLength.Short).Show();
                }
                else
                {
                    // Show a message if the input is empty
                    Toast.MakeText(this, "Please enter a player name", ToastLength.Short).Show();
                }
            });

            dialogBuilder.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                // Cancel button clicked, do nothing
            });

            // Show the dialog
            AlertDialog dialog = dialogBuilder.Create();
            dialog.Show();
        }

        private void UpdatePlayerCountDisplay(TextView numberOfPlayersTextView)
        {
            numberOfPlayersTextView.Text = currentNumberOfPlayers.ToString();
        }

        private void UpdatePlayerNameFields()
        {
            // Add or remove player name input fields based on the currentNumberOfPlayers
            while (playerNameSpinners.Count < currentNumberOfPlayers)
            {
                // Add new player name spinner
                Spinner playerNameSpinner = new Spinner(this)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
                };

                playerNameSpinner.ItemSelected += PlayerNameSpinner_ItemSelected;

                playerNameSpinners.Add(playerNameSpinner);
                playerNamesContainer.AddView(playerNameSpinner);
            }

            while (playerNameSpinners.Count > currentNumberOfPlayers)
            {
                // Remove the latest player name spinner
                Spinner removedSpinner = playerNameSpinners[^1];
                removedSpinner.ItemSelected -= PlayerNameSpinner_ItemSelected;
                playerNameSpinners.Remove(removedSpinner);
                playerNamesContainer.RemoveView(removedSpinner);
            }

            // Set unique names for each spinner
            SetUniqueNamesForSpinners();
        }

        private void SetUniqueNamesForSpinners()
        {
            List<string> availableNames = new List<string>(playerNames);

            for (int i = 0; i < playerNameSpinners.Count; i++)
            {
                Spinner spinner = playerNameSpinners[i];

                // Create a new list of available names for the spinner
                List<string> spinnerNames = availableNames.ToList();

                // Set up the adapter for the spinner
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, spinnerNames);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                // Disable the event handler to prevent triggering while setting the adapter
                spinner.ItemSelected -= PlayerNameSpinner_ItemSelected;

                // Set the new adapter
                spinner.Adapter = adapter;

                // Select a unique name if available
                if (i < availableNames.Count)
                {
                    spinner.SetSelection(i);
                    availableNames.RemoveAt(i);
                }

                // Re-enable the event handler
                spinner.ItemSelected += PlayerNameSpinner_ItemSelected;
            }
        }

        private void RefreshPlayerNameSpinners()
        {
            // Get the list of selected player names
            List<string> selectedPlayerNames = playerNameSpinners.Select(spinner => spinner.SelectedItem?.ToString()).ToList();

            foreach (var spinner in playerNameSpinners)
            {
                // Get the currently selected item in this spinner
                string selectedItem = spinner.SelectedItem?.ToString();

                // Create a new list of available names excluding the selected names from other spinners
                List<string> availableNames = playerNames.Except(selectedPlayerNames.Where(name => name != selectedItem)).ToList();

                // Set up the adapter for the spinner
                ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, availableNames);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                // Get the currently selected position (if any)
                int selectedPosition = availableNames.IndexOf(selectedItem);

                // Disable the event handler to prevent triggering while setting the adapter
                spinner.ItemSelected -= PlayerNameSpinner_ItemSelected;

                // Set the new adapter and restore the selection
                spinner.Adapter = adapter;
                if (selectedPosition >= 0)
                {
                    spinner.SetSelection(selectedPosition);
                }

                // Re-enable the event handler
                spinner.ItemSelected += PlayerNameSpinner_ItemSelected;
            }
        }

        private void IncreasePlayerCount(TextView numberOfPlayersTextView)
        {
            if (currentNumberOfPlayers < maxPlayers)
            {
                currentNumberOfPlayers++;
                UpdatePlayerCountDisplay(numberOfPlayersTextView);
                UpdatePlayerNameFields();
                SetUniqueNamesForSpinners();
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
            // Check if all player names are selected
            List<string> selectedPlayerNames = new List<string>();
            foreach (var spinner in playerNameSpinners)
            {
                string selectedPlayerName = spinner.SelectedItem?.ToString().Trim();
                if (string.IsNullOrWhiteSpace(selectedPlayerName))
                {
                    Toast.MakeText(this, "Please select a player for each slot", ToastLength.Short).Show();
                    return;
                }
                selectedPlayerNames.Add(selectedPlayerName);
            }

            // Pass the selected number of players and their names back to the calling activity
            Intent intent = new Intent();
            intent.PutExtra("NumberOfPlayers", currentNumberOfPlayers);
            intent.PutStringArrayListExtra("PlayerNames", selectedPlayerNames);
            SetResult(Result.Ok, intent);

            // Finish the activity
            Finish();
        }
    }
}

using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using System.Collections.Generic;
using System.Linq;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "FranksZooActivity")]
    public class FranksZooActivity : BaseGameActivity
    {
        private List<Spinner> hedgehogSpinners = new List<Spinner>();
        private List<Spinner> lionSpinners = new List<Spinner>();
        private List<Spinner> positionSpinners = new List<Spinner>();
        private List<TextView> pointsTextViews = new List<TextView>();
        private List<int> positionValues = new List<int>();
        private List<int> hedgehogValues = new List<int>();
        private List<int> lionValues = new List<int>();

        private int currentRound = 1; // Initialize the current round to 1

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Find the FAB button by its ID
            var fabNextRound = FindViewById<FloatingActionButton>(Resource.Id.fabNextRound);

            // Attach the click event handler
            fabNextRound.Click += NextRoundButton_Click;
        }

        protected override int GetLayoutResourceId() { return Resource.Layout.franksZooActivity; }

        protected override int GetMinPlayers() { return 3; }

        protected override int GetMaxPlayers() { return 7; }

        protected override void PopulateGameView(int numberOfPlayers, string[] playerNames)
        {
            // Access the TableLayout where you want to add rows dynamically
            TableLayout gameContainer = FindViewById<TableLayout>(Resource.Id.gameContainer);

            if (gameContainer != null)
            {
                // Loop through each player and add a row dynamically
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    // Create a new row layout (TableRow)
                    TableRow row = new TableRow(this)
                    {
                        LayoutParameters = new TableLayout.LayoutParams(
                        ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.WrapContent
                    )
                    };

                    // Add TextView for Player
                    AddTextView(row, playerNames[i], 1, GravityFlags.Center);

                    // Add Spinner for Player Position
                    Spinner positionSpinner = new Spinner(this);
                    AddPositionSpinner(row, positionSpinner, numberOfPlayers, i);

                    // Add Spinner for Hedgehogs and Lions
                    Spinner hedgehogSpinner = new Spinner(this);
                    Spinner lionSpinner = new Spinner(this);
                    AddSpinner(row, hedgehogSpinner, lionSpinner);

                    // Add TextView for Points
                    TextView pointsTextView = AddTextView(row, "0", 1f, GravityFlags.Center);

                    // Add the row to the game container
                    gameContainer.AddView(row);

                    // Add the dynamically created spinners to the lists
                    hedgehogSpinners.Add(hedgehogSpinner);
                    lionSpinners.Add(lionSpinner);

                    // Set the pointsTextView to the corresponding TextView in the row
                    pointsTextViews.Add(pointsTextView);
                }
            }
            else
            {
                Toast.MakeText(this, "Error: gameContainer is null", ToastLength.Short).Show();
            }
        }

        private void AddSpinner(TableRow row, Spinner hedgehogSpinner, Spinner lionSpinner)
        {
            if(currentRound == 1)
            {
                AddTextView(row, "/", 1f, GravityFlags.Center);
                AddTextView(row, "/", 1f, GravityFlags.Center);
                return;
            }

            // Set layout parameters for the Spinners
            hedgehogSpinner.LayoutParameters = new TableRow.LayoutParams(
                1,
                TableRow.LayoutParams.WrapContent,
                1)
            {
                Gravity = GravityFlags.Center // Center the content
            };

            lionSpinner.LayoutParameters = new TableRow.LayoutParams(
                1,
                TableRow.LayoutParams.WrapContent,
                1)
            {
                Gravity = GravityFlags.Center // Center the content
            };

            // Create an array of options based on the available counts
            List<string> options = new List<string> { "0", "1", "2", "3", "4", "5" };

            // Set adapter for the Spinners
            ArrayAdapter<string> hedgehogAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, options);
            hedgehogAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            hedgehogSpinner.Adapter = hedgehogAdapter;
            hedgehogSpinner.DropDownWidth = 100;

            ArrayAdapter<string> lionAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, options);
            lionAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            lionSpinner.Adapter = lionAdapter;
            lionSpinner.DropDownWidth = 100;

            // Add the Spinners to the layout
            row.AddView(hedgehogSpinner);
            row.AddView(lionSpinner);
        }

        private void AddPositionSpinner(TableRow row, Spinner spinner, int numberOfPlayers, int playerIndex)
        {
            // Set layout parameters for the Spinner
            TableRow.LayoutParams layoutParams = new TableRow.LayoutParams(1, TableRow.LayoutParams.WrapContent, 0.5f)
            {
                Gravity = GravityFlags.Center // Center the spinner horizontally and vertically
            };

            // Create an array of options based on the number of players
            List<string> options = Enumerable.Range(1, numberOfPlayers).Select(x => x.ToString()).ToList();

            // Set adapter for the Spinner
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, options);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            // Set default value based on player index
            spinner.SetSelection(playerIndex);

            // Set a fixed width for the spinner dropdown
            spinner.DropDownWidth = 100;

            // Add the Spinner to the layout 
            row.AddView(spinner, layoutParams);

            // Add the Spinner to the positionSpinners list
            positionSpinners.Add(spinner);
        }

        private bool CheckAnimalCounts()
        {
            if (currentRound == 1) return true;

            // Initialize counts
            int totalHedgehogs = 0;
            int totalLions = 0;

            hedgehogValues.Clear();
            lionValues.Clear();

            // Loop through each player and update the counts
            for (int i = 0; i < hedgehogSpinners.Count; i++)
            {
                Spinner hedgehogSpinner = hedgehogSpinners[i];
                Spinner lionSpinner = lionSpinners[i];

                // Get the selected values from spinners
                int selectedHedgehogs = int.Parse(hedgehogSpinner.SelectedItem.ToString());
                hedgehogValues.Add(selectedHedgehogs);
                int selectedLions = int.Parse(lionSpinner.SelectedItem.ToString());
                lionValues.Add(selectedLions);

                // Update the counts
                totalHedgehogs += selectedHedgehogs;
                totalLions += selectedLions;
            }

            // Check if counts exceed the limit
            if (totalHedgehogs + totalLions != 5)
            {
                Toast.MakeText(this, "Total hedgehogs and/or lions together should equal to 5", ToastLength.Short).Show();
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CheckPlayerPositions()
        {
            // Get all the position spinner values
            positionValues.Clear();
            for (int i = 0; i < positionSpinners.Count; i++)
            {
                Spinner positionSpinner = positionSpinners[i];
                int selectedPosition = int.Parse(positionSpinner.SelectedItem.ToString());
                positionValues.Add(selectedPosition);
            }

            // Check for duplicate values
            var duplicateValues = positionValues.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

            // Display a message if there are duplicate values
            if (duplicateValues.Any())
            {
                Toast.MakeText(this, "Player positions are incorrect. Each position should be unique.", ToastLength.Short).Show();
                return false;
            }

            return true;
        }

        private void NextRoundButton_Click(object sender, System.EventArgs e)
        {
            if (CheckPlayerPositions() && CheckAnimalCounts())
            {
                CalculatePoints();
                currentRound++;
            }
            else
            {
                return;
            }
        }

        private void CalculatePoints()
        {
            // Calculate points for each player based on the described logic
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                int positionValue = positionValues[i];
                int points = (positionValue == NumberOfPlayers) ? 0 : (NumberOfPlayers - (positionValue - 1));

                // Find the TableRow for the player
                TableRow row = GetTableRowForSpinner(positionSpinners[i]);

                // Update the TextViews in the row
                if (row != null)
                {
                    UpdateTextViewInRow(row, 1, positionValue.ToString()); // Position TextView

                    // Remove spinners and add TextViews in their place
                    row.RemoveViews(1, 4); // Remove position spinner
                    AddTextView(row, positionValue.ToString(), 1f, GravityFlags.Center); // Add position TextView

                    if (currentRound != 1)
                    {
                        int HedgehogValue = hedgehogValues[i];
                        int lionValue = lionValues[i];

                        UpdateTextViewInRow(row, 2, hedgehogSpinners[i].SelectedItem.ToString()); // Hedgehogs TextView
                        UpdateTextViewInRow(row, 3, lionSpinners[i].SelectedItem.ToString()); // Lions TextView

                        AddTextView(row, HedgehogValue.ToString(), 1f, GravityFlags.Center); // Add hedgehog TextView
                        AddTextView(row, lionValue.ToString(), 1f, GravityFlags.Center); // Add lion TextView
                    } else
                    {
                        AddTextView(row, "/", 1f, GravityFlags.Center); // Add hedgehog TextView
                        AddTextView(row, "/", 1f, GravityFlags.Center); // Add lion TextView
                    }

                    // Set the points in the corresponding TextView
                    AddTextView(row, points.ToString(), 1f, GravityFlags.Center);
                }
            }

            // Clear the spinner lists
            positionSpinners.Clear();
            hedgehogSpinners.Clear();
            lionSpinners.Clear();
        }

        private TableRow GetTableRowForSpinner(Spinner spinner)
        {
            TableLayout table = (TableLayout)spinner.Parent.Parent;
            for (int i = 0; i < table.ChildCount; i++)
            {
                View view = table.GetChildAt(i);
                if (view is TableRow row && row.GetChildAt(1) == spinner)
                {
                    return row;
                }
            }
            return null;
        }

        private void UpdateTextViewInRow(TableRow row, int columnIndex, string text)
        {
            for (int j = 0; j < row.ChildCount; j++)
            {
                View child = row.GetChildAt(j);
                if (child is TextView textView && columnIndex-- == 0)
                {
                    textView.Text = text;
                    break;
                }
            }
        }
    }
}
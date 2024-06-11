using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlertDialog = Android.App.AlertDialog;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "Frank's Zoo Score Sheet")]
    public class FranksZooActivity : BaseGameActivity
    {
        private List<Spinner> hedgehogSpinners = new List<Spinner>();
        private List<Spinner> lionSpinners = new List<Spinner>();
        private List<Spinner> positionSpinners = new List<Spinner>();
        private List<sbyte> positionValues = new List<sbyte>();
        private List<sbyte> hedgehogValues = new List<sbyte>();
        private List<sbyte> lionValues = new List<sbyte>();

        // Calculate the number of teams
        byte numberOfTeams;
        sbyte negativeLions = 0;

        private Button finishGameButton;
        private FloatingActionButton fabNextRound;

        private byte currentRound = 1; // Initialize the current round to 1

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Find the FAB button by its ID and attach the click event handler
            fabNextRound = FindViewById<FloatingActionButton>(Resource.Id.fabNextRound);
            fabNextRound.Click += NextRoundButton_Click;

            finishGameButton = FindViewById<Button>(Resource.Id.finishGameButton);
            finishGameButton.Click += (sender, e) => FinishGame("Frank's Zoo");

            // Calculate the number of teams
            numberOfTeams = (byte)(NumberOfPlayers / 2);
        }

        protected override int GetLayoutResourceId() { return Resource.Layout.franksZooActivity; }

        protected override int GetMinPlayers() { return 3; }

        protected override int GetMaxPlayers() { return 7; }

        protected override void PopulateGameView(int numberOfPlayers, List<Player> players)
        {
            // Access the TableLayout where you want to add rows dynamically
            TableLayout gameContainer = FindViewById<TableLayout>(Resource.Id.gameContainer);

            if (gameContainer != null)
            {
                for (int i = 0; i < numberOfPlayers; i++)
                {
                    // Create a new row layout (TableRow)
                    TableRow row = new TableRow(this)
                    {
                        LayoutParameters = new TableLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    };

                    // Add TextView for playerNames
                    AddTextView(row, players[i].Name + players[i].Team + (players[i].StartPlayer ? " *" : ""), 1, GravityFlags.Start);

                    // Add Spinner for Player Position
                    Spinner positionSpinner = new Spinner(this);
                    AddPositionSpinner(row, positionSpinner, numberOfPlayers, i);

                    // Add Spinner for Hedgehogs and Lions
                    Spinner hedgehogSpinner = new Spinner(this);
                    Spinner lionSpinner = new Spinner(this);
                    AddSpinner(row, hedgehogSpinner, lionSpinner);

                    // Add TextView for Points
                    TextView pointsTextView = AddTextView(row, players[i].Points.ToString(), 0.5f, GravityFlags.Center);

                    // Add the row to the game container
                    gameContainer.AddView(row);

                    // Add the dynamically created spinners to the lists
                    hedgehogSpinners.Add(hedgehogSpinner);
                    lionSpinners.Add(lionSpinner);
                }
            }
            else
            {
                Toast.MakeText(this, "Error: gameContainer is null", ToastLength.Short).Show();
            }
        }

        private void AddSpinner(TableRow row, Spinner hedgehogSpinner, Spinner lionSpinner)
        {
            if (currentRound == 1)
            {
                AddTextView(row, "/", 1f, GravityFlags.Center);
                AddTextView(row, "/", 1f, GravityFlags.Center);
                return;
            }

            // Set layout parameters for the Spinners
            hedgehogSpinner.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent, 1)
            {
                Gravity = GravityFlags.Center // Center the content
            };

            lionSpinner.LayoutParameters = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent, 1)
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
            TableRow.LayoutParams layoutParams = new TableRow.LayoutParams(TableRow.LayoutParams.MatchParent, TableRow.LayoutParams.WrapContent, 0.5f)
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

        private async Task<bool> CheckAnimalCounts()
        {
            if (currentRound == 1) return true;

            // Initialize counts
            sbyte totalHedgehogs = 0;
            sbyte totalLions = 0;

            hedgehogValues.Clear();
            lionValues.Clear();

            // Loop through each player and update the counts
            for (sbyte i = 0; i < hedgehogSpinners.Count; i++)
            {
                Spinner hedgehogSpinner = hedgehogSpinners[i];
                Spinner lionSpinner = lionSpinners[i];

                // Get the selected values from spinners
                sbyte selectedHedgehogs = sbyte.Parse(hedgehogSpinner.SelectedItem.ToString());
                hedgehogValues.Add(selectedHedgehogs);
                sbyte selectedLions = sbyte.Parse(lionSpinner.SelectedItem.ToString());
                lionValues.Add(selectedLions);

                // Update the counts
                totalHedgehogs += selectedHedgehogs;
                totalLions += selectedLions;
            }

            if (totalLions > 5 || totalHedgehogs > 5)
            {
                Toast.MakeText(this, "Lions and Hedgehogs can be max 5.", ToastLength.Short).Show();
                return false;
            }

            // Check if counts exceed the limit
            if (totalLions != 5)
            {
                // Show lion input dialog and wait for user input
                bool result = await ShowLionsInputDialog();
                if (!result)
                {
                    // User canceled the dialog
                    return false;
                }
            }

            return true;
        }

        private Task<bool> ShowLionsInputDialog()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("How many Lions does the last player have?");

            EditText input = new EditText(this);
            input.InputType = Android.Text.InputTypes.ClassNumber;
            builder.SetView(input);

            builder.SetPositiveButton("OK", (sender, e) =>
            {
                string inputText = input.Text;
                if (sbyte.TryParse(inputText, out sbyte lions))
                {
                    negativeLions = lions;
                    tcs.SetResult(true);
                }
            });

            builder.SetNegativeButton("Cancel", (sender, e) =>
            {
                // Handle the cancellation
                tcs.SetResult(false);
            });

            builder.Show();

            return tcs.Task;
        }

        private bool CheckPlayerPositions()
        {
            // Get all the position spinner values
            positionValues.Clear();
            for (sbyte i = 0; i < positionSpinners.Count; i++)
            {
                Spinner positionSpinner = positionSpinners[i];
                sbyte selectedPosition = sbyte.Parse(positionSpinner.SelectedItem.ToString());
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

        private async void NextRoundButton_Click(object sender, System.EventArgs e)
        {
            bool animalCountsResult = await CheckAnimalCounts();
            if (CheckPlayerPositions() && animalCountsResult)
            {
                CalculatePoints();

                // Check if at least two players have 19 or more points
                int playersWith19OrMorePoints = Players.Count(player => player.Points >= 19);

                if (playersWith19OrMorePoints < 2)
                {
                    // If not, create a new line across the view and make a new table
                    TableLayout gameContainer = FindViewById<TableLayout>(Resource.Id.gameContainer);

                    // Add a horizontal line to separate tables
                    View line = new View(this);
                    line.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(Resources.DisplayMetrics.Density * 1));
                    line.SetBackgroundResource(Android.Resource.Color.DarkerGray);
                    gameContainer.AddView(line);

                    // Update player names with team numbers
                    UpdatePlayerNamesWithTeams();

                    //SortPlayersByPoints();

                    currentRound++;

                    // Create a new table for the next round
                    PopulateGameView(NumberOfPlayers, Players);
                }
                else
                {
                    Toast.MakeText(this, "Game Over! At least two players have 19 or more points.", ToastLength.Short).Show();

                    fabNextRound.Visibility = ViewStates.Gone;
                    fabNextRound.Enabled = false;

                    finishGameButton.Visibility = ViewStates.Visible;
                    finishGameButton.Enabled = true;
                }
            }
            else
            {
                return;
            }
        }

        private void CalculatePoints()
        {
            List<TableRow> rows = new List<TableRow>();

            for (byte i = 0; i < NumberOfPlayers; i++)
            {
                sbyte positionValue = positionValues[i];
                sbyte calculatedPoints = (sbyte)((positionValue == NumberOfPlayers) ? 0 : (NumberOfPlayers - (positionValue - 1)));
                sbyte pointsFromAnimals = 0;

                // Find the team members for the current player
                var teamMembers = Players.Where(player => player.Team == Players[i].Team).ToList();

                TableRow row = GetTableRowForSpinner(positionSpinners[i]);

                rows.Add(row);

                // Remove spinners and add TextViews in their place
                rows[i].RemoveViews(1, 4);
                AddTextView(rows[i], positionValue.ToString(), 0.5f, GravityFlags.Center); // Add position TextView

                if (currentRound != 1)
                {
                    sbyte HedgehogValue = hedgehogValues[i];
                    sbyte LionValue = lionValues[i];

                    AddTextView(rows[i], HedgehogValue.ToString(), 1f, GravityFlags.Center); // Add hedgehog TextView

                    if (positionValue == NumberOfPlayers)
                    {
                        if (negativeLions != 0)
                        {
                            AddTextView(rows[i], LionValue.ToString() + " - " + negativeLions, 1f, GravityFlags.Center); // Add lion TextView
                        } else
                        {
                            AddTextView(rows[i], LionValue.ToString(), 1f, GravityFlags.Center); // Add lion TextView
                        }
                        pointsFromAnimals -= negativeLions;
                    } else
                    {
                        AddTextView(rows[i], LionValue.ToString(), 1f, GravityFlags.Center); // Add lion TextView
                    }

                    if (Players[i].Team == " (Alone)")
                        calculatedPoints += 4;

                    // Update points for all team members
                    foreach (var member in teamMembers)
                    {
                        member.AddPoints(calculatedPoints);
                    }

                    // Adjust points based on hedgehog and lion values
                    pointsFromAnimals -= (sbyte)((HedgehogValue == 0) ? 1 : 0);
                    pointsFromAnimals += (sbyte)((LionValue > 1) ? LionValue : 0);

                    Players[i].AddPoints(pointsFromAnimals);

                    if (i == NumberOfPlayers - 1)
                    {
                        for (int j = 0; j < NumberOfPlayers; j++)
                        {
                            AddTextView(rows[j], Players[j].Points.ToString(), 0.5f, GravityFlags.Center);
                        }
                    }
                }
                else
                {
                    AddTextView(rows[i], "/", 1f, GravityFlags.Center); // Add hedgehog TextView
                    AddTextView(rows[i], "/", 1f, GravityFlags.Center); // Add lion TextView

                    Players[i].AddPoints(calculatedPoints);

                    // Set the points in the corresponding TextView
                    AddTextView(rows[i], Players[i].Points.ToString(), 0.5f, GravityFlags.Center);
                }

                Players[i].ChangeInPoints();
            }

            // Clear the spinner lists
            positionSpinners.Clear();
            hedgehogSpinners.Clear();
            lionSpinners.Clear();
            negativeLions = 0;
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

        private void UpdatePlayerNamesWithTeams()
        {
            // Sort players by points in descending order
            var sortedPlayers = Players.OrderByDescending(player => player.Points)
                           .ThenByDescending(player => player.ChangePoints)
                           .ToList();

            // Split players into two groups excluding the alone player if it's uneven
            List<Player> group1 = sortedPlayers.Take(sortedPlayers.Count / 2).ToList();
            List<Player> group2;

            // Check if it's an uneven group of players
            if (sortedPlayers.Count % 2 != 0)
            {
                // Find the middle player and make it the alone player
                Player alonePlayer = sortedPlayers[sortedPlayers.Count / 2];
                alonePlayer.ChangeTeam(" (Alone)");

                group2 = sortedPlayers.Skip(sortedPlayers.Count / 2 + 1).ToList();
            }
            else
            {
                group2 = sortedPlayers.Skip(sortedPlayers.Count / 2).ToList();
            }

            // Assign players to teams using the two groups
            for (int i = 0; i < numberOfTeams; i++)
            {

                group1[i].ChangeTeam($" (Team {i + 1})");
                group2[i].ChangeTeam($" (Team {i + 1})");
            }

            ClearStartPlayer();
            sortedPlayers.Last().StartPlayer = true;

            UpdatePlayers(sortedPlayers);
        }
    }
}
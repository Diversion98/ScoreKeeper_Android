using Android.App;
using Android.Content;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Java.Lang.Reflect;
using ScoreKeeper_Android.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static Android.InputMethodServices.Keyboard;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "FranksZooActivity")]
    public class FranksZooActivity : BaseGameActivity
    {
        private List<Spinner> hedgehogSpinners = new List<Spinner>();
        private List<Spinner> lionSpinners = new List<Spinner>();
        private List<Spinner> positionSpinners = new List<Spinner>();
        private List<int> positionValues = new List<int>();
        private List<int> hedgehogValues = new List<int>();
        private List<int> lionValues = new List<int>();

        // Calculate the number of teams
        int numberOfTeams;

        private int currentRound = 1; // Initialize the current round to 1

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Find the FAB button by its ID and attach the click event handler
            var fabNextRound = FindViewById<FloatingActionButton>(Resource.Id.fabNextRound);
            fabNextRound.Click += NextRoundButton_Click;

            // Calculate the number of teams
            numberOfTeams = NumberOfPlayers / 2;
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

                    Console.WriteLine(players[i].StartPlayer);

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
            if (totalHedgehogs != 5 || totalLions != 5)
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
                    // Stop the game or take appropriate action
                    Toast.MakeText(this, "Game Over! At least two players have 19 or more points.", ToastLength.Short).Show();
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

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                int positionValue = positionValues[i];
                int calculatedPoints = (positionValue == NumberOfPlayers) ? 0 : (NumberOfPlayers - (positionValue - 1));
                int pointsFromAnimals = 0;

                // Find the team members for the current player
                var teamMembers = Players.Where(player => player.Team == Players[i].Team).ToList();

                TableRow row = GetTableRowForSpinner(positionSpinners[i]);

                rows.Add(row);

                // Remove spinners and add TextViews in their place
                rows[i].RemoveViews(1, 4);
                AddTextView(rows[i], positionValue.ToString(), 0.5f, GravityFlags.Center); // Add position TextView

                if (currentRound != 1)
                {
                    int HedgehogValue = hedgehogValues[i];
                    int LionValue = lionValues[i];

                    UpdateTextViewInRow(rows[i], 2, hedgehogSpinners[i].SelectedItem.ToString()); // Hedgehogs TextView
                    UpdateTextViewInRow(rows[i], 3, lionSpinners[i].SelectedItem.ToString()); // Lions TextView

                    AddTextView(rows[i], HedgehogValue.ToString(), 1f, GravityFlags.Center); // Add hedgehog TextView
                    AddTextView(rows[i], LionValue.ToString(), 1f, GravityFlags.Center); // Add lion TextView

                    if (Players[i].Team == " (Alone)")
                        calculatedPoints += 4;

                    // Update points for all team members
                    foreach (var member in teamMembers)
                    {
                        member.AddPoints(calculatedPoints);
                    }

                    // Adjust points based on hedgehog and lion values
                    pointsFromAnimals -= (HedgehogValue == 0) ? 1 : 0;
                    pointsFromAnimals += (LionValue > 1) ? LionValue : 0;

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
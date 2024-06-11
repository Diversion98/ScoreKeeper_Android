using Android.App;
using Android.Database.Sqlite;
using Android.Database;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;

namespace ScoreKeeper_Android.Activities
{
    [Activity(Label = "Game History")]
    public class GameHistoryActivity : AppCompatActivity
    {
        private Spinner gameSpinner;
        private RecyclerView gameDetailsRecyclerView;
        private DatabaseHelper dbHelper;
        private ArrayAdapter<string> gameAdapter;
        private GameDetailsAdapter gameDetailsAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(Resource.Style.AppTheme);
            SetContentView(Resource.Layout.activity_game_history);

            gameSpinner = FindViewById<Spinner>(Resource.Id.gameSpinner);
            gameDetailsRecyclerView = FindViewById<RecyclerView>(Resource.Id.gameDetailsRecyclerView);
            dbHelper = new DatabaseHelper(this);

            gameDetailsRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            LoadGames();
            gameSpinner.ItemSelected += GameSpinner_ItemSelected;
        }

        private void LoadGames()
        {
            var games = dbHelper.GetDistinctGameTitles();
            gameAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, games);
            gameAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            gameSpinner.Adapter = gameAdapter;
        }

        private void GameSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string selectedGame = gameAdapter.GetItem(e.Position);
            if (selectedGame != null)
            {
                LoadGameDetails(selectedGame);
            }
        }

        private void LoadGameDetails(string gameTitle)
        {
            IDictionary<string, List<GameDetail>> gameDetailsByDate = new Dictionary<string, List<GameDetail>>();

            SQLiteDatabase db = dbHelper.ReadableDatabase;
            ICursor cursor = db.RawQuery("SELECT Players.Name, Scores.Score, Games.Date FROM Scores " +
                                         "INNER JOIN Players ON Scores.PlayerID = Players.ID " +
                                         "INNER JOIN Games ON Scores.GameID = Games.ID " +
                                         "WHERE Games.Activity = ?", new string[] { gameTitle });

            while (cursor.MoveToNext())
            {
                string playerName = cursor.GetString(0);
                int score = cursor.GetInt(1);
                string date = cursor.GetString(2);

                if (!gameDetailsByDate.ContainsKey(date))
                {
                    gameDetailsByDate[date] = new List<GameDetail>();
                }

                gameDetailsByDate[date].Add(new GameDetail { Player = playerName, Score = score, Date = date });
            }
            cursor.Close();

            var groupedGameDetails = new List<GameDetailGroup>();
            foreach (var entry in gameDetailsByDate)
            {
                groupedGameDetails.Add(new GameDetailGroup { Date = DateTime.Parse(entry.Key).ToString("MM/dd/yyyy"), GameDetails = entry.Value });
            }

            gameDetailsAdapter = new GameDetailsAdapter(groupedGameDetails);
            gameDetailsRecyclerView.SetAdapter(gameDetailsAdapter);
        }
    }
}
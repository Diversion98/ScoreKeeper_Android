using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using System;
using System.Collections.Generic;

public class DatabaseHelper : SQLiteOpenHelper
{
    private new const string DatabaseName = "game_scores.db";
    private const int DatabaseVersion = 1;

    public DatabaseHelper(Context context) : base(context, DatabaseName, null, DatabaseVersion) { }

    public override void OnCreate(SQLiteDatabase db)
    {
        db.ExecSQL("CREATE TABLE Players (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT)");
        db.ExecSQL("CREATE TABLE Games (ID INTEGER PRIMARY KEY AUTOINCREMENT, Activity TEXT, Date TEXT)");
        db.ExecSQL("CREATE TABLE Scores (ID INTEGER PRIMARY KEY AUTOINCREMENT, PlayerID INTEGER, GameID INTEGER, Score INTEGER, FOREIGN KEY(PlayerID) REFERENCES Players(ID), FOREIGN KEY(GameID) REFERENCES Games(ID))");
    }

    public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
    {
        db.ExecSQL("DROP TABLE IF EXISTS Players");
        db.ExecSQL("DROP TABLE IF EXISTS Games");
        db.ExecSQL("DROP TABLE IF EXISTS Scores");
        OnCreate(db);
    }

    // Method to add a new player
    public long AddPlayer(string name)
    {
        SQLiteDatabase db = WritableDatabase;
        ContentValues values = new ContentValues();
        values.Put("Name", name);
        return db.Insert("Players", null, values);
    }

    // Method to add a new game
    public long AddGame(string activity)
    {
        SQLiteDatabase db = WritableDatabase;
        ContentValues values = new ContentValues();
        values.Put("Activity", activity);
        values.Put("Date", DateTime.Now.ToString());
        return db.Insert("Games", null, values);
    }

    // Method to add a new score
    public long AddScore(long playerId, long gameId, int score)
    {
        SQLiteDatabase db = WritableDatabase;
        ContentValues values = new ContentValues();
        values.Put("PlayerID", playerId);
        values.Put("GameID", gameId);
        values.Put("Score", score);
        return db.Insert("Scores", null, values);
    }

    public long GetPlayerIdByName(string playerName)
    {
        using (var db = WritableDatabase)
        {
            string[] columns = { "Id" };
            string selection = "Name = ?";
            string[] selectionArgs = { playerName };

            using (var cursor = db.Query("Players", columns, selection, selectionArgs, null, null, null))
            {
                if (cursor.MoveToFirst())
                {
                    return cursor.GetLong(cursor.GetColumnIndexOrThrow("ID"));
                }
            }
        }
        return -1; // Return -1 if the player is not found
    }

    // Method to get all players
    public IList<string> GetAllPlayers()
    {
        IList<string> players = new List<string>();
        SQLiteDatabase db = ReadableDatabase;
        ICursor cursor = db.Query("Players", new string[] { "Name" }, null, null, null, null, null);
        while (cursor.MoveToNext())
        {
            players.Add(cursor.GetString(0));
        }
        cursor.Close();
        return players;
    }

    // Method to get all games
    public IList<string> GetAllGames()
    {
        IList<string> games = new List<string>();
        SQLiteDatabase db = ReadableDatabase;
        ICursor cursor = db.Query("Games", new string[] { "Activity", "Date" }, null, null, null, null, null);
        while (cursor.MoveToNext())
        {
            games.Add(cursor.GetString(0) + " - " + cursor.GetString(1));
        }
        cursor.Close();
        return games;
    }

    // Method to get scores of a specific player
    public IList<string> GetScoresOfPlayer(long playerId)
    {
        IList<string> scores = new List<string>();
        SQLiteDatabase db = ReadableDatabase;
        ICursor cursor = db.Query("Scores", new string[] { "Score" }, "PlayerID=?", new string[] { playerId.ToString() }, null, null, null);
        while (cursor.MoveToNext())
        {
            scores.Add(cursor.GetString(0));
        }
        cursor.Close();
        return scores;
    }

    public IList<string> GetDistinctGameTitles()
    {
        IList<string> games = new List<string>();
        SQLiteDatabase db = ReadableDatabase;
        ICursor cursor = db.RawQuery("SELECT DISTINCT Activity FROM Games", null);
        while (cursor.MoveToNext())
        {
            games.Add(cursor.GetString(0));
        }
        cursor.Close();
        return games;
    }
}

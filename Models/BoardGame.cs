using System;

public class BoardGame
{
    public string Name { get; set; }
    public int MinPlayers { get; }
    public int MaxPlayers { get; }
    public int CurrentPlayers { get; set; }
    public int TotalScore { get; set; }

    public BoardGame(string name, int minPlayers, int maxPlayers)
    {
        Name = name;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        CurrentPlayers = 0;
        TotalScore = 0;
    }

    public void SetPlayerCount(int playerCount)
    {
        if (playerCount >= MinPlayers && playerCount <= MaxPlayers)
        {
            CurrentPlayers = playerCount;
        }
        // You might want to handle invalid player count here.
    }

    // Add more methods or properties as needed for your common functionality.
}

// Inherit from BoardGame for each specific game
public class SevenWonders : BoardGame
{
    // Add specific properties or methods for SevenWonders
    public SevenWonders() : base("SevenWonders", 3, 7)
    {
        // Additional initialization if needed
    }
}

// Inherit from BoardGame for each specific game
public class SevenWondersArchitect : BoardGame
{
    // Add specific properties or methods for SevenWonders
    public SevenWondersArchitect() : base("SevenWondersArchitect", 3, 7)
    {
        // Additional initialization if needed
    }
}

public class FrankiZoo : BoardGame
{
    // Add specific properties or methods for TicketToRide
    public FrankiZoo() : base("FrankiZoo", 3, 7)
    {
        // Additional initialization if needed
    }
}

// Add more game classes as needed

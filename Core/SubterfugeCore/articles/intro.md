# Creating a new Game

In order to create a new game, you can use a combination of the `Game` and `GameConfiguration` objects.
First, create a new `GameConfiguration` object to load specific information about the game's generation parameters.
Once the `GameConfiguration` object has been created, you can pass it into the `Game` constructor to create a new
game based off of the configuration parameters. It is important to note that a `GameConfiguration` object requires a player list.

```cs
// Create a list of players for the game
List<Player> players = new List<Player>();
players.Add(new Player(1));
players.Add(new Player(2));
players.Add(new Player(3));
players.Add(new Player(4));
// Add more if needed
            
// pass in players to GameConfiguration constructor
GameConfiguration config = new GameConfiguration(players);

// Configure the map generation as required with the GameConfiguration object.
config.seed = 1234;
config.dormantsPerPlayer = 3;
config.maxiumumOutpostDistance = 100;
config.minimumOutpostDistance = 5;
config.outpostsPerPlayer = 7;     

// Pass the game config to the game
Game game = new Game(config)
```

Once the game has been created, most interaction will be done through the `TimeMachine` class. This class manages the
game's simulation and allows you to advance and rewind through the game as well as get the `GameState` at the current
tick.

The `TimeMachine` is globally accessible and can be obtained anywhere after a `Game` has been created 
by using the following snippet:
```cs
TimeMachine timeMachine = Game.timeMachine
```

Once you have access to the `TimeMachine` object, you can get the current game's state with the `getState()` method.
The GameState lets you access anything related to the game. For example, all subs, all outposts, all players,
all specialists, and more. The following code snippet shows how you can use the `GameState` to get some basic info.

```cs
GameState currentState = Game.timeMachine.getState()

// Get all outposts
List<Outpost> outpostList = currentState.getOutposts();
foreach(Outpost o : outpostList) {
    Console.WriteLine(o.getDrillerCount());
}

// Get all subs
List<Sub> subList = currentState.getSubs();
foreach(Sub s : subList) {
    Console.WriteLine(s.getDrillerCount());
}

// Get a list of the players
List<Player> playerList = currentState.getPlayers();
foreach(Player p : playerList) {
    Console.WriteLine(p.getPlayerName());
}

```

There is much more you can do with the `GameState` object and this is further documented by looking at the API
documentation.
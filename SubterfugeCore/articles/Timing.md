# Timing

A majority of the game is all about timing. The `TimeMachine` and `GameTick` objects are extremely important to running
the game's simulation to specific points in time. Let's start with the basics:

### Moving through time

In order to move through time, the `TimeMachine` exposes a few functions that let you run the simulation to specific
points in time. These functions are:

- `advance(int ticks)` advances by `ticks` ticks 
- `rewind(int ticks)` rewinds by `ticks` ticks
- `goTo(GameEvent event)` goes to a specific `GameEvent`
- `goTo(GameTick tick)` goes to a specific `GameTick`

These functions let you easily go to specific points in time. After using these functions, you can get the simulated 
game at that point in time with `Game.timeMachine.getState()` as normal.

### The GameTick object

In order to get a specific moment in time, the `GameTick` object has a few methods to make this easy. The following
code block shows a number of methods you can use to create a `GameTick` from a specific time.

```
// This creates a GameTick from the current time.
// Note: Do not use DateTime.Now to get the current time.
// Instead use the NtpConnector object to ensure users are getting the DateTime from the server.
// If a Game object has been created, this method calculates the offset from the game's start time
// to determine the current tick
GameTick.fromDate(NtpConnector.GetNetworkTime()); 

// This creates a GameTick from a tick number.
GameTick.fromTickNumber(55);

// Get the current game's GameTick
Game.timeMachine.getCurrentTick();
```

Once you have obtained a `GameTick` object, you can call `.advance(int)` or `.rewind(int)` on the tick itself to move
relatively to that `GameTick` object. Once you offset yourself from the original, you can then use the `TimeMachine`
to go to your tick with `goTo(offsetGameTick)`.
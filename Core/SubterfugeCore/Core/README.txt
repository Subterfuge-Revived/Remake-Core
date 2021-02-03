### Game Server

The GameServer is what will be hosted on both the client, to analyze everything about a game.
This object will NOT use any textured objects. THis is because textures take a log of computing power and hold the server up.

The GameServer will need to be able to generate a list of all of the events that occur within the game and generate a GameState.
The GameState object will then be used by the app to render content in the correct location.
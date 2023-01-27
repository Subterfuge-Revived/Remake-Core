[![languages](https://img.shields.io/github/languages/top/Subterfuge-Revived/Remake-Core)]()
[![code-size](https://img.shields.io/github/languages/code-size/Subterfuge-Revived/Remake-Core)]()
[![commit-activity](https://img.shields.io/github/commit-activity/y/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/pulse/yearly)
[![license](https://img.shields.io/github/license/Subterfuge-Revived/Remake-Core)](LICENSE)
[![discord](https://img.shields.io/discord/617149385196961792)](https://discord.gg/GNk7Xw4)
[![issues](https://img.shields.io/github/issues/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aopen)
[![issues-closed-raw](https://img.shields.io/github/issues-closed/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aclosed+)

# Remake-Core

### [View the API Documentation](https://subterfuge-revived.github.io/Remake-Core/index.html)

This repository is a combination of the class library which runs the game, as well as the game server and client libraries.

For additional information on the game server, view the readme in the `Server` directory.

## Setup

### Required Dependencies

Ensure that you have the following programs installed:

- A C# IDE like [Jetbrains Rider](https://www.jetbrains.com/rider/) or [Visual Studios Community 2019](https://visualstudio.microsoft.com/)
- [.NET standard 2.0 framework](https://dotnet.microsoft.com/download/dotnet-core/2.0) and [.Net Core 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Git](https://git-scm.com/downloads)

For local server development:

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

Note: Docker Desktop (available on [Windows](https://docs.docker.com/desktop/install/windows-install/), [Mac](https://docs.docker.com/desktop/install/mac-install/), and [Linux](https://docs.docker.com/desktop/install/linux-install/))
includes Docker Compose by default. If this is your first installation of Docker, I would recommend just installing Docker Desktop.
Docker Desktop provides a nice GUI that shows you all of your images, running containers, volumes, etc. so that you don't have to remember or worry about any docker CLI commands.


### Configuring the project

1. [Fork this repository](https://github.com/Subterfuge-Revived/Remake-Core/fork)

2. Clone this repository:

```
git clone git@github.com:Subterfuge-Revived/Remake-Core.git
```

3. Add your fork as a remote:

```
cd Remake-Core
git remote add <yourName> <URL to your fork repository>
```

5. Using your IDE, open the `SubterfugeRemake.Shared.sln` file. This will open the project within your IDE.

6. To verify things worked, run the unit tests. Expand the `Engine` folder and right click on `SubterfugeCoreTest` and click "Run unit tests" (or similar).
If your IDE does not have the ability to do this, the following command can be executed to run the tests instead:

```
dotnet test Core/SubterfugeCoreTest
```

7. Watch the tests pass!

Note: The `SubterfugeRestApiServerTest` project requires both the database and the server to be running in order to pass.
Please see the [Server readme](Server/README.md) for more information about the server.

# Repositories

## Engine

#### [Models](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Core/Models)

The Models project is a class library that contains C# classes that are shared between the server and the client libraries.
This project mainly contains Server-client classes that are shared between one another and are generally used to produce network calls, or are class objects that can be sereialized from the server's response.
These are mainly networking classes and should be pure classes with only fields and no methods.

#### [SubterfugeCore](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Core/SubterfugeCore)

The bread and butter. This project contains all of the game logic. Generating the game state, loading a game from the server, creating a new game, launching a sub, specialists, combats, outposts. Anything in-game related is done here.
This library is used in Unity to be able to determine what the current state of a game is, and also used to load a gamestate from the network. This project is also used on the server side to validate network requests, verify submitted events,
and perform other validation checks, for example, if there is a winner of the game.

### [SubterfugeCoreTest](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Core/SubterfugeCoreTest)

Unit tests for Subterfuge Core. This project is purely for unit testing the main game logic. Unit tests ensure that upon future updates, existing code and functionality is not broken when new changes get added to the game.
These unit tests get executed on every merge request and are REQUIRED to pass before a merge request can be accepted. Ensure that any new code you write gets a unit test created for it.

## Server

#### [SubterfugeDatabaseProvider](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeDatabaseProvider)

A class library that creates database models for [MongoDB](https://www.mongodb.com/), and sets up all of the logic for MongoDB around the database models.
The data here is all encapsulated by interface methods which make it easy to swap out our database provider with another database if we chose to in the future.
Writing to and reading from the database is done here.

#### [SubterfugeRestApiClient](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeRestApiClient)

A client repository for sending API requests to the server. This client library is used to perform integration tests against the server, but also used by the unity client to make interacting with the server easier.

#### [SubterfugeRestApiServer](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeRestApiServer)

The subterfuge game server. This project integrates database storage with incoming client API requests to facilitate multiplayer play.
The server stores and processes information like: Creating game lobbies, joining game lobbies, submitting game events, adding friends, sending chat messages, and more.

#### [SubterfugeRestApiServerTest](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeRestApiServerTest)

Unit tesets for the Subterfuge Server. This project performs integration tests against the SubterfugeServer and requires a running database. This project tests various server endpoints and validates
that all of the required server functionality can be performed as expected.

# Using the Core Libraries

The unity application makes use of the `Engine` projects. Specifically `SubterfugeCore` which contains a class library with all of the primary game logic.

Once the `SubterfugeCore` project is build, a `dll` file is generated which needs to be included in the unity editor (the unity repository has this file committed to ensure devs don't need to have both repos running).

However, when working with the `dll`, it is a good idea to view the [API documentation on the class library here](https://subterfuge-revived.github.io/Remake-Core/index.html) to understand how to use classes that are avaliable.

For example, how to load a game, parse a gamestate from the server, launch a sub, alter the time machine, etc.
[![languages](https://img.shields.io/github/languages/top/Subterfuge-Revived/Remake-Core)]()
[![code-size](https://img.shields.io/github/languages/code-size/Subterfuge-Revived/Remake-Core)]()
[![commit-activity](https://img.shields.io/github/commit-activity/y/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/pulse/yearly)
[![license](https://img.shields.io/github/license/Subterfuge-Revived/Remake-Core)](LICENSE)
[![discord](https://img.shields.io/discord/617149385196961792)](https://discord.gg/GNk7Xw4)
[![issues](https://img.shields.io/github/issues/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aopen)
[![issues-closed-raw](https://img.shields.io/github/issues-closed/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aclosed+)
[![Banner](banner.png)]()

# Remake-Server

The server directory is a combination of server files that are used to facilitate the backend services of the game. These services include a number of projects to allow network communications
to take place between the players and the server. Within this project you will find 4 projects.

## Setup

See the [main repository readme](https://github.com/Subterfuge-Revived/Remake-Core) to setup your IDE.

## Local Server Deployment & Testing

#### Testing a Unity Client

To test a Unity client, you will need a running instance of the Subterfuge Server as well as the database.

To deploy the server and database, ensure you are in the root directory (One directory up from the file you are looking at right now):

Then, deploy the server and database with the following command:

```
docker-compose up -d
```

This command will start the subterfuge server, server tests, and the MongoDB database as docker containers.
This will allow you to start up a local development server with everything needed to test a Unity client against.

However, this will NOT let you run the debugger (which is usually extremely useful if you are trying to do server development or figure out why something in the server is not working properly.
Which leads us to:

#### Debugging the Server

If you need to run the debugger on server to figure out why things aren't working, this can easily be done through the IDE.

Note: If you have any docker containers running at this point, take them down with `docker-compose down` to get a fresh docker state.

The server requires the database to be running, and we can start just the database with the following command:

```
docker-compose up db -d
```

Once started, you can then start your server locally (with debugging) if needed.
Right click on the `SubterfugeRestApiServer` project in your IDE and click:

- "Run SubterfugeRestApiServer" to just run the server locally or,
- "Debug SubterfugeRestApiServer" to run the server in debug mode and allow you to set breakpoints in the server.

You can then run the Unity client and make requests to the server to debug, or run the integration tests, web server, discord bot, etc. to send requests to the local server for debugging.

# MongoDB Table Design

View the database models [here](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeDatabaseProvider/Models)
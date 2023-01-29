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

#### Running the Server

To run the server, there are a few different options depending on the type of setup you would like to run.
However, no matter the setup, we will be using docker compose.

All deployments will require a running instances of the database to connect to in order to ensure that data can be saved to the database.

There are currently 3 different deployment modes: `default`, `production`, `test`:

- `default` - Only deploys the database. This is useful if you are working on changes to the server
- `production` - Deploys the database and server. This is useful if you are working on a client (website, unity, discord bot, etc.) and don't want to change the server.
- `test` - Only really useful for CI/CD pipelines. This deployment will deploy the server, database, and will start up an extra container that runs the integration tests against the server. Not advised for local development because the integration tests will flush(delete) the entire database for testing.

#### `default` Deployment

The default deployment will only deploy the database. This is useful if you are trying to debug the server, or plan on running the server locally for development purposes.
This mode allows you to quickly stop the server to make quick modifications to debug or implement server functionality.

To deploy in the default mode (only the database), ensure you are in the root directory (One directory up from the file you are looking at right now):

Then, deploy the database with the following command:

```
docker-compose up -d
```

#### Debugging the Server

Now that we have started the database, it is likely we will want to run or debug the server locally.
You can then start your server locally (with debugging) if needed directly from the IDE.
Right click on the `SubterfugeRestApiServer` project in your IDE and click one of the two options:

- "Run SubterfugeRestApiServer" to just run the server locally or,
- "Debug SubterfugeRestApiServer" to run the server in debug mode. This will allow you to set breakpoints in the server for debugging potential bugs.

Once the server is running you can then run the Unity client, frontend web server, run integration tests, discord bot, etc. to send requests to the local server for debugging.

#### `production` Deployment

The production deployment will deploy the mongo database as well as a running instance of the server.
This is most useful if you are not planning to change the server and just want to develop a client, for example, the unity client, discord bot, website, etc.

To deploy in production mode, ensure that you are in the root directory (One directory up from the file you are looking at right now).
Then, deploy with the following command:

```
docker-compose --profile production up -d
```

Once this command is executed, you will have a running database and server.
Access the server at: `http://localhost:8080`, or view the swagger UI at `http://localhost:8080/swagger/index.html`

#### `test` Deployment

The `test` deployment is not advised to be used locally.
This deployment is mainly for testing the server builds within a CI/CD pipeline.
This deployment starts up the database, server, and a container that executes integration tests.

The integration tests will flush the mongodb database when they start up so it is not advised to start this mode locally unless you are okay with deleting your entire database.

To deploy in production mode, ensure that you are in the root directory (One directory up from the file you are looking at right now).
Then, deploy with the following command:

```
docker-compose --profile test up -d
```

Once this command is executed, you will have a running database and server as well as a container executing integration tests against them.
The CI/CD pipeline uses the following extra flags: `--abort-on-container-exit --exit-code-from server_test`.
These flags will get the exit code from the integration test container and return them to the CI/CD pipeline. If the tests pass, the CI/CD pipeline passes, otherwise the pipeline fails.

# MongoDB Table Design

View the database models [here](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeDatabaseProvider/Models)
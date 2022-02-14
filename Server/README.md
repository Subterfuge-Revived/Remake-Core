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

1. Install a C# IDE. We prefer [Jetbrains Rider](https://www.jetbrains.com/rider/) but you could try [Visual Studios Community 2019](https://visualstudio.microsoft.com/) as well.

2. Once you have installed an IDE, ensure that you have the [.NET standard 2.0 framework](https://dotnet.microsoft.com/download/dotnet-core/2.0).

3. I'm sure if you're here this step is done but, you need to [download Git](https://git-scm.com/downloads) to gain access to the code

4. Fork this repository and `git clone` your fork.

Note: After cloning the repository you will have the `master` branch checked out. This is likely not the most recent version of the code. You will want to checkout a branch. Ask in our discord what branch is the most up to date.

5. Using your IDE, open the project's `.sln` file. This will open the project within your IDE. Once open in the IDE, you are ready to go.


## Server Deployment

To deploy the server ensure you are in the `/Server` directory (This file is in the server directory):

`cd Server`

Deploy the server with:

`docker-compose up -d`

The above command, however, won't let you run the debugger. If you need to run the debugger on your project to test things, you should
start the database alone with:

`docker-compose up db -d`

Once started, you can then start your server locally (with debugging) if needed. First, modify `SubterfugeServer/Program.cs` line 16 and 12 to reference `localhost` instead of pointing to the docker container names.
You can then run the `SubterfugeClient` repository to send sample requests to the server for debugging, or run the `SubterfugeServerTest` repository to run the entire test suite against the server.

# Repositories

#### ProtoFiles

The profofiles project is a project that should ONLY include `.proto` files. [Protobuf]() is a message format that auto-generates server and client code so that native objects
within the respective language can be used instead of relying on JSON and web API documentation. This makes it easy for developers to jump in and understand the networking code.
In order to update the network interface, take a look at the [Protobuf language guide](https://developers.google.com/protocol-buffers/docs/proto3) which explains how to create messages,
services, service endpoints, and more.

<b>When you modify a `.proto` file, ensure that you `Build` this project! </b> When this project is built, it will create auto-generated files within the `ProtoGenerated` project.

#### ProtoGenerated

This project SHOULD NEVER BE MODIFIED. This project contains the generated `.cs` files that get created from the `.proto` files in the `ProtoFiles` project. This is a seperate repository
because this repository doesn't include some of the Protobuf development libraries that are used to generate this code, thus making this project smaller. Additionally, the `ProtoFiles` project
does not compile the generated `.cs` files. By putting the generated `.cs` fils in this project, the project will then compile the files and allow them to be used any dependent projects.

This allows us to use the messages and C# objects defined in the network interface both in the gRPC server, as well as in the Client. This project does not need to be built or modified.

#### SubterfugeClient

This is currently a testing repository to send gRPC requests as a client to the server. This project will eventually evolve to be a standalone class library which contains a client interface to send
network requests. This client should be included in the unity project to allow users of the app to send requests to the server.

To send example requests to the server, modify `SubterfugeClient.cs` to use the gRPC endpoints of your choice and run the project.

#### SubterfugeServer

This project is the glue of the project. This project integrates the database and storage mechanism with the incoming client requests to facilitate multiplayer play. This project uses Redis,
a key value store database as well as makes use of the gRPC service interface to handle incoming requests to the server.

Learn the basics of [Redis here](https://docs.redislabs.com/latest/rs/references/client_references/client_csharp/) and [advanced set/get operations](https://redis.io/commands) here. Specifically [HSET and HGET](https://redis.io/commands/hset).

Note: The database format is `key:value` ONLY and can only contain lists or dictionaries. However, it cannot contain complex types like a list of lists, or dictionary of dictionaries.
Because of this, it is ideal to setup lookup tables if you are going to need to query on a specific field. For example,
For a user, you will likely want to just obtain `user:19` to get data about user 19. However, when a user logs in they login with their username... Because of this, we need a lookup table for username to user id.
This will be the case for many other tables and the right design will need to be thought about.

To start a local server with debugging, modify `SubterfugeServer/Program.cs` line 16 and 12 to reference `localhost` instead of pointing to the docker container names. This will allow
you to debug the server locally. You can then run the `SubterfugeClient` repository to send sample requests to the server for debugging.

# MongoDB Table Design

View the database models [here](https://github.com/Subterfuge-Revived/Remake-Core/tree/master/Server/SubterfugeServer/Database/Models)

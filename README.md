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

1. Install a C# IDE like [Jetbrains Rider](https://www.jetbrains.com/rider/) or [Visual Studios Community 2019](https://visualstudio.microsoft.com/).

2. Once you have installed an IDE, ensure that you have the [.NET standard 2.0 framework](https://dotnet.microsoft.com/download/dotnet-core/2.0).

3. I'm sure if you're here this step is done but, you need to [download Git](https://git-scm.com/downloads) to gain access to the code

4. Fork this repository and get clone the repository.

Note: After cloning the repository you will have the `master` branch checked out. This is likely not the most recent version of the code. You will want to checkout a branch. Ask in our discord what branch is the most up to date.

5. Using your IDE, open the project's `.sln` file. This will open the project within your IDE. Once open in the IDE, you are ready to go.

## Using the Core Libraries

If using the Core `dll` class libraries, be sure to [View the API Documentation][documentation on the class library's API here](https://subterfuge-revived.github.io/Remake-Core/index.html) to understand how to make use of the API to load a game and parse a game state.

## Running Unit tests

Once you have the project loaded, you should have two folders, each containing a number of projects. Within each project, a `Test` project is present which is used
to validate that the code is running as expected. In order to run the test projects, you can simply right click the test project and click "Run tests in <project>".

If your IDE does not have the ability to do this, the following command can be executed to run the tests:

```
dotnet test Core/SubterfugeCoreTest
```

Additionally note: The `SubterfugeServerTest` project requires the database to be started. Please see the [Server readme](Server/README.md) for
more information on starting the database.
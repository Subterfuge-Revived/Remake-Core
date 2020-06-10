[![languages](https://img.shields.io/github/languages/top/Subterfuge-Revived/Remake-Core)]()
[![code-size](https://img.shields.io/github/languages/code-size/Subterfuge-Revived/Remake-Core)]()
[![commit-activity](https://img.shields.io/github/commit-activity/y/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/pulse/yearly)
[![license](https://img.shields.io/github/license/Subterfuge-Revived/Remake-Core)](LICENSE)
[![discord](https://img.shields.io/discord/617149385196961792)](https://discord.gg/GNk7Xw4)
[![issues](https://img.shields.io/github/issues/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aopen)
[![issues-closed-raw](https://img.shields.io/github/issues-closed/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aclosed+)
[![Banner](banner.png)]()

# Remake-Core

### [View the API Documentation](https://subterfuge-revived.github.io/Remake-Core/index.html)

This repository is a class library that contains all of the core game-logic and functionality for the game. This class library gets built into a `dll` which is then consumed by the unity repository so that unity can parse and display the game state. This project also includes a DLL CLI tool which the backend repository makes use of in order to validate all player events when they are recieved. View tutorials and [documentation on the class library's API here](https://subterfuge-revived.github.io/Remake-Core/index.html).

## Setup

1. Install a C# IDE like [Jetbrains Rider](https://www.jetbrains.com/rider/) or [Visual Studios Community 2019](https://visualstudio.microsoft.com/).

2. Once you have installed an IDE, ensure that you have the [.NET standard 2.0 framework](https://dotnet.microsoft.com/download/dotnet-core/2.0).

3. I'm sure if you're here this step is done but, you need to [download Git](https://git-scm.com/downloads) to gain access to the code

4. Fork this repository and get clone the repository.

Note: After cloning the repository you will have the `master` branch checked out. This is likely not the most recent version of the code. You will want to checkout a branch. Ask in our discord what branch is the most up to date.

5. Using your IDE, open the project's `.sln` file. This will open the project within your IDE. Once open in the IDE, you are ready to go.

## Using the Core Libraries

If using the Core `dll` class libraries within the frontend or backend repositories, be sure to [View the API Documentation][documentation on the class library's API here](https://subterfuge-revived.github.io/Remake-Core/index.html) to understand how to make use of the API to load a game and parse a game state.


Needed for Backend:
### How to access dotnet on Linux for backend use of the CLI:

- Get the latest link for dot net core 3.1 from the microsoft website
https://dotnet.microsoft.com/download/dotnet-core/3.1
- Select version 3.1 and copy the "Direct Link" URL.

- Navigate to a folder of your choice where you would like to download dotnet to.
- Make note of the current folder path at this point. Replace future occurances of `{path}` with your current path.
- Note: Replace `{directLink}` in the first linux command below with the Direct Link to dotnet.


1. Download dotnet: `wget {directLink}`
2. Create a directory to extract the tar: `mkdir dotnet-arm32`
3. Unzip to the directory: `tar zxf dotnet-sdk-3.1.102-linux-arm.tar.gz -C dotnet-arm32/`
4. Allow using the `dotnet` command globally by setting the PATH:

`export DOTNET_ROOT={path}/dotnet-arm32/`<br/>
`export PATH=$PATH:{path}/dotnet-arm32/`

5. Verify the installation worked:
`dotnet --info`

[![languages](https://img.shields.io/github/languages/top/Subterfuge-Revived/Remake-Core)]()
[![code-size](https://img.shields.io/github/languages/code-size/Subterfuge-Revived/Remake-Core)]()
[![commit-activity](https://img.shields.io/github/commit-activity/y/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/pulse/yearly)
[![license](https://img.shields.io/github/license/Subterfuge-Revived/Remake-Core)](LICENSE)
[![discord](https://img.shields.io/discord/617149385196961792)](https://discord.gg/GNk7Xw4)
[![issues](https://img.shields.io/github/issues/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aopen)
[![issues-closed-raw](https://img.shields.io/github/issues-closed/Subterfuge-Revived/Remake-Core)](https://github.com/Subterfuge-Revived/Remake-Core/issues?q=is%3Aclosed+)
[![Banner](banner.png)]()

# Remake-Core
##### v0.1.2
This repository is a class library that contains all of the core game-logic and functionality for the game. This class library gets built into a `dll` which is then consumed by the unity repository so that unity can parse and display the game state. This project also includes a DLL CLI tool which the backend repository makes use of in order to validate all player events when they are recieved.

## Setup

1. [Install Unity Hub](https://unity3d.com/get-unity/download) if you don't have it already

2. Once unity hub is installed, activate a new license (pro or personal is fine).

3. After getting a license, Install unity version `2019.3.0f6`. I've tried to upgrade to even the smallest versionn above this but for some reason updating it breaks the whole project. You need to install this version of unity.

4. I'm sure if you're here this step is done but, you need to [download Git](https://git-scm.com/downloads) to gain access to the code

5. Create a [GitHub](https://github.com/) account.

6. Message Myself or another Team member with your GitHub username to be granted access to the team.

7. Clone the unity repository to a folder of your choice with `git clone https://github.com/Subterfuge-Revived/Remake-Core.git`

Note: After cloning the repository you will have the `master` branch checked out. This is likely not the most recent version of the code. You will want to checkout a branch. Ask in our discord what branch is the most up to date.

8. In unity hub click "Import Project" and path yourself to the project folder. Once imported, start the project with unity.

9. Unity defaults to using Visual Studios as the primary code editor. You can configure Rider (or another IDE) as your default text editor by going to `Edit > Preferences > External Tools` and setting `External Script Editor` to your editor of choice.

10. Congratulations! you're all setup! Feel free to assign yourself an issue and contribute to development.

## Using the Core Libraries

TODO Once github pages is up.


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

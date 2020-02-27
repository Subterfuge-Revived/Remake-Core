## Subterfuge Core

## Using the API

### API Documentation

To start making use of the SubterfugeCore.dll, take a look at the [API documentation found here](https://subterfugeremake.gitlab.io/subterfuge-frontend/).
There are a few guides as well as auto-generated documentation that show the avaliable methods, paramteres, classes and more that you can make use of.

## Contributing

### Setup

1. Ensure you have installed [Rider](https://www.jetbrains.com/rider/), an IDE for C# development offered by JetBrains.
Students can register their school email to get free access to the IDE. You can alternatively use Visual Studios.

2. If you are using Rider but you do not have Visual Studios installed on your machine, you will need to install some additional SDKs for development.
Take a look at what you may need to install here:
https://rider-support.jetbrains.com/hc/en-us/articles/207288089-Using-Rider-under-Windows-without-Visual-Studio-prerequisites

3. You may need to download .NET Core. Not certain on this.
.NET Core - 2.1.801 - https://dotnet.microsoft.com/download/dotnet-core/2.1

4. Clone the frontend repository to a folder of your choice with git clone https://gitlab.com/subterfugeRemake/subterfuge-frontend.git

5. Use your IDE to open `SubterfugeRemake.Shared.sln` file to open the C# project. Your IDE should automatically find and install the required NuGet packages automatically.

6. You are ready to contribute!


### Generating a Build

Once you have made changes to any of the code, you will likely want to generate a new `dll` file so that Unity and backend devs can use the latest changes in their development.
Take the following steps to create a new build:

1. Within the project, find the "SubterfugeCoreTest" project. Right click this project and click "Run Unit Tests"

2. A window will appear showing all of the tests that were executed as well as how many (if any) test failed. If any automated tests fail, you have likely
made some changes that broke other features. Determine which tests are failing, why, and try to resolve the problem.

3. If you have created a new component and don't want anyone to break your component's functionality, write an automated test for your component. You
can do this by referencing the other test files and test each function and method to ensure your object is operating as expected.

4. Once you have written your automated tests and confirmed that all tests are passing, you can generate a new build.

5. To generate a new build, Right click on the `SubterfugeCore` project and click `Build` (or something similar). This will compile all of the files into their binaries
and will output a `dll` file for you.

6. Access the `dll` in the following directory: `\SubterfugeCore\obj\Debug\netstandard2.0\SubterfugeCore.dll`
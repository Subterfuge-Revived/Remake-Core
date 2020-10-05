# Updating Unity

When change have been made to the Core repository, it is very likely that you will want to test
if they are working by playing the game. In order to do this, the class library that is generated
from a build in the Core library needs to be copied into the Unity repository.

### Building the Core library

Once you have some changes that you would like tested, you can perform a 'build' of the Core library
In most IDEs there will be a button in the top toolbar that says 'build' to allow you to do so.
Once the library is built, the output files can be found in the following directory:

`<projectRoot>/SubterfugeCore/obj/Debug/netstandard2.0/`

At this directory, a number of files will be present but the main file of concern is `SubterfugeCore.dll`.
Copy this file. This is the class library and unity uses the `.dll` to load in the classes into the editor.

### Updating Unity

In unity, there is a folder called `lib`. Unfortunately, you cannot paste files directly in unity
so right click this folder and open it in file explorer (`Show in Explorer`on windows). Once the file
explorer is open, delete the existing `SubterfugeCore` files and paste in the `.dll` that has been created
from your build. Once the new `.dll` has been placed in this directory, Unity will use the updated class
library when running the game.

NOTE: Please commit the `.dll` file with your unity commits. Git should automatically detect changes to this
file but don't specifically exclude this file. If you make some updates to Unity while using an updated `.dll`
any other developer will need to have the same `.dll` file in order for the game to work.

Also Note: When submitting PRs for the Core and unity repositories, if the Unity update includes a
PR that has been submitted to the core library but is still pending, please link to the open PR
as the PR to Core should be done first. 
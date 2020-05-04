# Remake-Core
### Version 0.1.1

Core repository that holds all game logic for both front end rendering and back end validation.

Will need in the future:

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

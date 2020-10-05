# Networking and Multiplayer

Being a multiplayer game, the game needs to be able to talk to a server to verify other player
actions as well as coordinate the game between multiple people. The networking API makes use of 
HTTP REST endpoints to recieve a JSON response. The JSON response is then parsed and loaded into C# objects
that represent the request. All of the networking is performed in the `Network` folder. Within
the core library, none of the networking calls are ever called. The networking layer is only there
to allow people using the `.dll` an easier way of making requests.

When using the networking interface, the `Api` class allows you to create a network interface.
When using the `Api`, the object can be created empty with `new Api()` and this connection will
use a connection to `localhost`. However, if you would like to connect to a server, you can also
use `new Api("19.131.14.11")` to specify the IP address or domain.

If you don't want to use an IP, a fake backend is implemented that will always return successful results.
However, these results will not correspond to the incoming requests. To enable this mode, you can
call `api.developmentMode(true)`.

Once the api object has been created, you can send a request to the network by using one of the many
functions built into the api class. For example, `api.GetOpenRooms()`. All of the Api calls are asynchronous
and thus, when making a call, be sure to use `await`. For example:

`NetworkResponse<GameRoomResponse> roomResponse = await api.GetOpenRooms()`

Note: This module will also remember a user's session token after they login. This means that
subsequent calls to the api to not need to keep hold of the user's token manually.
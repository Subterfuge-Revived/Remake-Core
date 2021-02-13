## Understanding the backend

The backend server uses [gRPC](https://grpc.io/) (Google remote procedure calls) to define its network interface.
In the `Server` folder, you can find all of the projects that make the server work.

### ProtoFiles

This project defines the `.proto` files which define the network calls that can be made. To understand these files,
take a look at the [protobuf language guide](https://developers.google.com/protocol-buffers/docs/proto3).

A basic implementation looks like this:
```
service MyService {
  rpc Login(LoginRequest) returns(LoginResponse) {};
}

message LoginRequest {
  string username = 1;
  string password = 2;
}

message LoginResponse {
  string isSuccess = 1;
}
```

This example defines a method called `Login()` which takes a `LoginRequest` as input and responds with a `LoginResponse`. Protobuf will automatically compile these
network definitions into `.cs` files that can be used within the backend server as well as the main game library. If you make changes to any of the `.proto` files,
building this repository will re-generate the `.cs` files.

### ProtoGenerated

This project contains the generated files from the `ProtoFiles` project. This project should never be modified directly and is automatically populated when the
`ProtoFiles` project is build.

### SubterfugeServer

This project is the game server. Running this project will start a local server on your own machine. This is helpful for debugging as you are able to set breakpoints
and debug the incoming requests in order to make modifications to the server. The server allos users to register, add friends, join games, and submit game events. The server
will also perform server-side validation to ensure that the game-events that users are submitting are valid.

While you can run the server locally, you also need to have the database running in order to save data.
To run the server locally, ensure that you have [docker installed](https://docs.docker.com/get-docker/) as well as [docker-compose](https://docs.docker.com/compose/install/).
Start the database with `docker-compose up -d db`.

Alternatively, start both the database and the server at the same time with `docker-compose up -d`.

### SubterfugeClient

This is a gRPC client that automatically applies authorization headers to requests. This client library should be used for clients to avoid any hassle.

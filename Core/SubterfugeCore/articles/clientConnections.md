# Networking and Multiplayer

## Starting a local server (optional)

The Subterfuge server saves information like player login information, open lobbies, and the game events for games.
In order to login, create games, and play with other players you will need to contact a running server. This may be the online server
or may also be a local server on your own machine.

Running a server on your machine makes it easier to make modifications to the server on the fly. However, access to the online server
can be used if you are not expecting to make any server changes.

To run the server locally, ensure that you have [docker installed](https://docs.docker.com/get-docker/) as well as [docker-compose](https://docs.docker.com/compose/install/).
Once they have been installed, Clone the core git repository. Once cloned, move into the root directory of the repository and run `docker-compose up -d`. This will start the
server.

### Connecting to a server

Once the server is started, the Core repository provides a GrpcClient implementation which abstracts access to the server.

```cs
// Setup Access Strings:

// String Hostname = "52.14.116.178"; // For hosted server
String Hostname = "localhost"; // For local deployments
int Port = 5000;

// Create the client:
client = new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());

// Ensure that the client can connect to the server by accessing the health check endpoint
 try
{
    client.HealthCheck(new HealthCheckRequest());
    return isConnected = true;
}
catch (RpcException exception)
{
    client = null;
    return isConnected = false;
}
```

### Making network calls
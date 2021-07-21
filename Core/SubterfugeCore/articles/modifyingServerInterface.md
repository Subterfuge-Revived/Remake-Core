# Changing proto files

When changes are made to the `.proto` files within the `ProtoFiles` project, these files also need to be included in the unity client. To ensure that the unity client
is able to use any new networking messages, you will need to copy the `SubterfugeClient` and the `ProtoGenerated` dlls into unity. To do this, build the `SubterfugeClient` project. Once done,
find the following build files:

`<projectRoot>/Server/SubterfugeClient/obj/Debug/netstandard2.0/SubterfugeClient.dll`<br/>
`<projectRoot>/Server/ProtoGenerated/obj/Debug/netstandard2.0/ProtoGenerated.dll`<br/>

Copy both of these `.dll` files into the Unity folder and replace the existing files.

`<projectRoot>/Assets/PROTOBUF/`

### Serializing Protobuf Messages

```cs
// To bytes:
ByteArray[] message = ProtoMessage.toByteArray()

// From bytes:
ProtoMessage parsed = ProtoMessage.Parser.parseFrom(message)
```

### MongoDB

For a database, the server uses MongoDB. MongoDB is a document based database which allows JSON objects to be stored in a database. View the [NuGet package](https://www.nuget.org/packages/mongodb.driver)
or [learn how to use the MongoDB API](https://mongodb.github.io/mongo-csharp-driver/2.12/getting_started/quick_tour/) from the MongoDB docs. Luckily, MongoDB supports most Protobuf messages
with the exception of any Protobuf object which includes a `List<>` type. If the protobuf object contains a list, a conversion class will be required in the server.
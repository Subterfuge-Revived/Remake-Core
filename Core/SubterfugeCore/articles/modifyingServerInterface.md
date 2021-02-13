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

### Redis

View all of the [Redis commands](https://redis.io/commands) to understand how data is being stored. If you are confused, message @R10t--
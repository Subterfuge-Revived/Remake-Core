using System;

namespace SubterfugeCore.Models.GameEvents
{
    public interface INetworkHealthInterface
    {
        PingResponse Ping(PingRequest pingRequest);
        PingResponse AuthorizedHealthCheck(AuthorizedPingRequest authorizedPingRequest);
    }
    
    public class NetworkResponse
    {
        public ResponseStatus Status { get; set; }
    }

    public class ResponseStatus
    {
        public Boolean IsSuccess { get; set; }
        public ResponseType ResponseType { get; set; }
        public String Detail { get; set; }
        public string Uri { get; set; }
    }

    public class PingRequest { }

    public class PingResponse : NetworkResponse { }
    
    public class AuthorizedPingRequest { }
    
    public enum ResponseType
    {
        SUCCESS,                      // Used if a message is OK
        ROOM_DOES_NOT_EXIST,          
        PLAYER_DOES_NOT_EXIST,        
        CHAT_GROUP_DOES_NOT_EXIST,    
        GAME_EVENT_DOES_NOT_EXIST,    
        FRIEND_REQUEST_DOES_NOT_EXIST,
        PLAYER_ALREADY_IN_LOBBY,
        PLAYER_IS_BLOCKED,            // Used if you try to perform an action on someone who is blocked.
        DUPLICATE,                    // Used if something already exists.
        INVALID_REQUEST,              // Used if a request argument is not valid or doesn't meet requirements
        ROOM_IS_FULL,                 // Used if a game room is at capacity
        PERMISSION_DENIED,            // Used if the user tries to access something they are not allowed to
        UNAUTHORIZED,                 // Used if the user is not logged in
        INVALID_CREDENTIALS,          // Used if the player tries to login with the wrong account information
        GAME_ALREADY_STARTED,         // Used if a game has already begun
        INTERNAL_SERVER_ERROR,        // Used if an unknown exception is thrown. This probably indicates that the code is doing something wrong...
                                      // We should track these. Maybe record them in the database with as much detail as possible.
    }
}
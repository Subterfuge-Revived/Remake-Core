using System;

namespace SubterfugeCore.Models.GameEvents
{
    public class NetworkResponse
    {
        public ResponseStatus Status { get; set; }
    }

    public class ResponseStatus
    {
        public Boolean IsSuccess { get; set; }
        public ResponseType ResponseType { get; set; }
        public String Detail { get; set; }
    }

    public class PingRequest { }

    public class PingResponse : NetworkResponse { }

    public class AuthorizedPingResponse : NetworkResponse
    {
        public User LoggedInUser { get; set; }
    }
    
    public class AuthorizedPingRequest { }
    
    public enum ResponseType
    {
        UNKNOWN = 0,
        SUCCESS = 1,                      // Used if a message is OK
        DUPLICATE = 2,                    // Used if something already exists.
        INVALID_REQUEST = 3,              // Used if a request argument is not valid or doesn't meet requirements
        UNAUTHORIZED = 4,                 // Used if the user is not logged in
        INTERNAL_SERVER_ERROR = 5,        // Used if an unknown exception is thrown. This probably indicates that the code is doing something wrong...
        NOT_FOUND = 6,
        BANNED = 7,
        VALIDATION_ERROR = 8,
    }
}
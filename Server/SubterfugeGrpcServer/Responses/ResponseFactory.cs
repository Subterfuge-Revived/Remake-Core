using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Responses
{
    public class ResponseFactory
    {
        public static ResponseStatus createResponse(ResponseType type)
        {
            if (type == ResponseType.SUCCESS)
            {
                return new ResponseStatus()
                {
                    IsSuccess = true,
                    Detail = type.ToString(),
                };
            }
            return new ResponseStatus()
            {
                IsSuccess = false,
                Detail = type.ToString(),
            };
        }
    }

    public enum ResponseType
    {
        SUCCESS,                      // Used if a message is OK
        ROOM_DOES_NOT_EXIST,          
        PLAYER_DOES_NOT_EXIST,        
        CHAT_GROUP_DOES_NOT_EXIST,    
        GAME_EVENT_DOES_NOT_EXIST,    
        FRIEND_REQUEST_DOES_NOT_EXIST,
        PLAYER_IS_BLOCKED,            // Used if you try to perform an action on someone who is blocked.
        DUPLICATE,                    // Used if something already exists.
        INVALID_REQUEST,              // Used if a request argument is not valid or doesn't meet requirements
        ROOM_IS_FULL,                 // Used if a game room is at capacity
        PERMISSION_DENIED,            // Used if the user tries to access something they are not allowed to
        UNAUTHORIZED,                 // Used if the user is not logged in
        INVALID_CREDENTIALS,          // Used if the player tries to login with the wrong account information
        GAME_ALREADY_STARTED,         // Used if a game has already begun
    }
}
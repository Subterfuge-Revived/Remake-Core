using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Responses
{
    public class ResponseFactory
    {
        public static ResponseStatus createResponse(ResponseType type, string? details="")
        {
            if (type == ResponseType.SUCCESS)
            {
                return new ResponseStatus()
                {
                    IsSuccess = true,
                    ResponseType = type,
                    Detail = details,
                };
            }
            return new ResponseStatus()
            {
                IsSuccess = false,
                ResponseType = type,
                Detail = details,
            };
        }
    }
}
using System;

namespace SubterfugeCoreCLI.Response
{
    [Serializable]
    public class FailureResponse : Response
    {
        public FailureResponse(string message)
        {
            this.Success = false;
            this.Message = message;
        }
    }
}
using System;

namespace SubterfugeCoreCLI.Response
{
    [Serializable]
    public class ValidateEventResponse : Response
    {
        public bool IsValid { get; set; }

        public ValidateEventResponse(string message, bool isValid)
        {
            this.Message = message;
            this.Success = true;
            this.IsValid = isValid;
        }
    }
}
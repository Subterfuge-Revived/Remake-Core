using System;
using Newtonsoft.Json;

namespace SubterfugeCoreCLI.Response
{
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
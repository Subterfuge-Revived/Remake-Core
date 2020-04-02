using System;
using Newtonsoft.Json;

namespace SubterfugeCoreCLI.Response
{
    public class ValidateEventResponse : Response
    {
        public bool isValid { get; set; }

        public ValidateEventResponse(string message, bool isValid)
        {
            this.message = message;
            this.success = true;
            this.isValid = isValid;
        }
    }
}
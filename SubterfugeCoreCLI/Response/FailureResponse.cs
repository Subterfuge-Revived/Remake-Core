using System;
using Newtonsoft.Json;

namespace SubterfugeCoreCLI.Response
{
    public class FailureResponse : Response
    {
        public FailureResponse(string message)
        {
            this.success = false;
            this.message = message;
        }
    }
}
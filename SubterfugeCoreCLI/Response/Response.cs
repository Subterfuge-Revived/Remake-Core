using System;

namespace SubterfugeCoreCLI.Response
{
    [Serializable]
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        
    }
}
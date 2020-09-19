using System;
using Jil;

namespace SubterfugeCoreCLI.Response
{
    [Serializable]
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        
        public override string ToString()
        {
            return JSON.Serialize(this);
        }
    }
}
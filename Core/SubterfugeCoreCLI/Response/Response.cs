using Newtonsoft.Json;

namespace SubterfugeCoreCLI.Response
{
    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
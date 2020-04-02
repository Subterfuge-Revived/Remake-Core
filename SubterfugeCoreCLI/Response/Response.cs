using Newtonsoft.Json;

namespace SubterfugeCoreCLI.Response
{
    public class Response
    {
        public bool success { get; set; }
        public string message { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
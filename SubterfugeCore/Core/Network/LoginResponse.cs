namespace SubterfugeCore.Core.Network
{
    public class LoginResponse : NetworkResponse
    {
        public NetworkUser user { get; set; }
        public string token { get; set; }
    }
}
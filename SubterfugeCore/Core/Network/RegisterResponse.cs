namespace SubterfugeCore.Core.Network
{
    public class RegisterResponse : NetworkResponse
    {
        public NetworkUser user { get; set; }
        public string token { get; set; }
    }
}
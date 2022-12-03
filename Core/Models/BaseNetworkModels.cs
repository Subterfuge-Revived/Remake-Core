using System;

namespace SubterfugeCore.Models.GameEvents
{
    public interface INetworkHealthInterface
    {
        PingResponse Ping(PingRequest pingRequest);
        PingResponse AuthorizedHealthCheck(AuthorizedPingRequest authorizedPingRequest);
    }
    
    public class NetworkResponse
    {
        public ResponseStatus Status { get; set; }
    }

    public class ResponseStatus
    {
        public Boolean IsSuccess { get; set; }
        public String Detail { get; set; }
    }

    public class PingRequest { }

    public class PingResponse : NetworkResponse { }
    
    public class AuthorizedPingRequest { }
}
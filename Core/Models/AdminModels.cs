using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class ServerAction
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string RemoteIpAddress { get; set; }
        public string HttpMethod { get; set; }
        public string RequestUrl { get; set; }
        public int? StatusCode { get; set; }
        public DateTime TimeProcessed { get; set; } = DateTime.UtcNow;
    }
    
    public class ServerActionLogResponse
    {
        public List<ServerAction> Actions { get; set; }
    }
    
    public class ServerException
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; }
        public string UserId { get; set; }
        public string RemoteIpAddress { get; set; }
        public string HttpMethod { get; set; }
        public string RequestUri { get; set; }
        public string RequestBody { get; set; }
        public string QueryString { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionSource { get; set; }
        public string StackTrace { get; set; }
        public DateTime UnixTimeProcessed { get; set; } = DateTime.UtcNow;
    }
    
    public class ServerExceptionLogResponse
    {
        public List<ServerException> Exceptions { get; set; }
    }
}
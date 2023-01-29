#nullable enable
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

    public class ServerActionLogRequeset
    {
        public int Pagination { get; set; } = 1;
        public string? Username { get; set; } = null;
        public string? UserId { get; set; } = null;
        public string? HttpMethod { get; set; } = null;
        public string? RequestUrl { get; set; } = null;
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

    public class ServerExceptionLogRequest
    {
        public int Pagination { get; set; } = 1;
        public string? Username { get; set; } = null;
        public string? UserId { get; set; } = null;
        public string? HttpMethod { get; set; } = null;
        public string? RequestUrl { get; set; } = null;
        public string? ExceptionSource { get; set; } = null;
        public string? RemoteIpAddress { get; set; } = null;
    }
    
    public class ServerExceptionLogResponse
    {
        public List<ServerException> Exceptions { get; set; }
    }

    public class GetIpBansResponse
    {
        public List<IpBans> BannedIps { get; set; }
    }

    public class IpBans
    {
        public string IpOrRegex { get; set; }
        public DateTime DateApplied { get; set; }
        public DateTime BannedUntil { get; set; }
        public string AdminNotes { get; set; }
    }

    public class GetBannedPlayerResponse
    {
        public List<DetailedUser> BannedUsers { get; set; }
    }

    public class Echo
    {
        public string EchoContent { get; set; }
    }
}
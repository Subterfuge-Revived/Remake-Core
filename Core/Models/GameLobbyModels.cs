using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public enum RoomStatus
    {
        Open,
        Ongoing,
        Closed,
        Completed,
        Expired,
    }
    
    public class GameConfiguration {
        public string Id { get; set; }
        public RoomStatus RoomStatus { get; set; }
        public User Creator { get; set; }
        public GameSettings GameSettings { get; set; }
        public MapConfiguration MapConfiguration { get; set; }
        public string RoomName { get; set; }
        public string GameVersion { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<User> PlayersInLobby { get; set; }
    }

    public class GameSettings
    {
        public List<SpecialistConfiguration> AllowedSpecialists { get; set; } = new List<SpecialistConfiguration>();
        public double MinutesPerTick { get; set; }
        public Goal Goal { get; set; }
        public Boolean IsRanked { get; set; }
        public Boolean IsAnonymous { get; set; }
        public Boolean IsPrivate { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class MapConfiguration
    {
        public int Seed { get; set; }
        public int OutpostsPerPlayer { get; set; }
        public int MinimumOutpostDistance { get; set; }
        public int MaximumOutpostDistance { get; set; }
        public int DormantsPerPlayer { get; set; }
        public OutpostDistribution OutpostDistribution { get; set; }
    }

    public class OutpostDistribution
    {
        public float GeneratorWeight { get; set; }
        public float FactoryWeight { get; set; }
        public float WatchtowerWeight { get; set; }
    }

    public enum Goal
    {
        Mining,
        Domination
    }

    public class GetLobbyRequest
    {
        public int Pagination { get; set; } = 1;
        public RoomStatus RoomStatus { get; set; } = RoomStatus.Open;
        public string? CreatedByUserId { get; set; } = null;
        public string? UserIdInRoom { get; set; } = null;
        public string? RoomId { get; set; } = null;
        public Goal? Goal { get; set; } = null;
        public int MinPlayers { get; set; } = 0;
        public int MaxPlayers { get; set; } = 999;
        public bool? IsAnonymous { get; set; } = null;
        public bool? IsRanked { get; set; } = null;
    }

    public class OpenLobbiesResponse : NetworkResponse
    {
        public GameConfiguration[] Rooms { get; set; }
    }
    
    public class PlayerCurrentGamesRequest {}

    public class CreateRoomRequest
    {
        public GameSettings GameSettings { get; set; }
        public MapConfiguration MapConfiguration { get; set; }
        public string RoomName { get; set; }
        public Boolean IsPrivate { get; set; }
    }

    public class CreateRoomResponse : NetworkResponse
    {
        public GameConfiguration GameConfiguration { get; set; }
    }

    public class JoinRoomResponse : NetworkResponse { }

    public class JoinRoomRequest { }

    public class LeaveRoomResponse : NetworkResponse { }
    
    public class StartGameEarlyResponse : NetworkResponse { }

    public class GetLobbyResponse : NetworkResponse
    {
        public GameConfiguration[] Lobbies { get; set; }
    }
}
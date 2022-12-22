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
        public List<SpecialistConfiguration> AllowedSpecialists { get; set; }
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

    public class OpenLobbiesRequest
    {
        public RoomStatus RoomStatus { get; set; }
        public Boolean joinedLobbies { get; set; }
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

    public class GetLobbyRequest
    {
        public string IdFilter { get; set; }
        public RoomStatus RoomStatusFilter { get; set; }
        public string CreatorIdFilter { get; set; }
        // Game Config Filters
        public string[] SpecialistIsAllowed { get; set; }
        public Goal GoalFilter { get; set; }
        public string RoomNameFilter { get; set; }
        public string GameVersionFilter { get; set; }
        public string[] PlayersIdIsInLobby { get; set; }
    }

    public class GetLobbyResponse : NetworkResponse
    {
        public GameConfiguration[] Lobbies { get; set; }
    }
}
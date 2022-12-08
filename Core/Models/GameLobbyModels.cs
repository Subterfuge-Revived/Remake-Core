using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{

    public interface INetworkLobbyController
    {
        OpenLobbiesResponse GetOpenLobbies(OpenLobbiesRequest lobbyRequest);
        PlayerCurrentGamesResponse GetPlayerCurrentGames(PlayerCurrentGamesRequest currentGamesRequest);
        CreateRoomResponse CreateNewRoom(CreateRoomRequest createRoomRequest);
        JoinRoomResponse JoinRoom(JoinRoomRequest joinRoomRequest);
        LeaveRoomResponse LeaveRoom(LeaveRoomRequest leaveRoomRequest);
        StartGameEarlyResponse StartGameEarly(StartGameEarlyRequest startGameEarlyRequest);
    }

    public enum RoomStatus
    {
        Open,
        Ongoing,
        Closed,
        Private,
    }
    
    public class GameConfiguration {
        public string Id { get; set; }
        public RoomStatus RoomStatus { get; set; }
        public User Creator { get; set; }
        public GameSettings GameSettings { get; set; }
        public MapConfiguration MapConfiguration { get; set; }
        public string RoomName { get; set; }
        public long UnixTimeCreated { get; set; }
        public long UnixTimeStarted { get; set; }
        public List<User> PlayersInLobby { get; set; }
    }

    public class GameSettings
    {
        public List<SpecialistConfiguration> AllowedSpecialists { get; set; }
        public double MinutesPerTick { get; set; }
        public Goal Goal { get; set; }
        public Boolean IsRanked { get; set; }
        public Boolean IsAnonymous { get; set; }
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
        private List<GameConfiguration> Rooms { get; set; }
    }
    
    public class PlayerCurrentGamesRequest {}

    public class PlayerCurrentGamesResponse : NetworkResponse
    {
        public List<GameConfiguration> Games { get; set; }
    }

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

    public class LeaveRoomRequest { }

    public class LeaveRoomResponse : NetworkResponse { }
    
    public class StartGameEarlyRequest { }
    public class StartGameEarlyResponse : NetworkResponse { }
}
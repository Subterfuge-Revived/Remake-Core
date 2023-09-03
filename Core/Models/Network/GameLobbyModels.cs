﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Subterfuge.Remake.Api.Network
{
    public enum RoomStatus
    {
        Unknown = 0,
        Open = 1,
        Ongoing = 2,
        Closed = 3,
        Completed = 4,
        Expired = 5,
    }
    
    public class GameConfiguration
    {
        public string Id { get; set; } = "Debug";
        public RoomStatus RoomStatus { get; set; } = RoomStatus.Ongoing;
        public User Creator { get; set; } = new SimpleUser().ToUser();
        public GameSettings GameSettings { get; set; } = new GameSettings();
        public MapConfiguration MapConfiguration { get; set; } = new MapConfiguration();
        public string RoomName { get; set; } = "Default";
        public GameVersion GameVersion { get; set; } = GameVersion.ALPHA01;
        public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
        public DateTime TimeStarted { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
        public List<User> PlayersInLobby { get; set; } = new List<User>() { new SimpleUser() { Id = "1", Username = "Test1"}.ToUser(), new SimpleUser(){ Id = "2", Username = "Test2"}.ToUser() };

        public Dictionary<string, List<SpecialistTypeId>> PlayerSpecialistDecks { get; set; } =
            new Dictionary<string, List<SpecialistTypeId>>();
    }
    
    public enum GameVersion
    {
        UNKNOWN = 0,
        ALPHA01 = 1,
    }

    public class GameSettings
    {
        public double MinutesPerTick { get; set; } = 10;
        public Goal Goal { get; set; } = Goal.Domination;
        public Boolean IsRanked { get; set; } = false;
        public Boolean IsAnonymous { get; set; } = true;
        public Boolean IsPrivate { get; set; } = false;
        public int MaxPlayers { get; set; } = 2;
    }

    public class MapConfiguration
    {
        public int Seed { get; set; } = 1234;
        public int OutpostsPerPlayer { get; set; } = 1;
        public int MinimumOutpostDistance { get; set; } = 50;
        public int MaximumOutpostDistance { get; set; } = 200;
        public int DormantsPerPlayer { get; set; } = 4;
        public OutpostDistribution OutpostDistribution { get; set; } = new OutpostDistribution();
    }

    public class OutpostDistribution
    {
        public float GeneratorWeight { get; set; } = 0.45f;
        public float FactoryWeight { get; set; } = 0.45f;
        public float WatchtowerWeight { get; set; } = 0.1f;
    }

    public enum Goal
    {
        Unknown = 0,
        Mining = 1,
        Domination = 2,
    }

    public class GetLobbyRequest
    {
        public int Pagination { get; set; } = 1;
        public RoomStatus? RoomStatus { get; set; } = null;
        public string? CreatedByUserId { get; set; } = null;
        public string? UserIdInRoom { get; set; } = null;
        public string? RoomId { get; set; } = null;
        public Goal? Goal { get; set; } = null;
        public int MinPlayers { get; set; } = 0;
        public int MaxPlayers { get; set; } = 999;
        public bool? IsAnonymous { get; set; } = null;
        public bool? IsRanked { get; set; } = null;
    }

    public class OpenLobbiesResponse
    {
        public GameConfiguration[] Rooms { get; set; } = new GameConfiguration[] { };
    }
    
    public class PlayerCurrentGamesRequest {}

    public class CreateRoomRequest
    {
        public GameSettings GameSettings { get; set; }
        public MapConfiguration MapConfiguration { get; set; }
        public string RoomName { get; set; }
        public Boolean IsPrivate { get; set; }
        public List<SpecialistTypeId> CreatorSpecialistDeck { get; set; }
    }

    public class CreateRoomResponse
    {
        public GameConfiguration GameConfiguration { get; set; }
    }

    public class JoinRoomResponse { }

    public class JoinRoomRequest
    {
        public List<SpecialistTypeId> SpecialistDeck { get; set; }
    }

    public class LeaveRoomResponse { }
    
    public class StartGameEarlyResponse { }

    public class GetLobbyResponse
    {
        public GameConfiguration[] Lobbies { get; set; }
    }

    public enum SpecialistTypeId
    {
        Unknown = 0,
        Infiltrator = 1,
        Helmsman = 2,
        Veteran = 3,
        Dispatcher = 4,
        Assasin = 5,
        Pirate = 6,
        Inspector = 7,
        Smuggler = 8,
        Sentry = 9,
        Revered_Elder = 10,
        Saboteur = 11,
        Princess = 12,
        Intelligence_Officer = 13,
        Foreman = 14,
        Tinkerer = 15,
        Hypnotist = 16,
        Warden = 17,
        Technician = 18,
        Automation = 19,
        Advisor = 20,
        Amnesiac = 21,
        Martyr = 22,
        Sniper = 23,
        Iron_Maiden = 24,
        Scrutineer = 25,
        Theif = 26,
        Escort = 27,
        Sapper = 28,
        Icicle = 29,
        Engineer = 30,
        Enforcer = 31,
        Merchant = 32,
        Economist = 33,
        Double_Agent = 34,
        Industrialist = 35,
        Breeder = 36,
        SignalJammer = 37,
        Bolster = 38,
        
        // Heroes in the 1000 range
        Queen = 1001,
    }
}
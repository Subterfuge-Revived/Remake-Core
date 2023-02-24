using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Controllers.game;

[ApiController]
[Authorize]
[Route("api/lobby/")]
public class LobbyController : ControllerBase, ISubterfugeGameLobbyApi
{
    
    private IDatabaseCollection<DbUserModel> _dbUserCollection;
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbLobbies;
    private IDatabaseCollection<DbGameEvent> _dbGameEvents;

    public LobbyController(IDatabaseCollectionProvider mongo)
    {
        this._dbLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
        this._dbGameEvents = mongo.GetCollection<DbGameEvent>();
        this._dbUserCollection = mongo.GetCollection<DbUserModel>();
    }
    
    [HttpGet]
    public async Task<SubterfugeResponse<GetLobbyResponse>> GetLobbies([FromQuery] GetLobbyRequest request)
    {
        DbUserModel user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<GetLobbyResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
        
        // If the player is not an admin; Ensure they only see their own lobbies (other than public ones)
        if (!user.HasClaim(UserClaim.Administrator))
        {
            if (request.UserIdInRoom.IsNullOrEmpty())
            {
                if (request.RoomStatus != RoomStatus.Open)
                {
                    // Only show closed/ongoing/private games created by the user.
                    // Cannot see games that you are not in unless you are an admin.
                    request.UserIdInRoom = user.Id;
                }
            }
            else
            {
                if (request.UserIdInRoom != user.Id)
                    return SubterfugeResponse<GetLobbyResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot search for rooms other players are in.");
            }
        }

        var query = _dbLobbies.Query();

        if (!request.CreatedByUserId.IsNullOrEmpty())
            query = query.Where(it => it.Creator.Id == request.CreatedByUserId);

        if(request.RoomStatus != null)
            query = query.Where(it => it.RoomStatus == request.RoomStatus);
        
        if (!request.RoomId.IsNullOrEmpty())
            query = query.Where(it => it.Id == request.RoomId);
        
        
        query = query.Where(it => it.GameSettings.MaxPlayers >= request.MinPlayers);
        query = query.Where(it => it.GameSettings.MaxPlayers <= request.MaxPlayers);
        
        if(request.IsAnonymous != null) 
            query = query.Where(it => it.GameSettings.IsAnonymous == request.IsAnonymous);
        
        if(request.IsRanked != null)
            query = query.Where(it => it.GameSettings.IsRanked == request.IsRanked);

        if (request.Goal != null)
            query = query.Where(it => it.GameSettings.Goal == request.Goal);

        if (!request.UserIdInRoom.IsNullOrEmpty())
            query = query.Where(it => it.PlayerIdsInLobby.Contains(request.UserIdInRoom));
        
        
        var matchingRooms = (await query
            .OrderByDescending(it => it.TimeCreated)
            .Skip(50 * (request.Pagination - 1))
            .Take(50)
            .ToListAsync())
            .Select(async it => await it.ToGameConfiguration(_dbUserCollection))
            .ToArray();
            
        GetLobbyResponse roomResponse = new GetLobbyResponse();
        roomResponse.Lobbies = await Task.WhenAll(matchingRooms);
            
        return SubterfugeResponse<GetLobbyResponse>.OfSuccess(roomResponse);
    }

    [HttpPost]
    [Route("create")]
    public async Task<SubterfugeResponse<CreateRoomResponse>> CreateNewRoom(CreateRoomRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<CreateRoomResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        // Ensure max players is over 1
        if (request.GameSettings.MaxPlayers < 2)
            return SubterfugeResponse<CreateRoomResponse>.OfFailure(ResponseType.VALIDATION_ERROR, "A lobby requires MinPlayers to be 2 or more.");
        
        // Ensure the player has specified a deck
        if(request.CreatorSpecialistDeck.Count != Constants.REQUIRED_PLAYER_SPECIALIST_DECK_SIZE)
            return SubterfugeResponse<CreateRoomResponse>.OfFailure(ResponseType.VALIDATION_ERROR, $"Specialist deck contains only {request.CreatorSpecialistDeck.Count} but requires exactly {Constants.REQUIRED_PLAYER_SPECIALIST_DECK_SIZE}.");

        var room = DbGameLobbyConfiguration.FromRequest(request, dbUserModel.ToUser());
        await _dbLobbies.Upsert(room);
               
        return SubterfugeResponse<CreateRoomResponse>.OfSuccess(new CreateRoomResponse()
        {
            GameConfiguration = await room.ToGameConfiguration(_dbUserCollection),
        });
    }

    [HttpPost]
    [Route("{guid}/join")]
    public async Task<SubterfugeResponse<JoinRoomResponse>> JoinRoom(JoinRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var room = await _dbLobbies.Query().Where(it => it.Id == guid).FirstOrDefaultAsync();
        if (room == null)
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the room you wish to enter.");

        if (room.GameSettings.MaxPlayers <= room.PlayerIdsInLobby.Count)
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.ROOM_IS_FULL, "Lobby is full.");

        if (room.PlayerIdsInLobby.Contains(dbUserModel.Id))
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.DUPLICATE, "You are already a member of this room.");

        if (room.RoomStatus != RoomStatus.Open)
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.GAME_ALREADY_STARTED, "You're too late! Your friends decided to play without you.");
        
        // Ensure the player has specified a deck
        if(request.SpecialistDeck.Count != Constants.REQUIRED_PLAYER_SPECIALIST_DECK_SIZE)
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.VALIDATION_ERROR, $"Specialist deck contains only {request.SpecialistDeck.Count} but requires exactly {Constants.REQUIRED_PLAYER_SPECIALIST_DECK_SIZE}.");

        // Check if any players in the room are "pseudonyms" / multiboxing
        if((await room.GetPlayersInLobby(_dbUserCollection)).Any(roomMember => roomMember.Pseudonyms.Any(pseudoUser => pseudoUser.Id == dbUserModel.Id)))
            return SubterfugeResponse<JoinRoomResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot join a game with an account registered to the same device");
        
        // Check if this is the last player to join
        if (room.PlayerIdsInLobby.Count == room.GameSettings.MaxPlayers - 1)
        {
            room.RoomStatus = RoomStatus.Ongoing;
            room.TimeStarted = DateTime.UtcNow;
        }
        
        room.PlayerIdsInLobby.Add(dbUserModel.Id);
        room.PlayerSpecialistDecks.Add(dbUserModel.Id, request.SpecialistDeck);
        await _dbLobbies.Upsert(room);

        return SubterfugeResponse<JoinRoomResponse>.OfSuccess(new JoinRoomResponse());
    }
    
    [HttpGet]
    [Route("{guid}/leave")]
    public async Task<SubterfugeResponse<LeaveRoomResponse>> LeaveRoom(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<LeaveRoomResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var room = await _dbLobbies.Query().FirstOrDefaultAsync(it => it.Id == guid);
        if (room == null)
            return SubterfugeResponse<LeaveRoomResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot leave a room that does not exist");

        if (!room.PlayerIdsInLobby.Contains(dbUserModel.Id))
            return SubterfugeResponse<LeaveRoomResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "You are not a member of this lobby.");

        if (room.RoomStatus == RoomStatus.Open)
        {
            if (room.Creator.Id == dbUserModel.Id)
            {
                // The creator is leaving the game. Destroy the lobby.
                await _dbLobbies.Delete(room);
                return SubterfugeResponse<LeaveRoomResponse>.OfSuccess(new LeaveRoomResponse());
            }
        
            // Update the player list to remove the current player.
            room.PlayerIdsInLobby = room.PlayerIdsInLobby.Where(it => it != dbUserModel.Id).ToList();
            room.PlayerSpecialistDecks.Remove(dbUserModel.Id);
            await _dbLobbies.Upsert(room);

            return SubterfugeResponse<LeaveRoomResponse>.OfSuccess(new LeaveRoomResponse());
        }
        
        if (room.RoomStatus == RoomStatus.Ongoing)
        {
            // Create player leave event.
            // Get current tick
            GameTick now = new GameTick(room.TimeStarted, DateTime.UtcNow);

            var leaveEvent = new DbGameEvent()
            {
                OccursAtTick = now.GetTick(),
                GameEventType = EventDataType.PlayerLeaveGameEventData,
                SerializedEventData = JsonConvert.SerializeObject(new PlayerLeaveGameEventData()
                {
                    Player = dbUserModel.ToSimpleUser()
                }),
                IssuedBy = dbUserModel.ToSimpleUser(),
                RoomId = room.Id,
            };
            await _dbGameEvents.Upsert(leaveEvent);

            return SubterfugeResponse<LeaveRoomResponse>.OfSuccess(new LeaveRoomResponse());
        }
        
        return SubterfugeResponse<LeaveRoomResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot leave a room after it is over.");
    }
    
    [HttpGet]
    [Route("{guid}/start")]
    public async Task<SubterfugeResponse<StartGameEarlyResponse>> StartGameEarly(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<StartGameEarlyResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var room = await _dbLobbies.Query().FirstOrDefaultAsync(it => it.Id == guid);
        if (room == null)
            return SubterfugeResponse<StartGameEarlyResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified room does not exist.");

        if (room.Creator.Id != dbUserModel.Id && !dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<StartGameEarlyResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "You are not the creator of the room; you cannot start the game early.");

        if (room.PlayerIdsInLobby.Count < 2)
            return SubterfugeResponse<StartGameEarlyResponse>.OfFailure(ResponseType.VALIDATION_ERROR, "Playing with yourself might be fun, but consider finding some friends first. Cannot start the game without at least 2 players.");

        room.RoomStatus = RoomStatus.Ongoing;
        room.TimeStarted = DateTime.UtcNow;
        room.GameSettings.MaxPlayers = room.PlayerIdsInLobby.Count;
        await _dbLobbies.Upsert(room);
        
        return SubterfugeResponse<StartGameEarlyResponse>.OfSuccess(new StartGameEarlyResponse());
    }
}
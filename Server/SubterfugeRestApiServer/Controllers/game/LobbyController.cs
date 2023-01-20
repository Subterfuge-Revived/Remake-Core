using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

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
    public async Task<GetLobbyResponse> GetLobbies([FromQuery] GetLobbyRequest request)
    {
        DbUserModel user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            throw new UnauthorizedException();
        
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
                    throw new ForbidException();
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
        roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
        return roomResponse;
    }

    [HttpPost]
    [Route("create")]
    public async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
            
        // Ensure max players is over 1
        if (request.GameSettings.MaxPlayers < 2)
            throw new BadRequestException("A lobby requires at least 2 players");

        var room = DbGameLobbyConfiguration.FromRequest(request, dbUserModel.ToUser());
        await _dbLobbies.Upsert(room);
               
        return new CreateRoomResponse()
        {
            GameConfiguration = await room.ToGameConfiguration(_dbUserCollection),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }

    [HttpPost]
    [Route("{guid}/join")]
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
            
        var room = await _dbLobbies.Query().Where(it => it.Id == guid).FirstOrDefaultAsync();
        if (room == null)
            throw new NotFoundException("Cannot find the room you wish to enter.");

        if (room.GameSettings.MaxPlayers <= room.PlayerIdsInLobby.Count)
            throw new BadRequestException("This room has too many players.");

        if (room.PlayerIdsInLobby.Contains(dbUserModel.Id))
            throw new ConflictException("Memory loss? You are already a member of this room.");

        if (room.RoomStatus != RoomStatus.Open)
            throw new BadRequestException("You're too late! Your friends decided to play without you.");
        
        if(room.PlayerIdsInLobby.Count >= room.GameSettings.MaxPlayers)
            throw new BadRequestException("The room is already full. Please try another lobby.");
        
        // Check if any players in the room are "pseudonyms" / multiboxing
        if((await room.GetPlayersInLobby(_dbUserCollection)).Any(roomMember => roomMember.Pseudonyms.Any(pseudoUser => pseudoUser.Id == dbUserModel.Id)))
            throw new ForbidException();
        
        // Check if this is the last player to join
        if (room.PlayerIdsInLobby.Count == room.GameSettings.MaxPlayers - 1)
        {
            room.RoomStatus = RoomStatus.Ongoing;
            room.TimeStarted = DateTime.UtcNow;
        }
        
        room.PlayerIdsInLobby.Add(dbUserModel.Id);
        await _dbLobbies.Upsert(room);

        return new JoinRoomResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpGet]
    [Route("{guid}/leave")]
    public async Task<LeaveRoomResponse> LeaveRoom(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
            
        var room = await _dbLobbies.Query().FirstOrDefaultAsync(it => it.Id == guid);
        if (room == null)
            throw new NotFoundException("Cannot leave a room that does not exist");

        if (!room.PlayerIdsInLobby.Contains(dbUserModel.Id))
            throw new NotFoundException("You are not a member of this lobby.");

        if (room.RoomStatus == RoomStatus.Open)
        {
            if (room.Creator.Id == dbUserModel.Id)
            {
                // The creator is leaving the game. Destroy the lobby.
                await _dbLobbies.Delete(room);
                return new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
                };
            }
        
            // Update the player list to remove the current player.
            room.PlayerIdsInLobby = room.PlayerIdsInLobby.Where(it => it != dbUserModel.Id).ToList();
            await _dbLobbies.Upsert(room);

            return new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }
        
        if (room.RoomStatus == RoomStatus.Ongoing)
        {
            // Create player leave event.
            // Get current tick
            GameTick now = new GameTick(room.TimeStarted, DateTime.UtcNow);
            
            var leaveEvent = new DbGameEvent()
            {
                EventData = new PlayerLeaveGameEventData()
                {
                    Player = dbUserModel.ToUser()
                },
                IssuedBy = dbUserModel.ToUser(),
                OccursAtTick = now.GetTick(),
                RoomId = room.Id,
            };
            await _dbGameEvents.Upsert(leaveEvent);

            return new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        throw new BadRequestException("Cannot leave a room after it is over.");
    }
    
    [HttpGet]
    [Route("{guid}/start")]
    public async Task<StartGameEarlyResponse> StartGameEarly(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
            
        var room = await _dbLobbies.Query().FirstOrDefaultAsync(it => it.Id == guid);
        if (room == null)
            throw new NotFoundException("The specified room does not exist.");

        if (room.Creator.Id != dbUserModel.Id && !dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        if (room.PlayerIdsInLobby.Count < 2)
            throw new BadRequestException("Playing with yourself might be fun, but consider finding some friends first.");

        room.RoomStatus = RoomStatus.Ongoing;
        room.TimeStarted = DateTime.UtcNow;
        room.GameSettings.MaxPlayers = room.PlayerIdsInLobby.Count;
        await _dbLobbies.Upsert(room);
        
        return new StartGameEarlyResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }
}
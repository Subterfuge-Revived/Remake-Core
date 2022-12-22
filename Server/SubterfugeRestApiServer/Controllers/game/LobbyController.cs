using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/lobby/")]
public class LobbyController : ControllerBase
{
    
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbLobbies;
    private IDatabaseCollection<DbGameEvent> _dbGameEvents;

    public LobbyController(IDatabaseCollectionProvider mongo)
    {
        this._dbLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
        this._dbGameEvents = mongo.GetCollection<DbGameEvent>();
    }
    
    [HttpGet]
    public async Task<ActionResult<GetLobbyResponse>> GetLobbies(
        int pagination = 1,
        RoomStatus roomStatus = RoomStatus.Open,
        string? createdByUserId = null,
        string? userIdInRoom = null,
        string? roomId = null,
        Goal? goal = null,
        int? minPlayers = 0,
        int? maxPlayers = 999,
        bool? isAnonymous = null,
        bool? isRanked = null
    ) {
        DbUserModel user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
        
        // If the player is not an admin; Ensure they only see their own lobbies (other than public ones)
        if (!user.HasClaim(UserClaim.Administrator))
        {
            if (userIdInRoom.IsNullOrEmpty())
            {
                if (roomStatus != RoomStatus.Open)
                {
                    // Only show closed/ongoing/private games created by the user.
                    // Cannot see games that you are not in unless you are an admin.
                    userIdInRoom = user.Id;
                }
            }
            else
            {
                if (userIdInRoom != user.Id)
                    return Forbid();
            }
        }

        var query = _dbLobbies.Query();

        if (!createdByUserId.IsNullOrEmpty())
            query = query.Where(it => it.Creator.Id == createdByUserId);

        if (roomStatus != null)
            query = query.Where(it => it.RoomStatus == roomStatus);
        
        if (!roomId.IsNullOrEmpty())
            query = query.Where(it => it.Id == roomId);
        
        
        query = query.Where(it => it.GameSettings.MaxPlayers >= (minPlayers ?? 0));
        query = query.Where(it => it.GameSettings.MaxPlayers <= (maxPlayers ?? 999));
        
        if(isAnonymous != null) 
            query = query.Where(it => it.GameSettings.IsAnonymous == isAnonymous);
        
        if(isRanked != null)
            query = query.Where(it => it.GameSettings.IsRanked == isRanked);

        if (goal != null)
            query = query.Where(it => it.GameSettings.Goal == goal);

        if (!userIdInRoom.IsNullOrEmpty())
            query = query.Where(it => it.PlayersInLobby.Any(player => player.Id == userIdInRoom));
        
        
        var matchingRooms = (await query
            .OrderByDescending(it => it.TimeCreated)
            .Skip(50 * (pagination - 1))
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToGameConfiguration())
            .ToArray();
            
        GetLobbyResponse roomResponse = new GetLobbyResponse();
        roomResponse.Lobbies = matchingRooms;
        roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
        return Ok(roomResponse);
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<CreateRoomResponse>> CreateNewRoom(CreateRoomRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        // Ensure max players is over 1
        if (request.GameSettings.MaxPlayers < 2)
            return BadRequest("A lobby requires at least 2 players");

        var room = DbGameLobbyConfiguration.FromRequest(request, dbUserModel.ToUser());
        await _dbLobbies.Upsert(room);
               
        return Ok(new CreateRoomResponse()
        {
            GameConfiguration = room.ToGameConfiguration(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }

    [HttpPost]
    [Route("{guid}/join")]
    public async Task<ActionResult<JoinRoomResponse>> JoinRoom(JoinRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        var room = await _dbLobbies.Query().FirstAsync(it => it.Id == guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to enter."));
        
        if (room.GameSettings.MaxPlayers <= room.PlayersInLobby.Count)
            return  UnprocessableEntity(ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL, "This room has too many players."));
            
        if(room.PlayersInLobby.Any(it => it.Id == dbUserModel.Id))
            return Conflict(ResponseFactory.createResponse(ResponseType.PLAYER_ALREADY_IN_LOBBY, "Memory loss? You are already a member of this room."));
            
        if(room.RoomStatus != RoomStatus.Open)
            return UnprocessableEntity(ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED, "You're too late! Your friends decided to play out without you."));
        
        if(room.PlayersInLobby.Count >= room.GameSettings.MaxPlayers)
            return UnprocessableEntity(ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL, "The room is already full. Please try another lobby."));
        
        // Check if any players in the room are "pseudonyms" / multiboxing
        if(room.PlayersInLobby.Any(roomMember => roomMember.Pseudonyms.Any(pseudoUser => pseudoUser.Id == dbUserModel.Id)))
            return BadRequest(ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED));
        
        // Check if this is the last player to join
        if (room.PlayersInLobby.Count == room.GameSettings.MaxPlayers - 1)
        {
            room.RoomStatus = RoomStatus.Ongoing;
            room.TimeStarted = DateTime.UtcNow;
        }
        
        room.PlayersInLobby.Add(dbUserModel.ToUser());
        await _dbLobbies.Upsert(room);

        return Ok(new JoinRoomResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
    [HttpGet]
    [Route("{guid}/leave")]
    public async Task<ActionResult<LeaveRoomResponse>> LeaveRoom(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        var room = await _dbLobbies.Query().FirstAsync(it => it.Id == guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot leave a room that does not exist"));

        if (room.PlayersInLobby.All(it => it.Id != dbUserModel.Id))
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST,
                "You are not a member of this lobby."));

        if (room.RoomStatus == RoomStatus.Open)
        {
            if (room.Creator.Id == dbUserModel.Id)
            {
                // The creator is leaving the game. Destroy the lobby.
                await _dbLobbies.Delete(room);
                return Ok(new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
                });
            }
        
            // Update the player list to remove the current player.
            room.PlayersInLobby = room.PlayersInLobby.Where(it => it.Id != dbUserModel.Id).ToList();
            await _dbLobbies.Upsert(room);

            return Ok(new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            });
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
                    player = dbUserModel.ToUser()
                },
                IssuedBy = dbUserModel.ToUser(),
                OccursAtTick = now.GetTick(),
                RoomId = room.Id,
            };
            await _dbGameEvents.Upsert(leaveEvent);

            return Ok(new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            });
        }
        
        return BadRequest(ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED,
            "Cannot leave a room after it is over."));
    }
    
    [HttpGet]
    [Route("{guid}/start")]
    public async Task<ActionResult<StartGameEarlyResponse>> StartGameEarly(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        var room = await _dbLobbies.Query().FirstAsync(it => it.Id == guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));

        if (room.Creator.Id != dbUserModel.Id || !dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();

        if (room.PlayersInLobby.Count < 2)
            return BadRequest(ResponseFactory.createResponse(ResponseType.INVALID_REQUEST,
                "Playing with yourself might be fun, but consider finding some friends first."));

        room.RoomStatus = RoomStatus.Ongoing;
        room.TimeStarted = DateTime.UtcNow;
        room.GameSettings.MaxPlayers = room.PlayersInLobby.Count;
        await _dbLobbies.Upsert(room);
        
        return Ok(new StartGameEarlyResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
}
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/lobby/")]
public class LobbyController : ControllerBase
{
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
        DbUserModel dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var isAdmin = dbUserModel.HasClaim(UserClaim.Administrator);
        
        // If the player is not an admin; Ensure they only see their own lobbies (other than public ones)
        if (!isAdmin)
        {
            if (userIdInRoom.IsNullOrEmpty())
            {
                if (roomStatus != RoomStatus.Open)
                {
                    // Only show closed/ongoing/private games created by the user.
                    // Cannot see games that you are not in unless you are an admin.
                    userIdInRoom = dbUserModel.UserModel.Id;
                }
            }
            else
            {
                if (userIdInRoom != dbUserModel.UserModel.Id)
                    return Forbid();
            }
        }

        var filterBuilder = Builders<GameConfiguration>.Filter;
        var filter = filterBuilder.Empty;

        if (!createdByUserId.IsNullOrEmpty())
        {
            filter &= filterBuilder.Eq(room => room.Creator.Id, createdByUserId);
        }

        if (roomStatus != null)
        {
            filter &= filterBuilder.Eq(room => room.RoomStatus, roomStatus);
        }

        if (!roomId.IsNullOrEmpty())
        {
            filter &= filterBuilder.Eq(room => room.Id, roomId);
        }
        
        filter &= filterBuilder.Gt(room => room.GameSettings.MaxPlayers, minPlayers ?? 0);
        filter &= filterBuilder.Lt(room => room.GameSettings.MaxPlayers, maxPlayers ?? 999);
        
        if(isAnonymous != null) 
            filter &= filterBuilder.Eq(room => room.GameSettings.IsAnonymous, isAnonymous);
        
        if(isRanked != null)
            filter &= filterBuilder.Eq(room => room.GameSettings.IsRanked, isRanked);

        if (goal != null)
        {
            filter &= filterBuilder.Eq(room => room.GameSettings.Goal, goal);
        }

        if (!userIdInRoom.IsNullOrEmpty())
        {
            filter &= filterBuilder.ElemMatch(room => room.PlayersInLobby, player => player.Id == userIdInRoom);
        }
        
        
        var matchingRooms = (await MongoConnector.GetGameRoomCollection().FindAsync(
                filter,
                new FindOptions<GameConfiguration>() 
                {
                    Sort = Builders<GameConfiguration>.Sort.Descending(it => it.UnixTimeCreated),
                    Limit = 50,
                    Skip = 50 * (pagination - 1),
                }
            ))
            .ToList()
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
                
            
        Room room = new Room(request, dbUserModel.AsUser());
        await room.CreateInDatabase();
                
               
        return Ok(new CreateRoomResponse()
        {
            GameConfiguration = room.GameConfiguration,
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
            
        Room room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to enter."));
        
        if (room.IsRoomFull())
            return  UnprocessableEntity(ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL, "This room has too many players."));
            
        if(room.IsPlayerInRoom(dbUserModel))
            return Conflict(ResponseFactory.createResponse(ResponseType.PLAYER_ALREADY_IN_LOBBY, "Memory loss? You are already a member of this room."));
            
        if(room.GameConfiguration.RoomStatus != RoomStatus.Open)
            return UnprocessableEntity(ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED, "You're too late! Your friends decided to play out without you."));

        return Ok(new JoinRoomResponse()
        {
            Status = await room.JoinRoom(dbUserModel)
        });
    }
    
    [HttpGet]
    [Route("{guid}/leave")]
    public async Task<ActionResult<LeaveRoomResponse>> LeaveRoom(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room? room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot leave a room that does not exist"));
            
        return Ok(new LeaveRoomResponse()
        {
            Status = await room.LeaveRoom(dbUserModel)
        });
    }
    
    [HttpGet]
    [Route("{guid}/start")]
    public async Task<ActionResult<StartGameEarlyResponse>> StartGameEarly(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));

        if (room.GameConfiguration.Creator.Id != dbUserModel.UserModel.Id || !dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        return Ok(new StartGameEarlyResponse()
        {
            Status = await room.StartGame(),
        });
    }
}
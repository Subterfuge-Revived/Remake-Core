using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
public class LobbyController : ControllerBase
{
    [HttpGet]
    [Route("api/lobby")]
    public async Task<ActionResult<GetLobbyResponse>> GetLobbies()
    {
        DbUserModel dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        OpenLobbiesResponse roomResponse = new OpenLobbiesResponse();
        List<GameConfiguration> rooms = (await Room.GetOpenLobbies()).Select(it => it.GameConfiguration).ToList();
        roomResponse.Rooms.AddRange(rooms);
        roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
        return Ok(roomResponse);
    }

    [HttpGet]
    [Route("api/{userId}/lobbies")]
    public async Task<ActionResult<PlayerCurrentGamesResponse>> GetPlayerCurrentGames(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        if (currentUser.UserModel.Id == userId)
        {
            PlayerCurrentGamesResponse currentGameResponse = new PlayerCurrentGamesResponse();
            List<GameConfiguration> rooms = (await currentUser.GetActiveRooms()).Select(it => it.GameConfiguration)
                .ToList();
            currentGameResponse.Games.AddRange(rooms);
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(currentGameResponse);
        }

        if (currentUser.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? targetPlayer = await DbUserModel.GetUserFromGuid(userId);
            if (targetPlayer == null)
                return NotFound();
            
            PlayerCurrentGamesResponse currentGameResponse = new PlayerCurrentGamesResponse();
            List<GameConfiguration> rooms = (await targetPlayer.GetActiveRooms()).Select(it => it.GameConfiguration)
                .ToList();
            currentGameResponse.Games.AddRange(rooms);
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(currentGameResponse);
        }

        // Non-Admin is trying to find the games that someone else is in.
        return Unauthorized();
    }

    [HttpPost]
    [Route("api/lobby/create")]
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
    [Route("api/lobby/{guid}/join")]
    public async Task<ActionResult<JoinRoomResponse>> JoinRoom(JoinRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound("Room not found");

        return Ok(new JoinRoomResponse()
        {
            Status = await room.JoinRoom(dbUserModel)
        });
    }
    
    [HttpGet]
    [Route("api/lobby/{guid}/leave")]
    public async Task<ActionResult<LeaveRoomResponse>> LeaveRoom(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room? room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound("Room not found");
            
        return Ok(new LeaveRoomResponse()
        {
            Status = await room.LeaveRoom(dbUserModel)
        });
    }
    
    [HttpGet]
    [Route("api/lobby/{guid}/start")]
    public async Task<ActionResult<StartGameEarlyResponse>> StartGameEarly(string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(guid);
        if (room == null)
            return NotFound("Room not found");

        if (room.GameConfiguration.Creator.Id != dbUserModel.UserModel.Id || !dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        return Ok(new StartGameEarlyResponse()
        {
            Status = await room.StartGame(),
        });
    }
}
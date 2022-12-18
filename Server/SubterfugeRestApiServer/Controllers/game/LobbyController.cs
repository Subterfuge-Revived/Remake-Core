﻿using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/lobby/")]
public class LobbyController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetLobbyResponse>> GetLobbies()
    {
        DbUserModel dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        GetLobbyResponse roomResponse = new GetLobbyResponse();
        List<GameConfiguration> lobbies = (await Room.GetOpenLobbies()).Select(it => it.GameConfiguration).ToList();
        roomResponse.Lobbies = lobbies.ToArray();
        roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
        return Ok(roomResponse);
    }

    // TODO: This should just be a nullable GET parameter to the method above.
    [HttpGet]
    [Route("{userId}/lobbies")]
    public async Task<ActionResult<GetLobbyResponse>> GetPlayerCurrentGames(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        if (currentUser.UserModel.Id == userId)
        {
            GetLobbyResponse currentGameResponse = new GetLobbyResponse();
            List<GameConfiguration> rooms = (await currentUser.GetActiveRooms()).Select(it => it.GameConfiguration)
                .ToList();
            currentGameResponse.Lobbies = rooms.ToArray();
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(currentGameResponse);
        }

        if (currentUser.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? targetPlayer = await DbUserModel.GetUserFromGuid(userId);
            if (targetPlayer == null)
                return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST, "The specified player does not exist."));
            
            GetLobbyResponse currentGameResponse = new GetLobbyResponse();
            List<GameConfiguration> rooms = (await targetPlayer.GetActiveRooms()).Select(it => it.GameConfiguration)
                .ToList();
            currentGameResponse.Lobbies = rooms.ToArray();
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(currentGameResponse);
        }

        // Non-Admin is trying to find the games that someone else is in.
        return Unauthorized();
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
            return Conflict(ResponseFactory.createResponse(ResponseType.DUPLICATE, "Memory loss? You are already a member of this room."));
            
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
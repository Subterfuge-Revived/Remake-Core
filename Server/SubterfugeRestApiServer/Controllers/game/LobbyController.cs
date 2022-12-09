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
    [HttpPost]
    [Route("api/lobby")]
    public async Task<GetLobbyResponse> GetLobbies(GetLobbyRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("api/lobby/create")]
    public async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new CreateRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        // Ensure max players is over 1
        if(request.GameSettings.MaxPlayers < 2)
            return new CreateRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
            };
                
            
        Room room = new Room(request, dbUserModel.AsUser());
        await room.CreateInDatabase();
                
               
        return new CreateRoomResponse()
        {
            GameConfiguration = room.GameConfiguration,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }

    [HttpPost]
    [Route("api/lobby/{guid}/join")]
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new JoinRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(guid);
        if(room == null)
            return new JoinRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        return new JoinRoomResponse()
        {
            Status = await room.JoinRoom(dbUserModel)
        };
    }
    
    [HttpPost]
    [Route("api/lobby/{guid}/leave")]
    public async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(guid);
        if(room == null)
            return new LeaveRoomResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        return new LeaveRoomResponse()
        {
            Status = await room.LeaveRoom(dbUserModel)
        };
    }
    
    [HttpPost]
    [Route("api/lobby/{guid}/start")]
    public async Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, string guid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new StartGameEarlyResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(guid);
        if(room == null)
            return new StartGameEarlyResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        if (room.GameConfiguration.Creator.Id == dbUserModel.UserModel.Id)
        {
            return new StartGameEarlyResponse()
            {
                Status = await room.StartGame(),
            };
        }
        return new StartGameEarlyResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
        };
    }
}
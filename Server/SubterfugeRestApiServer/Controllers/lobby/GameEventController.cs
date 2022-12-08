using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/{roomId}/[controller]/[action]")]
public class GameEventController : ControllerBase
{
    public GameEventController(
        IConfiguration configuration,
        ILogger<AccountController> logger,
        string roomId
    )
    {
        _config = configuration;
        _logger = logger;
        roomGuid = roomId;
    }

    private string roomGuid;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    [HttpGet]
    public async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new GetGameRoomEventsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(roomGuid);
        if(room == null)
            return new GetGameRoomEventsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        if (room.GameConfiguration.PlayersInLobby.All(it => it.Id != dbUserModel.UserModel.Id) && !dbUserModel.HasClaim(UserClaim.Administrator))
            return new GetGameRoomEventsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
            };
            
        List<GameEventData> events = await room.GetAllGameEvents();
        // Filter out only the player's events and events that have occurred in the past.
        // Get current tick to determine events in the past.
        GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(room.GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            
        // Admins see all events :)
        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            events = events.FindAll(it =>
                it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy.Id == dbUserModel.UserModel.Id);
        }

        GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
        response.GameEvents.AddRange(events);
        response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
        return response;
    }
    
    [HttpPost(Name="submit")]
    public async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomGuid);
        if(room == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        if(!room.IsPlayerInRoom(dbUserModel))
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
            };

        return await room.AddPlayerGameEvent(dbUserModel, request.GameEventRequest);
    }
    
    [HttpPut]
    [Route("{eventGuid}")]
    public async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomGuid);
        if(room == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        // GameEventToUpdate.
        return await room.UpdateGameEvent(dbUserModel, request);
    }
    
    [HttpDelete]
    [Route("{eventGuid}")]
    public async Task<DeleteGameEventResponse> Delete(DeleteGameEventRequest request, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomGuid);
        if(room == null)
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        return await room.RemovePlayerGameEvent(dbUserModel, request.EventId);
    }
}
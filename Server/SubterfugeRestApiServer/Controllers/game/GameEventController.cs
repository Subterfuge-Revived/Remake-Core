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
public class GameEventController : ControllerBase
{
    [HttpGet]
    [Route("api/{roomId}/events")]
    public async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new GetGameRoomEventsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(roomId);
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
    
    [HttpPost]
    [Route("api/{roomId}/events")]
    public async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomId);
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
    [Route("api/{roomId}/events/{eventGuid}")]
    public async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomId);
        if(room == null)
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        // GameEventToUpdate.
        return await room.UpdateGameEvent(dbUserModel, request);
    }
    
    [HttpDelete]
    [Route("api/{roomId}/events/{eventGuid}")]
    public async Task<DeleteGameEventResponse> Delete(DeleteGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(roomId);
        if(room == null)
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        return await room.RemovePlayerGameEvent(dbUserModel, request.EventId);
    }
}
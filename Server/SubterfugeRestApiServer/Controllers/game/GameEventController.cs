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
    [Route("api/room/{roomId}/events")]
    public async Task<ActionResult<GetGameRoomEventsResponse>> GetGameRoomEvents(string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));

        if (room.GameConfiguration.PlayersInLobby.All(it => it.Id != dbUserModel.UserModel.Id) && !dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();
            
        List<GameEventData> events = await room.GetAllGameEvents();
        // Filter out only the player's events and events that have occurred in the past.
        // Get current tick to determine events in the past.
        GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(room.GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            
        // Admins see all events :)
        // TODO: Allow admins to play games. Admins should not be able to see all events if they are a player in the game. Alternatively prevent admins from joining a game.
        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            events = events.FindAll(it =>
                it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy.Id == dbUserModel.UserModel.Id);
        }

        GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
        response.GameEvents.AddRange(events);
        response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
        return Ok(response);
    }
    
    [HttpPost]
    [Route("api/room/{roomId}/events")]
    public async Task<ActionResult<SubmitGameEventResponse>> SubmitGameEvent(SubmitGameEventRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        Room? room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));

        if (!room.IsPlayerInRoom(dbUserModel))
            return Forbid();

        return Ok(await room.AddPlayerGameEvent(dbUserModel, request.GameEventRequest));
    }
    
    [HttpPut]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<ActionResult<SubmitGameEventResponse>> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        Room? room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));
            
        // GameEventToUpdate.
        return Ok(await room.UpdateGameEvent(dbUserModel, eventGuid, request));
    }
    
    [HttpDelete]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<ActionResult<DeleteGameEventResponse>> Delete(DeleteGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        Room room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to delete."));

        return Ok(await room.RemovePlayerGameEvent(dbUserModel, request.EventId));
    }
}
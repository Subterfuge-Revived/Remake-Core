using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
public class GameEventController : ControllerBase
{

    private IDatabaseCollection<DbGameEvent> _dbGameEvents;
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbGameLobbies;

    public GameEventController(IDatabaseCollectionProvider mongo)
    {
        this._dbGameEvents = mongo.GetCollection<DbGameEvent>();
        this._dbGameLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
    }
    
    [HttpGet]
    [Route("api/room/{roomId}/events")]
    public async Task<ActionResult<GetGameRoomEventsResponse>> GetGameRoomEvents(string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));

        if (lobby.PlayersInLobby.All(it => it.Id != dbUserModel.Id) && !dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        List<DbGameEvent> events = await _dbGameEvents.Query()
            .Where(it => it.RoomId == roomId)
            .OrderBy(it => it.TimeIssued)
            .ToListAsync();
            
        // Filter out only the player's events and events that have occurred in the past.
        // Get current tick to determine events in the past.
        GameTick currentTick = new GameTick(lobby.TimeStarted, DateTime.UtcNow);
            
        // Admins see all events :)
        // TODO: Allow admins to play games. Admins should not be able to see all events if they are a player in the game. Alternatively prevent admins from joining a game.
        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            events = events
                .Where(it => it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy.Id == dbUserModel.Id)
                .ToList();
        }

        GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
        response.GameEvents = events.Select(it => it.ToGameEventData()).ToList();
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

        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));

        if (lobby.PlayersInLobby.All(it => it.Id != dbUserModel.Id))
            return Forbid();

        var gameEvent = DbGameEvent.FromGameEventRequest(request, dbUserModel.ToUser(), roomId);
        await _dbGameEvents.Upsert(gameEvent);

        return Ok(new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameEventData = gameEvent.ToGameEventData(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
    [HttpPut]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<ActionResult<SubmitGameEventResponse>> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to join."));

        var gameEvent = await _dbGameEvents.Query().FirstAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST, "This game event does not exist in the specified room."));

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            return Forbid();
        
        // Determine if the event has already passed.
        GameTick currentTick = new GameTick(lobby.TimeStarted, DateTime.UtcNow);

        if (gameEvent.OccursAtTick <= currentTick.GetTick())
            return BadRequest(ResponseFactory.createResponse(ResponseType.INVALID_REQUEST,
                "Cannot delete an event that has already happened"));
        
        gameEvent.EventData = request.GameEventRequest.EventData;
        gameEvent.OccursAtTick = request.GameEventRequest.OccursAtTick;
        await _dbGameEvents.Upsert(gameEvent);
        
        return Ok(new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameEventData = gameEvent.ToGameEventData(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
    [HttpDelete]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<ActionResult<DeleteGameEventResponse>> Delete(DeleteGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "Cannot find the room you wish to delete."));
        
        var gameEvent = await _dbGameEvents.Query().FirstAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST, "This game event does not exist in the specified room."));
        
        // Determine if the event has already passed.
        GameTick currentTick = new GameTick(lobby.TimeStarted, DateTime.UtcNow);

        if (gameEvent.OccursAtTick <= currentTick.GetTick())
            return BadRequest(ResponseFactory.createResponse(ResponseType.INVALID_REQUEST,
                "Cannot delete an event that has already happened"));

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            return Forbid();

        await _dbGameEvents.Delete(gameEvent);

        return Ok(new DeleteGameEventResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
}
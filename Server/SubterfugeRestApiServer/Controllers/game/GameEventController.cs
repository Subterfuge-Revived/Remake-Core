using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
public class SubterfugeGameEventController : ControllerBase, ISubterfugeGameEventApi
{

    private IDatabaseCollection<DbGameEvent> _dbGameEvents;
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbGameLobbies;
    private IDatabaseCollection<DbUserModel> _dbUserCollection;

    public SubterfugeGameEventController(IDatabaseCollectionProvider mongo)
    {
        this._dbGameEvents = mongo.GetCollection<DbGameEvent>();
        this._dbGameLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
        this._dbUserCollection = mongo.GetCollection<DbUserModel>();
    }
    
    [HttpGet]
    [Route("api/room/{roomId}/events")]
    public async Task<GetGameRoomEventsResponse> GetGameRoomEvents(string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("Cannot find the room you wish to get events from.");

        if (lobby.PlayerIdsInLobby.All(id => id != dbUserModel.Id) && !dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        List<DbGameEvent> events = await _dbGameEvents.Query()
            .Where(it => it.RoomId == roomId)
            .OrderBy(it => it.TimeIssued)
            .ToListAsync();
            
        // Filter out only the player's events and events that have occurred in the past.
        // Get current tick to determine events in the past.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));
            
        // Admins see all events :)
        // TODO: Allow admins to play games. Admins should not be able to see all events if they are a player in the game. Alternatively prevent admins from joining a game.
        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            events = events
                .Where(it => it.EventData.OccursAtTick <= currentTick.GetTick() || it.IssuedBy.Id == dbUserModel.Id)
                .ToList();
        }

        GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
        response.GameEvents = events.Select(it => it.ToGameEventData()).ToList();
        response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
        return response;
    }

    [HttpGet]
    [Route("afagagsadf")]
    public Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, string roomId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("api/room/{roomId}/events")]
    public async Task<SubmitGameEventResponse> SubmitGameEvent(string roomId)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);

        // You shouldn't need this line anymore.
        // reader.BaseStream.Seek(0, SeekOrigin.Begin);

        // You now have the body string raw
        var body = await reader.ReadToEndAsync();

        // As well as a bound model
        var request = JsonConvert.DeserializeObject<SubmitGameEventRequest>(body);
        
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("Cannot find the room you wish to submit this event to.");

        if (!lobby.PlayerIdsInLobby.Contains(dbUserModel.Id))
            throw new ForbidException();
        
        // Determine what tick the game is currently at.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));
        if(request.GameEventData.OccursAtTick <= currentTick.GetTick())
            throw new BadRequestException("Cannot delete an event that has already happened. Current Tick:" + currentTick.GetTick());

        // Determine if the user is trying to submit an admin-only game event:
        EventDataType eventType = EventDataType.Unknown;
        EventDataType.TryParse(request.GameEventData.EventData.EventDataType, true, out eventType);
        
        if (eventType == EventDataType.PauseGameEventData ||
            eventType == EventDataType.UnpauseGameEventData ||
            eventType == EventDataType.GameEndEventData)
        {
            // Only admins can create these type of events.
            if (!dbUserModel.HasClaim(UserClaim.Administrator))
                throw new ForbidException();
        }
        
        // Attempt to parse the event data into the actual event class
        NetworkGameEventData? castEvent = null;
        switch (eventType)
        {
            case EventDataType.LaunchEventData:
                castEvent = (request.GameEventData.EventData as LaunchEventData);
                break;
            case EventDataType.DrillMineEventData:
                castEvent = (request.GameEventData.EventData as DrillMineEventData);
                break;
            case EventDataType.GameEndEventData:
                castEvent = (request.GameEventData.EventData as GameEndEventData);
                break;
            case EventDataType.PauseGameEventData:
                castEvent = (request.GameEventData.EventData as PauseGameEventData);
                break;
            case EventDataType.ToggleShieldEventData:
                castEvent = (request.GameEventData.EventData as ToggleShieldEventData);
                break;
            case EventDataType.UnpauseGameEventData:
                castEvent = (request.GameEventData.EventData as UnpauseGameEventData);
                break;
            case EventDataType.PlayerLeaveGameEventData:
                castEvent = (request.GameEventData.EventData as PlayerLeaveGameEventData);
                break;
        }
        
        if (castEvent == null)
            throw new BadRequestException($"Event type is {eventType} but the server could not parse the event as such.");

        var gameEvent = DbGameEvent.FromGameEventRequest(request, dbUserModel.ToUser(), roomId);
        await _dbGameEvents.Upsert(gameEvent);

        return new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameRoomEvent = gameEvent.ToGameEventData(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpPut]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("Cannot find the room you wish to update.");

        var gameEvent = await _dbGameEvents.Query().FirstOrDefaultAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            throw new NotFoundException("Cannot find the game event you wish to update in the specified room.");

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            throw new ForbidException();
        
        // Determine if the event has already passed.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));

        if (gameEvent.EventData.OccursAtTick <= currentTick.GetTick())
            throw new BadRequestException("Cannot delete an event that has already happened");
        
        gameEvent.EventData = request.GameEventData;
        await _dbGameEvents.Upsert(gameEvent);
        
        return new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameRoomEvent = gameEvent.ToGameEventData(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpDelete]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<DeleteGameEventResponse> DeleteGameEvent(string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("Cannot find the room you wish to delete.");
        
        var gameEvent = await _dbGameEvents.Query().FirstOrDefaultAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            throw new NotFoundException("Cannot find the game event you wish to delete.");
        
        // Determine if the event has already passed.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));

        if (gameEvent.EventData.OccursAtTick <= currentTick.GetTick())
            throw new BadRequestException("Cannot delete an event that has already happened");

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            throw new ForbidException();

        await _dbGameEvents.Delete(gameEvent);

        return new DeleteGameEventResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }
}
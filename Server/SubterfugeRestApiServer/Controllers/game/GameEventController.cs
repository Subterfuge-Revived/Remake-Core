using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Controllers.game;

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
    public async Task<SubterfugeResponse<GetGameRoomEventsResponse>> GetGameRoomEvents(string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetGameRoomEventsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<GetGameRoomEventsResponse>.OfFailure(ResponseType.NOT_FOUND, "Room not found");

        if (lobby.PlayerIdsInLobby.All(id => id != dbUserModel.Id) && !dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<GetGameRoomEventsResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be a player within the lobby to get the events.");
        
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
                .Where(it => it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy.Id == dbUserModel.Id)
                .ToList();
        }

        GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
        response.GameEvents = events.Select(it => it.ToGameEventData()).ToList();
        return SubterfugeResponse<GetGameRoomEventsResponse>.OfSuccess(response);
    }

    [HttpPost]
    [Route("api/room/{roomId}/events")]
    public async Task<SubterfugeResponse<SubmitGameEventResponse>> SubmitGameEvent(SubmitGameEventRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the room you wish to submit this event to.");

        var adminEventTypes = new[]
        {
            EventDataType.PauseGameEventData,
            EventDataType.GameEndEventData,
            EventDataType.UnpauseGameEventData,
        };
        
        // Determine if the user is trying to submit an admin-only game event:
        EventDataType eventType = request.GameEventData.EventDataType;

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            // They must be in the lobby
            if (!lobby.PlayerIdsInLobby.Contains(dbUserModel.Id))
                return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot submit game events if you are not in the game.");
            
            // If they are in the lobby, ensure their event is not an admin event.
            if(adminEventTypes.Contains(eventType))
                return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Trying to submit administrative event as a player.");
        }
        else
        {
            // If admin, ensure that if they are not a player in the game they cannot send real game events.
            // They must be in the lobby
            if (!lobby.PlayerIdsInLobby.Contains(dbUserModel.Id))
            {
                if (!adminEventTypes.Contains(eventType))
                    return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Admins cannot submit normal events if they are not in the lobby");
            }
        }
        
        // Determine what tick the game is currently at.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));
        if(request.GameEventData.OccursAtTick <= currentTick.GetTick())
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(
                ResponseType.VALIDATION_ERROR, 
                $"Cannot submit an event at tick {request.GameEventData.OccursAtTick} because it is in the past. Current Tick:" + currentTick.GetTick()
            );
        
        // TODO: Determine if any players have left the game or lost the game.
        // Prevent players who have left or lost from submitting events (Or we just let them but prevent it in the core side.
        // Might be easier. Then we just check if the game is over and throw out the game events that are not valid.
        // Either we stop the events here, or stop them on the client.

        // Attempt to parse the event data into the actual event class
        try
        {
            switch (eventType)
            {
                case EventDataType.LaunchEventData:
                    JsonConvert.DeserializeObject<LaunchEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.DrillMineEventData:
                    JsonConvert.DeserializeObject<DrillMineEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.GameEndEventData:
                    JsonConvert.DeserializeObject<GameEndEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.PauseGameEventData:
                    JsonConvert.DeserializeObject<PauseGameEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.ToggleShieldEventData:
                    JsonConvert.DeserializeObject<ToggleShieldEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.UnpauseGameEventData:
                    JsonConvert.DeserializeObject<UnpauseGameEventData>(request.GameEventData.SerializedEventData);
                    break;
                case EventDataType.PlayerLeaveGameEventData:
                    var playerLeaveEvent = JsonConvert.DeserializeObject<PlayerLeaveGameEventData>(request.GameEventData.SerializedEventData);
                    if (!dbUserModel.HasClaim(UserClaim.Administrator) && playerLeaveEvent != null)
                    {
                        // Ensure the player in the leave event is the player making the request.
                        // If a player is trying to force someone else to leave, punish them and make them leave instead...
                        playerLeaveEvent.Player = new SimpleUser()
                        {
                            Id = dbUserModel.Id,
                            Username = dbUserModel.Username
                        };
                    }

                    break;
            }
        }
        catch (JsonSerializationException serializationException)
        {
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(
                ResponseType.VALIDATION_ERROR, 
                $"Event type is {eventType} but the server could not parse the event as such."
            );
        }

        var gameEvent = DbGameEvent.FromGameEventRequest(request, dbUserModel.ToUser(), roomId);
        await _dbGameEvents.Upsert(gameEvent);
        
        // If the game event was an end game event, we should also update the lobby to be marked as closed and the game over.
        // TODO: Give out player exp here, count Np, save player stats, etc. Make a method somewhere for handling game end operations.
        if (eventType == EventDataType.GameEndEventData)
        {
            var expiration = DateTime.Now.AddMonths(2);
            
            lobby.RoomStatus = RoomStatus.Closed;
            lobby.TimeEnded = DateTime.Now;
            
            // Mark all game events to expire in 2 months.
            var lobbyGameEvents = (await _dbGameEvents.Query()
                .Where(it => it.RoomId == lobby.Id)
                .ToListAsync());
            
            // TODO: Make a bulk update method for this... This is expensive AF.
            foreach(var lobbyGameEvent in lobbyGameEvents)
            {
                lobbyGameEvent.ExpiresAt = expiration;
                await _dbGameEvents.Upsert(lobbyGameEvent);
            }

            await _dbGameLobbies.Upsert(lobby);
        }

        return SubterfugeResponse<SubmitGameEventResponse>.OfSuccess(new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameRoomEvent = gameEvent.ToGameEventData(),
        });
    }

    [HttpPut]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<SubterfugeResponse<SubmitGameEventResponse>> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the room you wish to update.");

        var gameEvent = await _dbGameEvents.Query().FirstOrDefaultAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the game event you wish to update in the specified room.");

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot update an event issued by another player.");
        
        // Determine if the event has already passed.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));

        if (gameEvent.OccursAtTick <= currentTick.GetTick())
            return SubterfugeResponse<SubmitGameEventResponse>.OfFailure(ResponseType.VALIDATION_ERROR, "Cannot delete an event that has already happened");
        
        gameEvent.OccursAtTick = request.GameEventData.OccursAtTick;
        gameEvent.GameEventType = request.GameEventData.EventDataType;
        gameEvent.SerializedEventData = request.GameEventData.SerializedEventData;
        await _dbGameEvents.Upsert(gameEvent);
        
        return SubterfugeResponse<SubmitGameEventResponse>.OfSuccess(new SubmitGameEventResponse()
        {
            EventId = gameEvent.Id,
            GameRoomEvent = gameEvent.ToGameEventData(),
        });
    }

    [HttpDelete]
    [Route("api/room/{roomId}/events/{eventGuid}")]
    public async Task<SubterfugeResponse<DeleteGameEventResponse>> DeleteGameEvent(string roomId, string eventGuid)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<DeleteGameEventResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<DeleteGameEventResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the room you wish to delete.");
        
        var gameEvent = await _dbGameEvents.Query().FirstOrDefaultAsync(it => it.Id == eventGuid && it.RoomId == roomId);
        if (gameEvent == null)
            return SubterfugeResponse<DeleteGameEventResponse>.OfFailure(ResponseType.NOT_FOUND, "Cannot find the game event you wish to delete.");
        
        // Determine if the event has already passed.
        GameTick currentTick = GameTick.fromGameConfiguration(await lobby.ToGameConfiguration(_dbUserCollection));

        if (gameEvent.OccursAtTick <= currentTick.GetTick())
            return SubterfugeResponse<DeleteGameEventResponse>.OfFailure(ResponseType.VALIDATION_ERROR, "Cannot delete an event that has already happened.");

        if (gameEvent.IssuedBy.Id != dbUserModel.Id)
            return SubterfugeResponse<DeleteGameEventResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot delete an event that you did not create.");

        await _dbGameEvents.Delete(gameEvent);

        return SubterfugeResponse<DeleteGameEventResponse>.OfSuccess(new DeleteGameEventResponse());
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/room/{roomId}/")]
public class MessageGroupController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbGameLobbies;
    private IDatabaseCollection<DbUserModel> _dbUserCollection;
    private IDatabaseCollection<DbMessageGroup> _dbMessageGroups;
    private IDatabaseCollection<DbChatMessage> _dbChatMessages;
    
    public MessageGroupController(IConfiguration configuration, ILogger<UserController> logger, IDatabaseCollectionProvider mongo)
    {
        _config = configuration;
        _logger = logger;
        this._dbGameLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
        this._dbMessageGroups = mongo.GetCollection<DbMessageGroup>();
        this._dbChatMessages = mongo.GetCollection<DbChatMessage>();
        this._dbUserCollection = mongo.GetCollection<DbUserModel>();
    }

    [HttpPost]
    [Route("group/create")]
    public async Task<ActionResult<CreateMessageGroupResponse>> CreateMessageGroup(string roomId, CreateMessageGroupRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));

        if (lobby.RoomStatus != RoomStatus.Ongoing)
            return Forbid();

        if (!request.UserIdsInGroup.Contains(dbUserModel.Id))
            return BadRequest(ResponseFactory.createResponse(ResponseType.INVALID_REQUEST, "Cannot create a group without yourself in it"));
        
        // If, for any player in the group request, they are not a player in the lobby, the request is invalid.
        if (request.UserIdsInGroup.Any(userId => lobby.PlayersInLobby.All(user => user.Id != userId)))
            return BadRequest(ResponseFactory.createResponse(ResponseType.INVALID_REQUEST, "Cannot create a group without yourself in it"));
        
        // Check if there are existing groups that contain the same users.
        var existingGroups = await _dbMessageGroups.Query()
            .Where(it => it.RoomId == roomId)
            .Where(it => it.MembersInGroup.Count == request.UserIdsInGroup.Count)
            .Where(it => it.MembersInGroup.All(userId => request.UserIdsInGroup.Contains(userId.Id)))
            .ToListAsync();
        
        if(existingGroups.Count > 0)
            return BadRequest(ResponseFactory.createResponse(ResponseType.DUPLICATE, $"A chat group with the same members already exists in this room: {existingGroups[0].Id}"));

        var userIdsAsUsers = (await _dbUserCollection.Query()
            .Where(user => request.UserIdsInGroup.Contains(user.Id))
            .ToListAsync())
            .Select(it => it.ToUser())
            .ToList();
        
        if (userIdsAsUsers.Count != request.UserIdsInGroup.Count)
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST, "One of the users in the group is not a valid user id"));

        var newGroup = DbMessageGroup.CreateGroup(roomId, userIdsAsUsers);
        await _dbMessageGroups.Upsert(newGroup);
        
        return Ok(new CreateMessageGroupResponse()
        {
            GroupId = newGroup.Id,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }

    [HttpGet]
    [Route("groups")]
    public async Task<ActionResult<GetMessageGroupsResponse>> GetMessageGroups(string roomId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));
        
        var query = _dbMessageGroups.Query()
            .Where(group => group.RoomId == roomId);
        
        if (!currentUser.HasClaim(UserClaim.Administrator))
        {
            query = query.Where(group => group.MembersInGroup.Any(member => member.Id == currentUser.Id));
        }

        var groupChats = (await query.ToListAsync())
            .Select(it => it.ToMessageGroup())
            .ToList();

        return Ok(new GetMessageGroupsResponse()
        {
            MessageGroups = groupChats,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
    
    [HttpPost]
    [Route("group/{groupId}/send")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));

        var groupChat = await _dbMessageGroups.Query().FirstAsync(it => it.Id == groupId);
        if (groupChat == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST, "The specified group does not exist."));

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) || !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();

        await _dbChatMessages.Upsert(new DbChatMessage()
        {
            RoomId = roomId,
            GroupId = groupId,
            SentBy = currentUser.ToUser(),
            Message = request.Message,
        });

        return Ok(new SendMessageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
    
    [HttpGet]
    [Route("group/{groupId}/messages")]
    public async Task<ActionResult<GetGroupMessagesResponse>> Messages(
        string roomId,
        string groupId,
        GetGroupMessagesRequest request)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        var lobby = await _dbGameLobbies.Query().FirstAsync(it => it.Id == roomId);
        if (lobby == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST, "The specified room does not exist."));
            
        var groupChat = await _dbMessageGroups.Query().FirstAsync(it => it.Id == groupId);
        if (groupChat == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST, "The specified group does not exist."));

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) || !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();

        var messages = (await _dbChatMessages.Query()
            .Where(message => message.RoomId == roomId)
            .Where(message => message.GroupId == groupId)
            .OrderByDescending(message => message.CreatedAt)
            .Skip((request.Pagination) - 1 * 50)
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToChatMessage())
            .ToList();

        return new GetGroupMessagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Messages = messages,
        };
    }
}
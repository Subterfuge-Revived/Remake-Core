using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/room/{roomId}/")]
public class MessageSubterfugeGroupController : ControllerBase, ISubterfugeGroupChatApi
{
    private IDatabaseCollection<DbGameLobbyConfiguration> _dbGameLobbies;
    private IDatabaseCollection<DbUserModel> _dbUserCollection;
    private IDatabaseCollection<DbMessageGroup> _dbMessageGroups;
    private IDatabaseCollection<DbChatMessage> _dbChatMessages;
    
    public MessageSubterfugeGroupController(IDatabaseCollectionProvider mongo)
    {
        this._dbGameLobbies = mongo.GetCollection<DbGameLobbyConfiguration>();
        this._dbMessageGroups = mongo.GetCollection<DbMessageGroup>();
        this._dbChatMessages = mongo.GetCollection<DbChatMessage>();
        this._dbUserCollection = mongo.GetCollection<DbUserModel>();
    }

    [HttpPost]
    [Route("group/create")]
    public async Task<SubterfugeResponse<CreateMessageGroupResponse>> CreateMessageGroup(CreateMessageGroupRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified room does not exist.");

        if (lobby.RoomStatus != RoomStatus.Ongoing)
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot start group chats in a game that has ended.");

        if (!request.UserIdsInGroup.Contains(dbUserModel.Id))
            request.UserIdsInGroup.Add(dbUserModel.Id);
        
        // If, for any player in the group request, they are not a player in the lobby, the request is invalid.
        if (request.UserIdsInGroup.Any(userId => lobby.PlayerIdsInLobby.All(ids => ids != userId)))
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.VALIDATION_ERROR, "Cannot create a group with a player that is not in the game");
        
        // Check if there are existing groups that contain the same users.
        var existingGroups = await _dbMessageGroups.Query()
            .Where(it => it.RoomId == roomId)
            .Where(it => it.MembersInGroup.Count == request.UserIdsInGroup.Count)
            
            
            .Where(it => it.MembersInGroup.Any(it => it.Id == dbUserModel.Id))
            .ToListAsync();
        
        if(existingGroups.Count > 0)
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.DUPLICATE, $"A chat group with the same members already exists in this room: {existingGroups[0].Id}");

        var userIdsAsUsers = (await _dbUserCollection.Query()
            .Where(user => request.UserIdsInGroup.Contains(user.Id))
            .ToListAsync())
            .Select(it => it.ToUser())
            .ToList();
        
        if (userIdsAsUsers.Count != request.UserIdsInGroup.Count)
            return SubterfugeResponse<CreateMessageGroupResponse>.OfFailure(ResponseType.NOT_FOUND, "One of the users in the group is not a valid user id");

        var newGroup = DbMessageGroup.CreateGroup(roomId, userIdsAsUsers);
        await _dbMessageGroups.Upsert(newGroup);
        
        return SubterfugeResponse<CreateMessageGroupResponse>.OfSuccess(new CreateMessageGroupResponse()
        {
            GroupId = newGroup.Id,
        });
    }

    [HttpPost]
    [Route("group/{groupId}/send")]
    public async Task<SubterfugeResponse<SendMessageResponse>> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<SendMessageResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<SendMessageResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified room does not exist.");

        var groupChat = await _dbMessageGroups.Query().FirstOrDefaultAsync(it => it.Id == groupId);
        if (groupChat == null)
            return SubterfugeResponse<SendMessageResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified group does not exist.");

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) && !currentUser.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<SendMessageResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "You are not a member of this group.");

        await _dbChatMessages.Upsert(new DbChatMessage()
        {
            RoomId = roomId,
            GroupId = groupId,
            SentBy = currentUser.ToUser(),
            Message = request.Message,
        });

        return SubterfugeResponse<SendMessageResponse>.OfSuccess(new SendMessageResponse());
    }

    [HttpGet]
    [Route("groups")]
    public async Task<SubterfugeResponse<GetMessageGroupsResponse>> GetMessageGroups(string roomId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<GetMessageGroupsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<GetMessageGroupsResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified room does not exist.");

        var query = _dbMessageGroups.Query()
            .Where(group => group.RoomId == roomId);
        
        if (!currentUser.HasClaim(UserClaim.Administrator))
        {
            query = query.Where(group => group.MembersInGroup.Any(member => member.Id == currentUser.Id));
        }

        var groupChats = (await query.ToListAsync())
            .Select(async group => await group.ToMessageGroup(_dbChatMessages))
            .ToArray();

        return SubterfugeResponse<GetMessageGroupsResponse>.OfSuccess(new GetMessageGroupsResponse()
        {
            MessageGroups = Task.WhenAll(groupChats).Result.ToList(),
        });
    }

    [HttpGet]
    [Route("group/{groupId}/messages")]
    public async Task<SubterfugeResponse<GetGroupMessagesResponse>> GetMessages(
        [FromQuery] GetGroupMessagesRequest request,
        string roomId,
        string groupId
    ) {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<GetGroupMessagesResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            return SubterfugeResponse<GetGroupMessagesResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified room does not exist.");
            
        var groupChat = await _dbMessageGroups.Query().FirstOrDefaultAsync(it => it.Id == groupId);
        if (groupChat == null)
            return SubterfugeResponse<GetGroupMessagesResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified group does not exist within this room.");

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) && !currentUser.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<GetGroupMessagesResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot get messages from a group you are not in");

        var messages = (await _dbChatMessages.Query()
                .Where(message => message.RoomId == roomId)
                .Where(message => message.GroupId == groupId)
                .OrderByDescending(message => message.SentAt)
                .Skip((request.Pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToChatMessage())
            .ToList();

        return SubterfugeResponse<GetGroupMessagesResponse>.OfSuccess(new GetGroupMessagesResponse()
        {
            Messages = messages,
        });
    }
}
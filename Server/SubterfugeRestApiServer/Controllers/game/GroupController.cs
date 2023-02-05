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
    public async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, string roomId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("The specified room does not exist.");

        if (lobby.RoomStatus != RoomStatus.Ongoing)
            throw new ForbidException();

        if (!request.UserIdsInGroup.Contains(dbUserModel.Id))
            throw new BadRequestException("Cannot create a group without yourself in it");
        
        // If, for any player in the group request, they are not a player in the lobby, the request is invalid.
        if (request.UserIdsInGroup.Any(userId => lobby.PlayerIdsInLobby.All(ids => ids != userId)))
            throw new BadRequestException("Cannot create a group with a player that is not in the game");
        
        // Check if there are existing groups that contain the same users.
        var existingGroups = await _dbMessageGroups.Query()
            .Where(it => it.RoomId == roomId)
            .Where(it => it.MembersInGroup.Count == request.UserIdsInGroup.Count)
            
            
            .Where(it => it.MembersInGroup.Any(it => it.Id == dbUserModel.Id))
            .ToListAsync();
        
        if(existingGroups.Count > 0)
            throw new BadRequestException($"A chat group with the same members already exists in this room: {existingGroups[0].Id}");

        var userIdsAsUsers = (await _dbUserCollection.Query()
            .Where(user => request.UserIdsInGroup.Contains(user.Id))
            .ToListAsync())
            .Select(it => it.ToUser())
            .ToList();
        
        if (userIdsAsUsers.Count != request.UserIdsInGroup.Count)
            throw new NotFoundException("One of the users in the group is not a valid user id");

        var newGroup = DbMessageGroup.CreateGroup(roomId, userIdsAsUsers);
        await _dbMessageGroups.Upsert(newGroup);
        
        return new CreateMessageGroupResponse()
        {
            GroupId = newGroup.Id,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpPost]
    [Route("group/{groupId}/send")]
    public async Task<SendMessageResponse> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();

        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("The specified room does not exist.");

        var groupChat = await _dbMessageGroups.Query().FirstOrDefaultAsync(it => it.Id == groupId);
        if (groupChat == null)
            throw new NotFoundException("The specified group does not exist.");

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) && !currentUser.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        await _dbChatMessages.Upsert(new DbChatMessage()
        {
            RoomId = roomId,
            GroupId = groupId,
            SentBy = currentUser.ToUser(),
            Message = request.Message,
        });

        return new SendMessageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }

    [HttpGet]
    [Route("groups")]
    public async Task<GetMessageGroupsResponse> GetMessageGroups(string roomId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("The specified room does not exist.");
        
        var query = _dbMessageGroups.Query()
            .Where(group => group.RoomId == roomId);
        
        if (!currentUser.HasClaim(UserClaim.Administrator))
        {
            query = query.Where(group => group.MembersInGroup.Any(member => member.Id == currentUser.Id));
        }

        var groupChats = (await query.ToListAsync())
            .Select(async group => await group.ToMessageGroup(_dbChatMessages))
            .ToArray();

        return new GetMessageGroupsResponse()
        {
            MessageGroups = Task.WhenAll(groupChats).Result.ToList(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }

    [HttpGet]
    [Route("group/{groupId}/messages")]
    public async Task<GetGroupMessagesResponse> GetMessages(
        [FromQuery] GetGroupMessagesRequest request,
        string roomId,
        string groupId
    ) {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();
            
        var lobby = await _dbGameLobbies.Query().FirstOrDefaultAsync(it => it.Id == roomId);
        if (lobby == null)
            throw new NotFoundException("The specified room does not exist.");
            
        var groupChat = await _dbMessageGroups.Query().FirstOrDefaultAsync(it => it.Id == groupId);
        if (groupChat == null)
            throw new NotFoundException("The specified group does not exist within this room.");

        if (groupChat.MembersInGroup.All(member => member.Id != currentUser.Id) && !currentUser.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        var messages = (await _dbChatMessages.Query()
                .Where(message => message.RoomId == roomId)
                .Where(message => message.GroupId == groupId)
                .OrderByDescending(message => message.SentAt)
                .Skip((request.Pagination - 1) * 50)
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
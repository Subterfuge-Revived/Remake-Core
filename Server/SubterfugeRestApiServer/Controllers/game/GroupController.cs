using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/room/{roomId}/")]
public class MessageGroupController : ControllerBase
{
    public MessageGroupController(IConfiguration configuration, ILogger<UserController> logger)
    {
        _config = configuration;
        _logger = logger;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    [HttpPost]
    [Route("group/create")]
    public async Task<ActionResult<CreateMessageGroupResponse>> CreateMessageGroup(string roomId, CreateMessageGroupRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound();

        if (room.GameConfiguration.RoomStatus != RoomStatus.Ongoing)
            return Forbid();

        if (!request.UserIdsInGroup.Contains(dbUserModel.UserModel.Id))
            return BadRequest("Cannot create a message group without yourself in it");

        return Ok(await room.CreateMessageGroup(request.UserIdsInGroup.ToList()));
    }

    [HttpGet]
    [Route("groups")]
    public async Task<ActionResult<GetMessageGroupsResponse>> GetMessageGroups(string roomId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound();
        
        // TODO: Add administrator ability here.

        List<GroupChat> groupChats = await room.GetPlayerGroupChats(currentUser);
        GetMessageGroupsResponse response = new GetMessageGroupsResponse();
        foreach (var groupModel in groupChats)
        {
            response.MessageGroups.Add(await groupModel.asMessageGroup());
        }

        response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
        return Ok(response);
    }
    
    [HttpPost]
    [Route("group/{groupId}/send")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        Room? room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound("Room not found");

        GroupChat? groupChat = await room.GetGroupChatById(groupId);
        if (groupChat == null)
            return NotFound("Group not found");

        if (!groupChat.IsPlayerInGroup(currentUser) || !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();

        return Ok(new SendMessageResponse()
        {
            Status = await groupChat.SendChatMessage(currentUser, request.Message)
        });
    }
    
    [HttpGet]
    [Route("group/{groupId}/messages")]
    public async Task<ActionResult<GetGroupMessagesResponse>> Messages(string roomId, string groupId, GetGroupMessagesRequest request)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(roomId);
        if (room == null)
            return NotFound("Room not found");
            
        GroupChat groupChat = await room.GetGroupChatById(groupId);
        if (groupChat == null)
            return NotFound("Group not found");

        if (!groupChat.IsPlayerInGroup(currentUser) || !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();

        return new GetGroupMessagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Group = await groupChat.asMessageGroup(request.Pagination)
        };
    }
}
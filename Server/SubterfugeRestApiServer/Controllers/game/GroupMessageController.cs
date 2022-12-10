using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/room/{roomId}/group/{groupId}/[action]")]
public class GroupMessageController : ControllerBase
{
    public GroupMessageController(IConfiguration configuration, ILogger<UserController> logger, string roomId, string groupId)
    {
        _config = configuration;
        _logger = logger;
        _roomGuid = roomId;
        _groupId = groupId;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly string _roomGuid;
    private readonly string _groupId;
    
    [HttpPost]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(SendMessageRequest request, string groupId)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        Room? room = await Room.GetRoomFromGuid(_roomGuid);
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
    public async Task<ActionResult<GetGroupMessagesResponse>> Messages(GetGroupMessagesRequest request)
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if (room == null)
            return NotFound("Room not found");
            
        GroupChat groupChat = await room.GetGroupChatById(_groupId);
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
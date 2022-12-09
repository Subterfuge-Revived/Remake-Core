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
    public async Task<SendMessageResponse> SendMessage(SendMessageRequest request, string groupId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new SendMessageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if(room == null)
            return new SendMessageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        GroupChat groupChat = await room.GetGroupChatById(groupId);
        if(groupChat == null)
            return new SendMessageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
            };
            
        if(!groupChat.IsPlayerInGroup(dbUserModel))
            return new SendMessageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
            };

        return new SendMessageResponse()
        {
            Status = await groupChat.SendChatMessage(dbUserModel, request.Message)
        };
    }
    
    [HttpGet]
    public async Task<GetGroupMessagesResponse> Messages(GetGroupMessagesRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new GetGroupMessagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if(room == null)
            return new GetGroupMessagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        GroupChat groupChat = await room.GetGroupChatById(_groupId);
        if(groupChat == null)
            return new GetGroupMessagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
            };
            
        if(!groupChat.IsPlayerInGroup(dbUserModel))
            return new GetGroupMessagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
            };

        return new GetGroupMessagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Group = await groupChat.asMessageGroup(request.Pagination)
        };
    }
}
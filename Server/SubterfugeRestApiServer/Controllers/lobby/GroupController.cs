using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/{roomId}/[controller]/[action]")]
public class MessageGroupController : ControllerBase
{
    public MessageGroupController(IConfiguration configuration, ILogger<AccountController> logger, string roomId)
    {
        _config = configuration;
        _logger = logger;
        _roomGuid = roomId;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly string _roomGuid;
    
    [HttpPost]
    public async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new CreateMessageGroupResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if(room == null)
            return new CreateMessageGroupResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };
            
        if(room.GameConfiguration.RoomStatus != RoomStatus.Ongoing)
            return new CreateMessageGroupResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
            };
            
        if(!request.UserIdsInGroup.Contains(dbUserModel.UserModel.Id))
            return new CreateMessageGroupResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
            };

        return await room.CreateMessageGroup(request.UserIdsInGroup.ToList());
    }

    [HttpGet]
    public async Task<GetMessageGroupsResponse> GetMessageGroups()
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new GetMessageGroupsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if(room == null)
            return new GetMessageGroupsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
            };

        List<GroupChat> groupChats = await room.GetPlayerGroupChats(dbUserModel);
        GetMessageGroupsResponse response = new GetMessageGroupsResponse();
        foreach (var groupModel in groupChats)
        {
            response.MessageGroups.Add(await groupModel.asMessageGroup());
        }

        response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
        return response;
    }
}
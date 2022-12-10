using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/room/{roomId}/group/[action]")]
public class MessageGroupController : ControllerBase
{
    public MessageGroupController(IConfiguration configuration, ILogger<UserController> logger, string roomId)
    {
        _config = configuration;
        _logger = logger;
        _roomGuid = roomId;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly string _roomGuid;
    
    [HttpPost]
    public async Task<ActionResult<CreateMessageGroupResponse>> CreateMessageGroup(CreateMessageGroupRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
        if (room == null)
            return NotFound();

        if (room.GameConfiguration.RoomStatus != RoomStatus.Ongoing)
            return Forbid();

        if (!request.UserIdsInGroup.Contains(dbUserModel.UserModel.Id))
            return BadRequest("Cannot create a message group without yourself in it");

        return Ok(await room.CreateMessageGroup(request.UserIdsInGroup.ToList()));
    }

    [HttpGet]
    public async Task<ActionResult<GetMessageGroupsResponse>> GetMessageGroups()
    {
        DbUserModel? currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
            
        Room room = await Room.GetRoomFromGuid(_roomGuid);
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
}
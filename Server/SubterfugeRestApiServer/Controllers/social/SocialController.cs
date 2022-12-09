using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
public class SocialController : ControllerBase
{
    [HttpPost]
    [Route("api/user/{userId}/block")]
    public async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null) 
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
            };
            
        if(await dbUserModel.IsBlocked(friend))
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
            };

        await dbUserModel.BlockUser(friend);
        return new BlockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpPost]
    [Route("api/user/{userId}/unblock")]
    public async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        // Check if player is valid.
        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null) 
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
            };
            
        // Check if player is blocked.
        if(!await dbUserModel.IsBlocked(friend))
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
            };

        await dbUserModel.UnblockUser(friend);
        return new UnblockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpPost]
    [Route("api/user/{userId}/addFriend")]
    public async Task<ActionResult<SendFriendRequestResponse>> AddAcceptFriend(string userId)
    {
        // Send a friend request OR accept an existing friend request.
        throw new NotImplementedException();
    }
    
    [HttpPost]
    [Route("api/user/{userId}/removeFriend")]
    public async Task<ActionResult<SendFriendRequestResponse>> RemoveRejectFriend(string userId)
    {
        // Remove an existing friend or remove a friend request
        throw new NotImplementedException();
    }
    
    [HttpGet]
    [Route("api/user/{userId}/friends")]
    public async Task<ActionResult<SendFriendRequestResponse>> GetFriendList(string userId)
    {
        // Get a player's friend list
        throw new NotImplementedException();
    }
    
}
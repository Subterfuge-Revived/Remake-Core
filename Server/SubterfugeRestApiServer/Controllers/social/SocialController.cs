using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/user/{userId}")]
public class SocialController : ControllerBase
{
    [HttpGet]
    [Route("blocks")]
    public async Task<ActionResult<BlockPlayerResponse>> ViewBlockedPlayers(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        if (userId == currentUser.UserModel.Id)
        {
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();

            var blockedUsers = await Task.WhenAll(
                (await currentUser.GetBlockedUsers())
                .Select(
                    async it => (
                        await DbUserModel.GetUserFromGuid(it.FriendId)
                    ).AsUser()
                )
            );
                
            
            response.BlockedUsers.AddRange(blockedUsers);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        } 
        
        // If admin, show the requested user's friends:
        if (currentUser.HasClaim(UserClaim.Administrator))
        {
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();
            DbUserModel? playerToQuery = await DbUserModel.GetUserFromGuid(userId);

            if (playerToQuery == null)
                return NotFound();

            var blockedUsers = await Task.WhenAll(
                (await playerToQuery.GetBlockedUsers())
                .Select(
                    async it => (
                        await DbUserModel.GetUserFromGuid(it.FriendId)
                    ).AsUser()
                )
            );
                
            
            response.BlockedUsers.AddRange(blockedUsers);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        }

        // Unauthorized if normal player is trying to see someone else's friends.
        return Unauthorized();
    }
    
    [HttpGet]
    [Route("friendRequests")]
    public async Task<ActionResult<ViewFriendRequestsResponse>> ViewFriendRequests(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        if (userId == currentUser.UserModel.Id)
        {
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            var friendRequests = await Task.WhenAll(
                (await currentUser.GetFriendRequests())
                .Select(async it =>
                {
                    return it.PlayerId == userId ? (await DbUserModel.GetUserFromGuid(it.FriendId)).AsUser() : (await DbUserModel.GetUserFromGuid(it.PlayerId)).AsUser();
                })
            );
            
            response.FriendRequests.AddRange(friendRequests);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        } 
        
        // If admin, show the requested user's friends:
        if (currentUser.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? playerToQuery = await DbUserModel.GetUserFromGuid(userId);

            if (playerToQuery == null)
                return NotFound();

            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            var friendRequests = await Task.WhenAll(
                (await playerToQuery.GetFriendRequests())
                .Select(async it =>
                {
                    return it.PlayerId == userId ? (await DbUserModel.GetUserFromGuid(it.FriendId)).AsUser() : (await DbUserModel.GetUserFromGuid(it.PlayerId)).AsUser();
                })
            );
            
            response.FriendRequests.AddRange(friendRequests);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        }

        // Unauthorized if normal player is trying to see someone else's friends.
        return Unauthorized();
    }
    
    [HttpPost]
    [Route("block")]
    public async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED, "I don't know how you got this message but you did. You need to be logged in to get here so... how?")
            };

        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null) 
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST, "Wow, you really hate that guy. Too bad they doesn't exist.")
            };
            
        if(await dbUserModel.IsRelationshipBlocked(friend))
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.DUPLICATE, "We understand. You really don't like this person. But you already blocked them.")
            };

        await dbUserModel.BlockUser(friend);
        return new BlockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpPost]
    [Route("unblock")]
    public async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED, "Please login.")
            };
            
        // Check if player is valid.
        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null) 
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST, "You are attempting to talk to a ghost. Are you a medium?")
            };
            
        // Check if player is blocked.
        if(!await dbUserModel.IsRelationshipBlocked(friend))
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST, "You do not have this player blocked.")
            };

        await dbUserModel.UnblockUser(friend);
        return new UnblockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpGet]
    [Route("addFriend")]
    public async Task<ActionResult<AddAcceptFriendResponse>> AddAcceptFriend(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if(currentUser == null)
            return Unauthorized();

        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null)
            return NotFound();

        if (await friend.IsRelationshipBlocked(currentUser))
            return Forbid();

        if (await currentUser.HasFriendRequestBetween(friend))
        {
            // Accept Friend Request
            await currentUser.AcceptFriendRequestFrom(friend);
            return Ok(new AddAcceptFriendResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            });
        }
        // Add a friend request on the other player from the current player.
        return Ok(new AddAcceptFriendResponse()
        {
            Status = await friend.AddFriendRequestFrom(currentUser),
        });
    }
    
    [HttpGet]
    [Route("removeFriend")]
    public async Task<ActionResult<DenyFriendRequestResponse>> RemoveRejectFriend(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();

        DbUserModel friend = await DbUserModel.GetUserFromGuid(userId);
        if (friend == null)
            return NotFound();
        
        // Don't care if the players are blocked here. You should always be able to remove a friend.

        if (await currentUser.HasFriendRequestBetween(friend))
        {
            // Remove the friend request.
            return Ok(new DenyFriendRequestResponse()
            {
                Status = await currentUser.RemoveFriendRequestFrom(friend),
            });
        }
        else if (await currentUser.IsFriend(friend))
        {
            // Deny the friend request.
            return Ok(new DenyFriendRequestResponse()
            {
                Status = await currentUser.RemoveFriend(friend),
            });
        }

        return UnprocessableEntity("Users are not friends and no friend request exists");
    }
    
    [HttpGet]
    [Route("friends")]
    public async Task<ActionResult<ViewFriendsResponse>> GetFriendList(string userId)
    {
        // Get a player's friend list
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        // Check if they are requesting to view their own friends.
        if (userId == currentUser.UserModel.Id)
        {
            ViewFriendsResponse response = new ViewFriendsResponse();
            
            var friends = (await Task.WhenAll(
                (await currentUser.GetFriends())
                .Select(async it =>
                    {
                        return it.PlayerId == userId ? (await DbUserModel.GetUserFromGuid(it.FriendId)).AsUser() : (await DbUserModel.GetUserFromGuid(it.PlayerId)).AsUser();
                    }
                )
            )).ToList();
            
            response.Friends.AddRange(friends);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        } 
        
        // If admin, show the requested user's friends:
        if (currentUser.HasClaim(UserClaim.Administrator))
        {
            ViewFriendsResponse response = new ViewFriendsResponse();
            DbUserModel userToQuery = await DbUserModel.GetUserFromGuid(userId);
            
            var friends = (await Task.WhenAll(
                (await userToQuery.GetFriends())
                .Select(async it =>
                    {
                        return it.PlayerId == userId ? (await DbUserModel.GetUserFromGuid(it.FriendId)).AsUser() : (await DbUserModel.GetUserFromGuid(it.PlayerId)).AsUser();
                    }
                )
            )).ToList();
            
            response.Friends.AddRange(friends);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return Ok(response);
        }
        // If a normal user is trying to look at someone else's friends, unauthorized.
        return Unauthorized();
    }
    
}
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
[Route("api/user/{userId}")]
public class SocialController : ControllerBase
{

    private IDatabaseCollection<DbPlayerRelationship> _dbRelations;
    private IDatabaseCollection<DbUserModel> _dbUserCollection;

    public SocialController(IDatabaseCollectionProvider mongo)
    {
        _dbRelations = mongo.GetCollection<DbPlayerRelationship>();
        _dbUserCollection = mongo.GetCollection<DbUserModel>();
    }
    
    
    [HttpGet]
    [Route("blocks")]
    public async Task<ActionResult<ViewBlockedPlayersResponse>> ViewBlockedPlayers(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        var blockedUser = (await _dbRelations.Query()
            .Where(it => it.RelationshipStatus == RelationshipStatus.Blocked)
            .Where(it => it.Player.Id == userId)
            .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();

        return Ok(new ViewBlockedPlayersResponse()
        {
            BlockedUsers = blockedUser,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
    [HttpGet]
    [Route("friendRequests")]
    public async Task<ActionResult<ViewFriendRequestsResponse>> ViewFriendRequests(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        var requests = (await _dbRelations.Query()
            .Where(relation => relation.Player.Id == userId || relation.Friend.Id == userId)
            .Where(relation => relation.RelationshipStatus == RelationshipStatus.Pending)
            .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return Ok(new ViewFriendRequestsResponse()
        {
            FriendRequests = requests,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
    [HttpPost]
    [Route("block")]
    public async Task<ActionResult<BlockPlayerResponse>> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var playerToBlock = await _dbUserCollection.Query().FirstAsync(it => it.Id == userId);
        if (playerToBlock == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST));
        
        var existingRelationship = await _dbRelations.Query()
            .FirstAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));

        // We can block.
        if (existingRelationship == null)
        {
            var relationship = new DbPlayerRelationship()
            {
                Player = dbUserModel.ToUser(),
                Friend = playerToBlock.ToUser(),
                RelationshipStatus = RelationshipStatus.Blocked
            };
            await _dbRelations.Upsert(relationship);
        }
        else
        {
            existingRelationship.RelationshipStatus = RelationshipStatus.Blocked;
            await _dbRelations.Upsert(existingRelationship);
        }
        
        return new BlockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpPost]
    [Route("unblock")]
    public async Task<ActionResult<UnblockPlayerResponse>> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var playerToUnblock = await _dbUserCollection.Query().FirstAsync(it => it.Id == userId);
        if (playerToUnblock == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST));
        
        var existingRelationship = await _dbRelations.Query()
            .FirstAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));
        
        if (existingRelationship == null)
        {
            var relationship = new DbPlayerRelationship()
            {
                Player = dbUserModel.ToUser(),
                Friend = playerToUnblock.ToUser(),
                RelationshipStatus = RelationshipStatus.NoRelation
            };
            await _dbRelations.Upsert(relationship);
        }
        else
        {
            if (existingRelationship.RelationshipStatus != RelationshipStatus.Blocked)
                return Forbid();
            
            // Cannot unblock someone if you were not the one to block them.
            if (existingRelationship.Player.Id != dbUserModel.Id)
                return Forbid();
            
            existingRelationship.RelationshipStatus = RelationshipStatus.NoRelation;
            await _dbRelations.Upsert(existingRelationship);
        }
        
        return new UnblockPlayerResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }

    [HttpGet]
    [Route("addFriend")]
    public async Task<ActionResult<AddAcceptFriendResponse>> AddAcceptFriend(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var playerToBefriend = await _dbUserCollection.Query().FirstAsync(it => it.Id == userId);
        if (playerToBefriend == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST));
        
        var existingRelationship = await _dbRelations.Query()
            .FirstAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));
        
        if (existingRelationship == null)
        {
            var relationship = new DbPlayerRelationship()
            {
                Player = dbUserModel.ToUser(),
                Friend = playerToBefriend.ToUser(),
                RelationshipStatus = RelationshipStatus.Pending
            };
            await _dbRelations.Upsert(relationship);
            return new AddAcceptFriendResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }
        
        switch (existingRelationship.RelationshipStatus)
        {
            case RelationshipStatus.Blocked:
                return Forbid();
            case RelationshipStatus.Friends:
                return Conflict(ResponseFactory.createResponse(ResponseType.DUPLICATE));
            case RelationshipStatus.Pending:
                // If you did not create the request, set the relationship to be friends
                if (existingRelationship.Player.Id != dbUserModel.Id)
                {
                    existingRelationship.RelationshipStatus = RelationshipStatus.Friends;
                    await _dbRelations.Upsert(existingRelationship);
                    return new AddAcceptFriendResponse()
                    {
                        Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
                    };
                }

                return Forbid();
            case RelationshipStatus.NoRelation:
                existingRelationship.RelationshipStatus = RelationshipStatus.Friends;
                // Swap the 'primary' friend to indicate who sent the request.
                existingRelationship.Player = dbUserModel.ToUser();
                existingRelationship.Friend = playerToBefriend.ToUser();
                await _dbRelations.Upsert(existingRelationship);
                return new AddAcceptFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
                };
            
            // Should never happen.
            default:
                return Forbid();
        }
    }
    
    [HttpGet]
    [Route("removeFriend")]
    public async Task<ActionResult<DenyFriendRequestResponse>> RemoveRejectFriend(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return Unauthorized();

        var playerToReject = await _dbUserCollection.Query().FirstAsync(it => it.Id == userId);
        if (playerToReject == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST));
        
        var existingRelationship = await _dbRelations.Query()
            .FirstAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));

        if (existingRelationship == null)
            return NotFound(ResponseFactory.createResponse(ResponseType.FRIEND_REQUEST_DOES_NOT_EXIST,
                "Cannot remove a friend request that does not exist."));

        if (existingRelationship.RelationshipStatus is not (RelationshipStatus.Pending or RelationshipStatus.Friends))
            return Forbid();

        existingRelationship.RelationshipStatus = RelationshipStatus.NoRelation;
        await _dbRelations.Upsert(existingRelationship);
        
        return new DenyFriendRequestResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpGet]
    [Route("friends")]
    public async Task<ActionResult<ViewFriendsResponse>> GetFriendList(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return Unauthorized();
        
        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        var requests = (await _dbRelations.Query()
            .Where(relation => relation.Player.Id == userId || relation.Friend.Id == userId)
            .Where(relation => relation.RelationshipStatus == RelationshipStatus.Friends)
            .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return Ok(new ViewFriendRequestsResponse()
        {
            FriendRequests = requests,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        });
    }
    
}
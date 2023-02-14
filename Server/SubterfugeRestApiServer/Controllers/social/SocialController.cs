using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/user/{userId}")]
public class SocialController : ControllerBase, ISubterfugeSocialApi
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
    public async Task<SubterfugeResponse<ViewBlockedPlayersResponse>> ViewBlockedPlayers(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<ViewBlockedPlayersResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<ViewBlockedPlayersResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Can only view your own blocked players.");
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            return SubterfugeResponse<ViewBlockedPlayersResponse>.OfFailure(ResponseType.NOT_FOUND, "The target player was not found.");
        
        var blockedUser = (await _dbRelations.Query()
                .Where(it => it.RelationshipStatus == RelationshipStatus.Blocked)
                .Where(it => it.Player.Id == userId)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();

        return SubterfugeResponse<ViewBlockedPlayersResponse>.OfSuccess(new ViewBlockedPlayersResponse()
        {
            BlockedUsers = blockedUser,
        });
    }
    
    [HttpGet]
    [Route("friendRequests")]
    public async Task<SubterfugeResponse<ViewFriendRequestsResponse>> ViewFriendRequests(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<ViewFriendRequestsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<ViewFriendRequestsResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Can only view your own friends.");
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            return SubterfugeResponse<ViewFriendRequestsResponse>.OfFailure(ResponseType.NOT_FOUND, "The target player was not found.");
        
        // For friend requests, if you didn't start the relation, you will be the friend ID
        var requests = (await _dbRelations.Query()
                .Where(relation => relation.Friend.Id == userId)
                .Where(relation => relation.RelationshipStatus == RelationshipStatus.Pending)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return SubterfugeResponse<ViewFriendRequestsResponse>.OfSuccess(new ViewFriendRequestsResponse()
        {
            FriendRequests = requests,
        });
    }
    
    [HttpPost]
    [Route("block")]
    public async Task<SubterfugeResponse<BlockPlayerResponse>> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<BlockPlayerResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var playerToBlock = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToBlock == null)
            return SubterfugeResponse<BlockPlayerResponse>.OfFailure(ResponseType.NOT_FOUND, "The target player was not found.");

        if (playerToBlock.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<BlockPlayerResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot block an administrator.");
        
        var existingRelationship = await _dbRelations.Query()
            .FirstOrDefaultAsync(relation => 
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

        return SubterfugeResponse<BlockPlayerResponse>.OfSuccess(new BlockPlayerResponse());
    }
    
    [HttpPost]
    [Route("unblock")]
    public async Task<SubterfugeResponse<UnblockPlayerResponse>> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<UnblockPlayerResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var playerToUnblock = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToUnblock == null)
            return SubterfugeResponse<UnblockPlayerResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified player does not exist.");
        
        var existingRelationship = await _dbRelations.Query()
            .FirstOrDefaultAsync(relation => 
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
                return SubterfugeResponse<UnblockPlayerResponse>.OfFailure(ResponseType.INVALID_REQUEST, "The player you wish to unblock is not currently blocked.");
            
            // Cannot unblock someone if you were not the one to block them.
            if (existingRelationship.Player.Id != dbUserModel.Id)
                return SubterfugeResponse<UnblockPlayerResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "You cannot unblock this player, as you did not block them.");
            
            existingRelationship.RelationshipStatus = RelationshipStatus.NoRelation;
            await _dbRelations.Upsert(existingRelationship);
        }
        
        return SubterfugeResponse<UnblockPlayerResponse>.OfSuccess(new UnblockPlayerResponse());
    }

    [HttpGet]
    [Route("addFriend")]
    public async Task<SubterfugeResponse<AddAcceptFriendResponse>> AddAcceptFriendRequest(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var playerToBefriend = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToBefriend == null)
            return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified player does not exist.");
        
        var existingRelationship = await _dbRelations.Query()
            .FirstOrDefaultAsync(relation => 
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
            return SubterfugeResponse<AddAcceptFriendResponse>.OfSuccess(new AddAcceptFriendResponse());
        }
        
        switch (existingRelationship.RelationshipStatus)
        {
            case RelationshipStatus.Blocked:
                return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot be friends while blocked.");
            case RelationshipStatus.Friends:
                return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.DUPLICATE, "You are already friends with this player.");
            case RelationshipStatus.Pending:
                // If you did not create the request, set the relationship to be friends
                if (existingRelationship.Player.Id != dbUserModel.Id)
                {
                    existingRelationship.RelationshipStatus = RelationshipStatus.Friends;
                    await _dbRelations.Upsert(existingRelationship);
                    return SubterfugeResponse<AddAcceptFriendResponse>.OfSuccess(new AddAcceptFriendResponse());
                }
                
                // Cannot accept your own request
                return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.DUPLICATE, "You already have a pending friend request to this player.");
            case RelationshipStatus.NoRelation:
                existingRelationship.RelationshipStatus = RelationshipStatus.Friends;
                // Swap the 'primary' friend to indicate who sent the request.
                existingRelationship.Player = dbUserModel.ToUser();
                existingRelationship.Friend = playerToBefriend.ToUser();
                await _dbRelations.Upsert(existingRelationship);
                return SubterfugeResponse<AddAcceptFriendResponse>.OfSuccess(new AddAcceptFriendResponse());
            
            // Should never happen.
            default:
                return SubterfugeResponse<AddAcceptFriendResponse>.OfFailure(ResponseType.INVALID_REQUEST, "I don't know how you did this, but well done; you've played the system.");
        }
    }
    
    [HttpGet]
    [Route("removeFriend")]
    public async Task<SubterfugeResponse<DenyFriendRequestResponse>> RemoveRejectFriend(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<DenyFriendRequestResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var playerToReject = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToReject == null)
            return SubterfugeResponse<DenyFriendRequestResponse>.OfFailure(ResponseType.NOT_FOUND, "The specified player does not exist.");
        
        var existingRelationship = await _dbRelations.Query()
            .FirstOrDefaultAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));

        if (existingRelationship == null)
            return SubterfugeResponse<DenyFriendRequestResponse>.OfFailure(ResponseType.INVALID_REQUEST, "A friend request from that player does not exist");

        if (existingRelationship.RelationshipStatus is not (RelationshipStatus.Pending or RelationshipStatus.Friends))
            return SubterfugeResponse<DenyFriendRequestResponse>.OfFailure(ResponseType.INVALID_REQUEST, "Cannot revoke friendship status for this player.");

        existingRelationship.RelationshipStatus = RelationshipStatus.NoRelation;
        await _dbRelations.Upsert(existingRelationship);
        
        return SubterfugeResponse<DenyFriendRequestResponse>.OfSuccess(new DenyFriendRequestResponse());
    }
    
    [HttpGet]
    [Route("friends")]
    public async Task<SubterfugeResponse<ViewFriendsResponse>> GetFriendList(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            return SubterfugeResponse<ViewFriendsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<ViewFriendsResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Can only view your own friend list.");
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            return SubterfugeResponse<ViewFriendsResponse>.OfFailure(ResponseType.NOT_FOUND, "The target player was not found");
        
        var friends = (await _dbRelations.Query()
                .Where(relation => relation.Player.Id == userId || relation.Friend.Id == userId)
                .Where(relation => relation.RelationshipStatus == RelationshipStatus.Friends)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return SubterfugeResponse<ViewFriendsResponse>.OfSuccess(new ViewFriendsResponse()
        {
            Friends = friends,
        });
    }
}
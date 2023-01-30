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
    public async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            throw new NotFoundException("The target player was not found");
        
        var blockedUser = (await _dbRelations.Query()
                .Where(it => it.RelationshipStatus == RelationshipStatus.Blocked)
                .Where(it => it.Player.Id == userId)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();

        return new ViewBlockedPlayersResponse()
        {
            BlockedUsers = blockedUser,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpGet]
    [Route("friendRequests")]
    public async Task<ViewFriendRequestsResponse> ViewFriendRequests(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            throw new NotFoundException("The target player was not found");
        
        // For friend requests, if you didn't start the relation, you will be the friend ID
        var requests = (await _dbRelations.Query()
                .Where(relation => relation.Friend.Id == userId)
                .Where(relation => relation.RelationshipStatus == RelationshipStatus.Pending)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return new ViewFriendRequestsResponse()
        {
            FriendRequests = requests,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpPost]
    [Route("block")]
    public async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var playerToBlock = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToBlock == null)
            throw new NotFoundException("The specified player does not exist.");

        if (playerToBlock.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
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
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var playerToUnblock = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToUnblock == null)
            throw new NotFoundException("The specified player does not exist.");
        
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
                throw new ForbidException();
            
            // Cannot unblock someone if you were not the one to block them.
            if (existingRelationship.Player.Id != dbUserModel.Id)
                throw new ForbidException();
            
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
    public async Task<AddAcceptFriendResponse> AddAcceptFriendRequest(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var playerToBefriend = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToBefriend == null)
            throw new NotFoundException("The specified player does not exist.");
        
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
            return new AddAcceptFriendResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }
        
        switch (existingRelationship.RelationshipStatus)
        {
            case RelationshipStatus.Blocked:
                throw new ForbidException();
            case RelationshipStatus.Friends:
                throw new ConflictException("You are already friends with this player.");
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
                
                // Cannot accept your own request
                throw new ConflictException("You already have a pending friend request to this player.");
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
                throw new ForbidException();
        }
    }
    
    [HttpGet]
    [Route("removeFriend")]
    public async Task<DenyFriendRequestResponse> RemoveRejectFriend(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        var playerToReject = await _dbUserCollection.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (playerToReject == null)
            throw new NotFoundException("The specified player does not exist");
        
        var existingRelationship = await _dbRelations.Query()
            .FirstOrDefaultAsync(relation => 
                (relation.Player.Id == dbUserModel.Id && relation.Friend.Id == userId) || 
                (relation.Player.Id == userId && relation.Friend.Id == dbUserModel.Id));

        if (existingRelationship == null)
            throw new NotFoundException("A friend request from that player does not exist");

        if (existingRelationship.RelationshipStatus is not (RelationshipStatus.Pending or RelationshipStatus.Friends))
            throw new ForbidException();

        existingRelationship.RelationshipStatus = RelationshipStatus.NoRelation;
        await _dbRelations.Upsert(existingRelationship);
        
        return new DenyFriendRequestResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
    
    [HttpGet]
    [Route("friends")]
    public async Task<ViewFriendsResponse> GetFriendList(string userId)
    {
        DbUserModel currentUser = HttpContext.Items["User"] as DbUserModel;
        if (currentUser == null)
            throw new UnauthorizedException();

        if (userId != currentUser.Id && !currentUser.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        DbUserModel targetUser = await _dbUserCollection.Query()
            .FirstOrDefaultAsync(it => it.Id == userId);
        
        if (targetUser == null)
            throw new NotFoundException("The target player was not found");
        
        var friends = (await _dbRelations.Query()
                .Where(relation => relation.Player.Id == userId || relation.Friend.Id == userId)
                .Where(relation => relation.RelationshipStatus == RelationshipStatus.Friends)
                .ToListAsync())
            .Select(it => it.GetOtherUser(userId))
            .ToList();
        
        return new ViewFriendsResponse()
        {
            Friends = friends,
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
    }
}
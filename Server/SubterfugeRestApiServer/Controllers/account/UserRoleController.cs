using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/user/{userId}/")]
public class SubterfugeUserRoleController : ControllerBase, ISubterfugeUserRoleApi
{
    private IDatabaseCollection<DbUserModel> _dbUsers;
    private IDatabaseCollection<DbIpBan> _dbIpBans;

    public SubterfugeUserRoleController(IDatabaseCollectionProvider mongo)
    {
        _dbUsers = mongo.GetCollection<DbUserModel>();
        _dbIpBans = mongo.GetCollection<DbIpBan>();
    }

    [Authorize]
    [HttpGet]
    [Route("roles")]
    public async Task<SubterfugeResponse<GetRolesResponse>> GetRoles(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (dbUserModel.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == userId);
            if(targetUser == null)
                return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.NOT_FOUND, "Off the grid? We have no record of this user.");
            
            return SubterfugeResponse<GetRolesResponse>.OfSuccess(new GetRolesResponse()
            {
                Claims = targetUser.Claims
            });
        }
        
        if (dbUserModel.Id == userId)
        {
            return SubterfugeResponse<GetRolesResponse>.OfSuccess(new GetRolesResponse()
            {
                Claims = dbUserModel.Claims
            });
        }

        return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Cannot get the roles of a user other than yourself.");
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [Route("roles")]
    public async Task<SubterfugeResponse<GetRolesResponse>> SetRoles(string userId, UpdateRolesRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Admins only.");
        
        DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (targetUser == null)
            return SubterfugeResponse<GetRolesResponse>.OfFailure(ResponseType.NOT_FOUND, "The user you are trying to update the role for does not exist.");

        targetUser.Claims = request.Claims;
        await _dbUsers.Upsert(targetUser);

        return SubterfugeResponse<GetRolesResponse>.OfSuccess(new GetRolesResponse() {
            Claims = targetUser.Claims
        });
    }
}
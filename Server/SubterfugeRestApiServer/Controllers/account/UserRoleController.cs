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
    public async Task<GetRolesResponse> GetRoles(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (dbUserModel.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == userId);
            if(targetUser == null)
                throw new NotFoundException("Off the grid? We have no record of this user.");
            
            var response = new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Claims = targetUser.Claims
            };
            return response;
        }
        
        if (dbUserModel.Id == userId)
        {
            var response = new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Claims = dbUserModel.Claims
            };
            return response;
        }

        throw new ForbidException();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [Route("roles")]
    public async Task<GetRolesResponse> SetRoles(string userId, UpdateRolesRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (targetUser == null)
            throw new NotFoundException("User not found.");

        targetUser.Claims = request.Claims;
        await _dbUsers.Upsert(targetUser);

        var response = new GetRolesResponse() {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Claims = targetUser.Claims
        };
        return response;
    }
}
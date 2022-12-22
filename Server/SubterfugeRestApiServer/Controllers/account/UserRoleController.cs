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
[Route("api/user/{userId}/[action]")]
public class UserRoleController : ControllerBase
{
    private IDatabaseCollection<DbUserModel> _dbUsers;

    public UserRoleController(IDatabaseCollectionProvider mongo)
    {
        _dbUsers = mongo.GetCollection<DbUserModel>();
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetRolesResponse>> GetRoles(string userId)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return Unauthorized();

        if (dbUserModel.HasClaim(UserClaim.Administrator))
        {
            DbUserModel? targetUser = await _dbUsers.Query().FirstAsync(it => it.Id == userId);
            if (targetUser != null)
            {
                var response = new GetRolesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                    Claims = targetUser.Claims
                };
                return Ok(response);
            }
            return NotFound(new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST, "Off the grid? We have no record of this user.")
            });
        }
        
        if (dbUserModel.Id == userId)
        {
            var response = new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Claims = dbUserModel.Claims
            };
            return Ok(response);
        }
        
        return Forbid();
    }
    
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<ActionResult<GetRolesResponse>> SetRoles(string userId, UpdateRolesRequest updateRoleRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return Unauthorized();
        
        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return Forbid();
        
        DbUserModel? targetUser = await _dbUsers.Query().FirstAsync(it => it.Id == userId);
        if (targetUser == null)
            return NotFound();

        targetUser.Claims = updateRoleRequest.Claims;
        await _dbUsers.Upsert(targetUser);

        var response = new GetRolesResponse() {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Claims = targetUser.Claims
        };
        return Ok(response);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/user/{userid}/[action]")]
public class UserRoleController : ControllerBase
{
    public UserRoleController(string userId)
    {
        this.userId = userId;
    }
    private readonly string userId;

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetRolesResponse>> GetRoles(GetRolesRequest roleRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return Unauthorized();

        if (dbUserModel.HasClaim(UserClaim.Administrator))
        {
            DbUserModel targetUser = await DbUserModel.GetUserFromGuid(userId);
            if (targetUser != null)
            {
                var response = new GetRolesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                    Claims = targetUser.UserModel.Claims
                };
                return Ok(response);
            }
            return NotFound(new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
            });
        }
        
        if (dbUserModel.UserModel.Id == userId)
        {
            var response = new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Claims = dbUserModel.UserModel.Claims
            };
            return Ok(response);
        }
        
        return Forbid();
    }
    
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<ActionResult<GetRolesResponse>> SetRoles(UpdateRolesRequest updateRoleRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return Unauthorized();

        DbUserModel? targetUser = await DbUserModel.GetUserFromGuid(userId);
        if (targetUser == null)
            return Conflict();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return Unauthorized();

        targetUser.UserModel.Claims = updateRoleRequest.Claims;
        await targetUser.SaveToDatabase();

        var response = new GetRolesResponse() {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Claims = targetUser.UserModel.Claims
        };
        return Ok(response);
    }
}
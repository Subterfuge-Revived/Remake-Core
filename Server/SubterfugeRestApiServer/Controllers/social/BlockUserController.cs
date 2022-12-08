using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/User/[controller]/[action]")]
public class BlockUserController : ControllerBase
{
    
    public BlockUserController(
        IConfiguration configuration,
        ILogger<AccountController> logger
    )
    {
        _config = configuration;
        _logger = logger;
    }
    
    
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    [HttpPost(Name="block")]
    public async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        DbUserModel friend = await DbUserModel.GetUserFromGuid(request.UserIdToBlock);
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
    
    [HttpPost(Name="unblock")]
    public async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        // Check if player is valid.
        DbUserModel friend = await DbUserModel.GetUserFromGuid(request.UserIdToUnblock);
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
    
}
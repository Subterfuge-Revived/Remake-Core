using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SubterfugeRestApiServer;

[ApiController]
[Authorize]
[Route("api/User/{userId}/[controller]")]
public class FriendController : ControllerBase
{
    
}
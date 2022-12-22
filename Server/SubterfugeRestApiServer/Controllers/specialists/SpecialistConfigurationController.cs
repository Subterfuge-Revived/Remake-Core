using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
[Route("api/specialist/")]
public class SpecialistConfigurationController: ControllerBase
{
    
    private IDatabaseCollection<DbSpecialistConfiguration> _dbSpecialists;

    public SpecialistConfigurationController(IDatabaseCollectionProvider mongo)
    {
        this._dbSpecialists = mongo.GetCollection<DbSpecialistConfiguration>();
    }

    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<SubmitCustomSpecialistResponse>> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();

        var dbItem = DbSpecialistConfiguration.FromRequest(request, user.ToUser());
        await _dbSpecialists.Upsert(dbItem);

        // Get the generated specialist ID
        string specialistId = dbItem.Id;

        return Ok(new SubmitCustomSpecialistResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistConfigurationId = specialistId,
        });
    }
    
    // TODO: Change this to a GET with URL params
    [HttpPost]
    public async Task<ActionResult<GetCustomSpecialistsResponse>> GetCustomSpecialists(GetCustomSpecialistsRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Cleanup this where clause to that it only applies if they are not null
        // TODO: Change this into a GET with query params.
        var results = (await _dbSpecialists.Query()
            .Where(it => it.SpecialistName.Contains(request.SearchTerm))
            .Where(it => it.PromotesFromSpecialistId.Contains(request.PromotesFromSpecialistId))
            .Where(it => it.Creator.Id == request.CreatedByPlayerId)
            .OrderByDescending(specialist => specialist.Ratings.AverageRating())
            .Skip(((int)request.PageNumber - 1) * 50)
            .Take(50)
            .ToListAsync())
            .Select(package => package.ToSpecialistConfiguration())
            .ToList();

        GetCustomSpecialistsResponse response = new GetCustomSpecialistsResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            CustomSpecialists = results
        };

        return Ok(response);
    }
    
    [HttpGet]
    [Route("{specialistId}")]
    public async Task<ActionResult<GetCustomSpecialistsResponse>> GetCustomSpecialists(string specialistId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Add filters to this endpoint.
        var results = (await _dbSpecialists.Query()
            .Where(it => it.Id == specialistId)
            .ToListAsync())
            .Select(package => package.ToSpecialistConfiguration())
            .ToList();

        return Ok(new GetCustomSpecialistsResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            CustomSpecialists = results
        });
    }
    
}
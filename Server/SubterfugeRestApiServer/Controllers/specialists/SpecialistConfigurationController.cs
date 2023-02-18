using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;
using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Controllers.specialists;

[ApiController]
[Authorize]
[Route("api/specialist/")]
public class SpecialistConfigurationController: ControllerBase, ISubterfugeCustomSpecialistApi
{
    
    private IDatabaseCollection<DbSpecialistConfiguration> _dbSpecialists;

    public SpecialistConfigurationController(IDatabaseCollectionProvider mongo)
    {
        this._dbSpecialists = mongo.GetCollection<DbSpecialistConfiguration>();
    }

    [HttpPost]
    [Route("create")]
    public async Task<SubterfugeResponse<SubmitCustomSpecialistResponse>> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<SubmitCustomSpecialistResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        var dbItem = DbSpecialistConfiguration.FromRequest(request, user.ToSimpleUser());
        await _dbSpecialists.Upsert(dbItem);

        // Get the generated specialist ID
        string specialistId = dbItem.Id;

        return SubterfugeResponse<SubmitCustomSpecialistResponse>.OfSuccess(new SubmitCustomSpecialistResponse()
        {
            SpecialistConfigurationId = specialistId,
        });
    }
    
    [HttpGet]
    public async Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialists([FromQuery] GetCustomSpecialistsRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<GetCustomSpecialistsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
        
        IMongoQueryable<DbSpecialistConfiguration> query = _dbSpecialists.Query();
        
        if (request.SearchTerm != null)
            query = query.Where(it => it.SpecialistName.ToLower().Contains(request.SearchTerm.ToLower()));
        
        if(request.CreatedByPlayerId != null)
            query = query.Where(it => it.Creator.Id == request.CreatedByPlayerId);
        
        if(request.PromotesFromSpecialistId != null)
            query = query.Where(it => it.PromotesFromSpecialistId == request.PromotesFromSpecialistId);
        
        var results = (await query
                .OrderByDescending(specialist => specialist.CreatedAt)
                .Skip(((int)request.Pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(package => package.ToSpecialistConfiguration())
            .ToList();

        GetCustomSpecialistsResponse response = new GetCustomSpecialistsResponse()
        {
            CustomSpecialists = results
        };

        return SubterfugeResponse<GetCustomSpecialistsResponse>.OfSuccess(response);
    }

    [HttpGet]
    [Route("{specialistId}")]
    public async Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialist(string specialistId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<GetCustomSpecialistsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        // Search through all specialists for the search term.
        // TODO: Add filters to this endpoint.
        var results = (await _dbSpecialists.Query()
                .Where(it => it.Id == specialistId)
                .ToListAsync())
            .Select(package => package.ToSpecialistConfiguration())
            .ToList();

        return SubterfugeResponse<GetCustomSpecialistsResponse>.OfSuccess(new GetCustomSpecialistsResponse()
        {
            CustomSpecialists = results
        });
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
[Route("api/specialist/package/")]
public class SpecialistPackageController : ControllerBase, ISubterfugeSpecialistPackageApi
{
    private IDatabaseCollection<DbSpecialistPackage> _dbSpecialistPackages;
    private IDatabaseCollection<DbSpecialistConfiguration> _dbSpecialistCollection;

    public SpecialistPackageController(IDatabaseCollectionProvider mongo)
    {
        this._dbSpecialistPackages = mongo.GetCollection<DbSpecialistPackage>();
        this._dbSpecialistCollection = mongo.GetCollection<DbSpecialistConfiguration>();
    }
    
    [HttpPost]
    [Route("create")]
    public async Task<SubterfugeResponse<CreateSpecialistPackageResponse>> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<CreateSpecialistPackageResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");

        DbSpecialistPackage package = DbSpecialistPackage.FromRequest(request, user.ToSimpleUser());
        
        // Ensure that any referenced specialists and packages exist!
        var existingSpecialists = await _dbSpecialistCollection.Query()
            .Where(it => request.SpecialistIds.Contains(it.Id))
            .ToListAsync();

        if (existingSpecialists.Count != request.SpecialistIds.Count)
            return SubterfugeResponse<CreateSpecialistPackageResponse>.OfFailure(ResponseType.NOT_FOUND, "A referenced specialist cannot be found.");
        
        var existingPackages = await _dbSpecialistPackages.Query()
            .Where(it => request.PackageIds.Contains(it.Id))
            .ToListAsync();

        if (existingPackages.Count != request.PackageIds.Count)
            return SubterfugeResponse<CreateSpecialistPackageResponse>.OfFailure(ResponseType.NOT_FOUND, "A referenced specialist package cannot be found.");
        
        await _dbSpecialistPackages.Upsert(package);

        return SubterfugeResponse<CreateSpecialistPackageResponse>.OfSuccess(new CreateSpecialistPackageResponse()
        {
            SpecialistPackageId = package.Id,
        });
    }
    
    [HttpGet]
    public async Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages([FromQuery] GetSpecialistPackagesRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<GetSpecialistPackagesResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
        
        IMongoQueryable<DbSpecialistPackage> query = _dbSpecialistPackages.Query();
        
        if (request.SearchTerm != null)
            query = query.Where(it => it.PackageName.ToLower().Contains(request.SearchTerm.ToLower()));
        
        if(request.CreatedByUserId != null)
            query = query.Where(it => it.Creator.Id == request.CreatedByUserId);
        
        if(request.ContainsSpecialistId != null)
            query = query.Where(it => it.SpecialistIds.Contains(request.ContainsSpecialistId));
        
        if(request.ContainsPackageId != null)
            query = query.Where(it => it.PackageIds.Contains(request.ContainsPackageId));

        var allPackages = await _dbSpecialistPackages.Query().ToListAsync();
        
        var results = (await query
                .OrderByDescending(specialist => specialist.CreatedAt)
                .Skip(((int)request.Pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(package => package.ToSpecialistPackage())
            .ToList();

        GetSpecialistPackagesResponse response = new GetSpecialistPackagesResponse()
        {
            SpecialistPackages = results
        };

        return SubterfugeResponse<GetSpecialistPackagesResponse>.OfSuccess(response);
    }
    
    [HttpGet]
    [Route("{packageId}")]
    public async Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages(string packageId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return SubterfugeResponse<GetSpecialistPackagesResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
            
        // Search through all specialists for the search term.
        // TODO: Apply filters here
        List<SpecialistPackage> results = (await _dbSpecialistPackages.Query()
            .Where(specialistPackage => specialistPackage.Id == packageId)
            .ToListAsync())
            .Select(it => it.ToSpecialistPackage())
            .ToList();

        return SubterfugeResponse<GetSpecialistPackagesResponse>.OfSuccess(new GetSpecialistPackagesResponse()
        {
            SpecialistPackages = results
        });
    }

}
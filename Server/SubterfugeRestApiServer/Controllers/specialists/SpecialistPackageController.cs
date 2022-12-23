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
    public async Task<CreateSpecialistPackageResponse> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            throw new UnauthorizedException();

        DbSpecialistPackage package = DbSpecialistPackage.FromRequest(request, user.ToUser());
        
        // Ensure that any referenced specialists and packages exist!
        var existingSpecialists = await _dbSpecialistCollection.Query()
            .Where(it => request.SpecialistIds.Contains(it.Id))
            .ToListAsync();

        if (existingSpecialists.Count != request.SpecialistIds.Count)
            throw new NotFoundException("A referenced specialist cannot be found.");
        
        var existingPackages = await _dbSpecialistPackages.Query()
            .Where(it => request.PackageIds.Contains(it.Id))
            .ToListAsync();

        if (existingPackages.Count != request.PackageIds.Count)
            throw new NotFoundException("A referenced specialist package cannot be found.");
        
        await _dbSpecialistPackages.Upsert(package);

        return new CreateSpecialistPackageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackageId = package.Id,
        };
    }
    
    [HttpGet]
    public async Task<GetSpecialistPackagesResponse> GetSpecialistPackages([FromQuery] GetSpecialistPackagesRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            throw new UnauthorizedException();
        
        IMongoQueryable<DbSpecialistPackage> query = _dbSpecialistPackages.Query();
        
        if (request.SearchTerm != null)
            query = query.Where(it => it.PackageName.Contains(request.SearchTerm));
        
        if(request.CreatedByUserId != null)
            query = query.Where(it => it.Creator.Id == request.CreatedByUserId);
        
        if(request.ContainsSpecialistId != null)
            query = query.Where(it => it.SpecialistIds.Contains(request.ContainsSpecialistId));
        
        if(request.ContainsPackageId != null)
            query = query.Where(it => it.PackageIds.Contains(request.ContainsPackageId));
        
        var results = (await query
                .OrderByDescending(specialist => specialist.CreatedAt)
                .Skip(((int)request.Pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(package => package.ToSpecialistPackage())
            .ToList();

        GetSpecialistPackagesResponse response = new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackages = results
        };

        return response;
    }
    
    [HttpGet]
    [Route("{packageId}")]
    public async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(string packageId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            throw new UnauthorizedException();
            
        // Search through all specialists for the search term.
        // TODO: Apply filters here
        List<SpecialistPackage> results = (await _dbSpecialistPackages.Query()
            .Where(specialistPackage => specialistPackage.Id == packageId)
            .ToListAsync())
            .Select(it => it.ToSpecialistPackage())
            .ToList();

        return new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackages = results
        };
    }

}
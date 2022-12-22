using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
[Route("api/specialist/package/")]
public class SpecialistPackageController : ControllerBase
{
    private IDatabaseCollection<DbSpecialistPackage> _dbSpecialistPackages;

    public SpecialistPackageController(IDatabaseCollectionProvider mongo)
    {
        this._dbSpecialistPackages = mongo.GetCollection<DbSpecialistPackage>();
    }
    
    [HttpPost]
    [Route("create")]
    public async Task<ActionResult<CreateSpecialistPackageResponse>> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();

        DbSpecialistPackage package = DbSpecialistPackage.FromRequest(request, user.ToUser());
        await _dbSpecialistPackages.Upsert(package);

        return Ok(new CreateSpecialistPackageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackageId = package.Id,
        });
    }
    
    // TODO: turn this into a GET with URL params
    [HttpPost]
    public async Task<ActionResult<GetSpecialistPackagesResponse>> GetSpecialistPackages(GetSpecialistPackagesRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Cleanup this where clause to that it only applies if they are not null
        // TODO: Change this into a GET with query params.
        List<SpecialistPackage> results = (await _dbSpecialistPackages.Query()
            .Where(it => it.PackageName.Contains(request.SearchTerm))
            .Where(it => it.SpecialistIds.Contains(request.PackageContainsSpecialistId))
            .Where(it => it.Creator.Id == request.IsPackageCreatedById)
            .Skip((int)request.PageNumber - 1 * 50)
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToSpecialistPackage())
            .ToList();

        GetSpecialistPackagesResponse response = new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackages = results
        };

        return Ok(response);
    }
    
    [HttpGet]
    [Route("{packageId}")]
    public async Task<ActionResult<GetSpecialistPackagesResponse>> GetSpecialistPackages(string packageId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Apply filters here
        List<SpecialistPackage> results = (await _dbSpecialistPackages.Query()
            .Where(specialistPackage => specialistPackage.Id == packageId)
            .ToListAsync())
            .Select(it => it.ToSpecialistPackage())
            .ToList();

        return Ok(new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackages = results
        });
    }

}
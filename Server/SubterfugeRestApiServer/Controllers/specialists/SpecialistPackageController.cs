using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
public class SpecialistPackageController : ControllerBase
{
    
    [HttpPost]
    [Route("api/specialist/package/create")]
    public async Task<ActionResult<CreateSpecialistPackageResponse>> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();

        // Set author
        request.SpecialistPackage.Creator = user.AsUser();
        SpecialistPackageModel packageModel = new SpecialistPackageModel(request.SpecialistPackage);
        await packageModel.SaveToDatabase();

        // Get the generated specialist ID
        string packageId = packageModel.SpecialistPackage.Id;

        return Ok(new CreateSpecialistPackageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackageId = packageId,
        });
    }
    
    [HttpPost]
    [Route("api/specialist/packages")]
    public async Task<ActionResult<GetSpecialistPackagesResponse>> GetSpecialistPackages(GetSpecialistPackagesRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Apply filters here
        List<SpecialistPackageModel> results = (await SpecialistPackageModel.Search(request.SearchTerm)).Skip((int)request.PageNumber * 50).Take(50).ToList();

        GetSpecialistPackagesResponse response = new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
            
        foreach (SpecialistPackageModel model in results)
        {
            response.SpecialistPackages.Add(model.SpecialistPackage);   
        }

        return Ok(response);
    }
    
    [HttpGet]
    [Route("api/specialist/package/{packageId}")]
    public async Task<ActionResult<GetSpecialistPackagesResponse>> GetSpecialistPackages(string packageId)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if (user == null)
            return Unauthorized();
            
        // Search through all specialists for the search term.
        // TODO: Apply filters here
        List<SpecialistPackage> results = (await MongoConnector.GetSpecialistPackageCollection()
            .FindAsync(it => it.Id == packageId)).ToList();

        return Ok(new GetSpecialistPackagesResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackages = results
        });
    }

}
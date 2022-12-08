using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class SpecialistPackageController : ControllerBase
{
    public SpecialistPackageController(
        IConfiguration configuration,
        ILogger<AccountController> logger
    )
    {
        _config = configuration;
        _logger = logger;
    }
    
    
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    [HttpPost]
    public async Task<CreateSpecialistPackageResponse> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if(user == null)
            return new CreateSpecialistPackageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        // Set author
        request.SpecialistPackage.Creator = user.AsUser();
        SpecialistPackageModel packageModel = new SpecialistPackageModel(request.SpecialistPackage);
        await packageModel.SaveToDatabase();

        // Get the generated specialist ID
        string packageId = packageModel.SpecialistPackage.Id;

        return new CreateSpecialistPackageResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistPackageId = packageId,
        };
    }
    
    [HttpGet]
    async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(GetSpecialistPackagesRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if(user == null)
            return new GetSpecialistPackagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
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

        return response;
    }

}
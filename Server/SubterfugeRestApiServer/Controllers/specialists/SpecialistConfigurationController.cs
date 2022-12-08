using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.specialists;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class SpecialistConfigurationController: ControllerBase
{
    public SpecialistConfigurationController(
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
    public async Task<SubmitCustomSpecialistResponse> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if(user == null)
            return new SubmitCustomSpecialistResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };

        // Set author
        request.SpecialistConfiguration.Creator = user.AsUser();
        SpecialistConfigurationModel configModel = new SpecialistConfigurationModel(request.SpecialistConfiguration);
        await configModel.Save();

        // Get the generated specialist ID
        string specialistId = configModel.SpecialistConfig.Id;

        return new SubmitCustomSpecialistResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            SpecialistConfigurationId = specialistId,
        };
    }
    
    [HttpGet]
    public async Task<GetCustomSpecialistsResponse> GetCustomSpecialists(GetCustomSpecialistsRequest request)
    {
        DbUserModel? user = HttpContext.Items["User"] as DbUserModel;
        if(user == null)
            return new GetCustomSpecialistsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            };
            
        // Search through all specialists for the search term.
        // TODO: Add filters to this endpoint.
        List<SpecialistConfigurationModel> results = (await SpecialistConfigurationModel.Search(request.SearchTerm)).Skip((int)request.PageNumber * 50).Take(50).ToList();

        GetCustomSpecialistsResponse response = new GetCustomSpecialistsResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
            
        foreach (SpecialistConfigurationModel model in results)
        {
            response.CustomSpecialists.Add(model.SpecialistConfig);   
        }

        return response;
    }
    
}
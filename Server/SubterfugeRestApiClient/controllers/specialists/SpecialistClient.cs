using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.specialists;

public class SpecialistClient : ISubterfugeCustomSpecialistApi, ISubterfugeSpecialistPackageApi
{
    private SubterfugeHttpClient client;

    public SpecialistClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }

    public async Task<SubterfugeResponse<CreateSpecialistPackageResponse>> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        return await client.Post<CreateSpecialistPackageRequest, CreateSpecialistPackageResponse>($"api/specialist/package/create", request);
    }
    
    public async Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages(GetSpecialistPackagesRequest request)
    {
        return await client.Get<GetSpecialistPackagesResponse>($"api/specialist/package",
            new Dictionary<string, string>()
            {
                { "PageNumber", request.Pagination.ToString() },
                { "SearchTerm", request.SearchTerm },
                { "CreatedByUserId", request.CreatedByUserId },
                { "ContainsSpecialistId", request.ContainsSpecialistId },
                { "ContainsPackageId", request.ContainsPackageId },
            }
        );
    }
    
    public async Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages(string packageId)
    {
        return await client.Get<GetSpecialistPackagesResponse>($"api/specialist/package/{packageId}", null);
    }

    public async Task<SubterfugeResponse<SubmitCustomSpecialistResponse>> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request)
    {
        return await client.Post<SubmitCustomSpecialistRequest, SubmitCustomSpecialistResponse>($"api/specialist/create", request);
    }

    public async Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialists(GetCustomSpecialistsRequest request)
    {
        return await client.Get<GetCustomSpecialistsResponse>($"api/specialist",
            new Dictionary<string, string>()
            {
                { "PageNumber", request.Pagination.ToString() },
                { "SearchTerm", request.SearchTerm },
                { "CreatedByPlayerId", request.CreatedByPlayerId },
                { "PromotesFromSpecialistId", request.PromotesFromSpecialistId },
            }
        );
    }

    public async Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialist(string specialistId)
    {
        return await client.Get<GetCustomSpecialistsResponse>($"api/specialist/{specialistId}", null);
    }
}
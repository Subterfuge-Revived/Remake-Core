using System.Web;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.specialists;

public class SpecialistClient : ISubterfugeCustomSpecialistApi, ISubterfugeSpecialistPackageApi
{
    private HttpClient client;

    public SpecialistClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<CreateSpecialistPackageResponse> CreateSpecialistPackage(CreateSpecialistPackageRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/specialist/package/create", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<CreateSpecialistPackageResponse>();
    }
    
    public async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(GetSpecialistPackagesRequest request)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["PageNumber"] = request.Pagination.ToString();
        query["SearchTerm"] = request.SearchTerm;
        query["CreatedByUserId"] = request.CreatedByUserId;
        query["ContainsSpecialistId"] = request.ContainsSpecialistId;
        query["ContainsPackageId"] = request.ContainsPackageId;
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/specialist/package?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetSpecialistPackagesResponse>();
    }
    
    public async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(string packageId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/specialist/package/{packageId}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetSpecialistPackagesResponse>();
    }

    public async Task<SubmitCustomSpecialistResponse> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/specialist/create", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SubmitCustomSpecialistResponse>();
    }

    public async Task<GetCustomSpecialistsResponse> GetCustomSpecialists(GetCustomSpecialistsRequest request)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["PageNumber"] = request.Pagination.ToString();
        query["SearchTerm"] = request.SearchTerm;
        query["CreatedByPlayerId"] = request.CreatedByPlayerId;
        query["PromotesFromSpecialistId"] = request.PromotesFromSpecialistId;
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/specialist?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetCustomSpecialistsResponse>();
    }

    public async Task<GetCustomSpecialistsResponse> GetCustomSpecialist(string specialistId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/specialist/{specialistId}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetCustomSpecialistsResponse>();
    }
}
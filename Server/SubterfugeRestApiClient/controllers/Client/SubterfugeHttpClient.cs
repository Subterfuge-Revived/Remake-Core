using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Api.Client.controllers.Client;

public class SubterfugeHttpClient
{
    private HttpClient Client = new HttpClient();

    /// <summary>
    /// The client constructor.
    ///
    /// By default, this constructor works for local development as well as docker container environments.
    /// The client will look for the `ASPNETCORE_ENVIRONMENT` environment variable.
    /// If the environment variable is `Docker`, the client connects to: "server:5295", otherwise it will connect to "localhost:5295".
    /// If you need to connect to a custom server URL, use the secondary constructor which accepts a base URL.
    /// </summary>
    public SubterfugeHttpClient()
    {
        string hostname = "localhost";
        int port = 5295;
            
        // Get environment
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Docker")
        {
            hostname = "server";
        }
        setupClient($"http://{hostname}:{port}/");
    }
        
    /// <summary>
    /// Creates an instance of a subterfuge client using the specified baseURL.
    /// </summary>
    /// <param name="baseUrl">The server URL to connect to. Should include the protocol, hostname, and port. ex. http://localhost:5295</param>
    public SubterfugeHttpClient(string baseUrl)
    {
        setupClient(baseUrl);
    }
    
    private void setupClient(string baseUrl)
    {
        Client.BaseAddress = new Uri(baseUrl);
        Client.DefaultRequestHeaders.Accept.Clear();
        Client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }
    
    public async Task<SubterfugeResponse<TResponse>> Post<TRequest, TResponse>(string url, TRequest data)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(url, data);
        return await ReturnSerializedResponse<TResponse>(response);
    }
    
    public async Task<SubterfugeResponse<TResponse>> Get<TResponse>(string url, IDictionary<string, string> getParams)
    {
        HttpResponseMessage response = await Client.GetAsync(GetUrlFromParams(url, getParams));
        return await ReturnSerializedResponse<TResponse>(response);
    }
    
    public async Task<SubterfugeResponse<TResponse>> Put<TRequest, TResponse>(string url, TRequest data)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(url, data);
        return await ReturnSerializedResponse<TResponse>(response);
    }
    
    public async Task<SubterfugeResponse<TResponse>> Delete<TResponse>(string url, IDictionary<string, string> deleteParams)
    {
        HttpResponseMessage response = await Client.DeleteAsync(GetUrlFromParams(url, deleteParams));
        return await ReturnSerializedResponse<TResponse>(response);
    }

    public void SetAuthHeader(string Token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
    }

    private string GetUrlFromParams(string baseUrl, IDictionary<string, string> queryParams)
    {
        string queryUrl = baseUrl;
        if (queryParams != null)
        {
            var nonNullParams = queryParams
                .Where(it => it.Value != null)
                .ToDictionary(it => it.Key, it => it.Value);

            queryUrl = QueryHelpers.AddQueryString(baseUrl, nonNullParams);
        }

        return queryUrl;
    }

    private async Task<SubterfugeResponse<TResponse>> ReturnSerializedResponse<TResponse>(HttpResponseMessage rawResponse)
    {
        if (!rawResponse.IsSuccessStatusCode)
        {
            if (rawResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                return SubterfugeResponse<TResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Permission denied.");
            }
            if (rawResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return SubterfugeResponse<TResponse>.OfFailure(ResponseType.UNAUTHORIZED, "You are not logged in.");
            }
            // TODO: parse the failure content into a response
            return SubterfugeResponse<TResponse>.OfFailure(ResponseType.INTERNAL_SERVER_ERROR, rawResponse.ReasonPhrase);
        }
        return await rawResponse.Content.ReadAsAsync<SubterfugeResponse<TResponse>>();
    }
}
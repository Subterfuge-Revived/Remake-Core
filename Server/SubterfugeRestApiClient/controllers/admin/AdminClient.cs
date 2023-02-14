using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.admin;

public class AdminClient : ISubterfugeAdminApi
{
    private SubterfugeHttpClient client;

    public AdminClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }
    
    public async Task<SubterfugeResponse<ServerActionLogResponse>> GetActionLog(ServerActionLogRequeset request)
    {
        return await client.Get<ServerActionLogResponse>($"api/admin/serverLog", new Dictionary<string, string>()
        {
            { "Pagination", request.Pagination.ToString() },
            { "Username", request.Username.ToString() },
            { "UserId", request.UserId.ToString() },
            { "HttpMethod", request.HttpMethod.ToString() },
            { "RequestUrl", request.RequestUrl.ToString() },
        });
    }

    public async Task<SubterfugeResponse<ServerExceptionLogResponse>> GetServerExceptions(ServerExceptionLogRequest request)
    {
        return await client.Get<ServerExceptionLogResponse>($"api/admin/exceptions", new Dictionary<string, string>()
        {
            { "Pagination", request.Pagination.ToString() },
            { "Username", request.Username.ToString() },
            { "UserId", request.UserId.ToString() },
            { "HttpMethod", request.HttpMethod.ToString() },
            { "RequestUrl", request.RequestUrl.ToString() },
            { "ExceptionSource", request.ExceptionSource.ToString() },
            { "RemoteIpAddress", request.RemoteIpAddress.ToString() },
        });
    }

    public async Task<SubterfugeResponse<BanPlayerResponse>> BanPlayer(BanPlayerRequest banPlayerRequest)
    {
        return await client.Post<BanPlayerRequest, BanPlayerResponse>($"api/admin/banPlayer", banPlayerRequest);
    }

    public async Task<SubterfugeResponse<BanIpResponse>> BanIp(BanIpRequest banIpRequest)
    {
        return await client.Post<BanIpRequest, BanIpResponse>($"api/admin/banIp", banIpRequest);
    }

    public async Task<SubterfugeResponse<GetIpBansResponse>> GetIpBans(int pagination)
    {
        return await client.Get<GetIpBansResponse>($"api/admin/ipBans", new Dictionary<string, string>()
        {
            { "Pagination", pagination.ToString() }
        });
    }

    public async Task<SubterfugeResponse<GetBannedPlayerResponse>> GetBannedPlayers(int pagination)
    {
        return await client.Get<GetBannedPlayerResponse>($"api/admin/playerBans", new Dictionary<string, string>()
        {
            { "Pagination", pagination.ToString() }
        });
    }

    public async Task<SubterfugeResponse<Echo>> EchoRequest(Echo request)
    {
        return await client.Post<Echo, Echo>($"api/admin/echo", request);
    }
}
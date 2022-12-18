using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.game;

public class GroupClient
{
    private HttpClient client;

    public GroupClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<CreateMessageGroupResponse> CreateMessageGroup(string roomId, CreateMessageGroupRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/room/{roomId}/CreateMessageGroup", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<CreateMessageGroupResponse>();
    }
    
    public async Task<GetMessageGroupsResponse> GetMessageGroups(string roomId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/room/{roomId}/GetMessageGroups");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetMessageGroupsResponse>();
    }
    
    public async Task<SendMessageResponse> SendMessage(string roomId, string groupId, SendMessageRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/room/{roomId}/group/{groupId}/send", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SendMessageResponse>();
    }
    
    public async Task<SendMessageResponse> GetMessages(string roomId, string groupId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/room/{roomId}/group/{groupId}/messages");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SendMessageResponse>();
    }
    
}
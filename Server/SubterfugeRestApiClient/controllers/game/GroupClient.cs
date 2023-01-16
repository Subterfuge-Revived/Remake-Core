using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.game;

public class GroupClient : ISubterfugeGroupChatApi
{
    private HttpClient client;

    public GroupClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, string roomId)
    {
        Console.WriteLine("CreateMessageGroup");
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/room/{roomId}/group/create", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<CreateMessageGroupResponse>();
    }

    public async Task<SendMessageResponse> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        Console.WriteLine("SendMessage");
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/room/{roomId}/group/{groupId}/send", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SendMessageResponse>();
    }

    public async Task<GetMessageGroupsResponse> GetMessageGroups(string roomId)
    {
        Console.WriteLine("GetMessageGroups");
        HttpResponseMessage response = await client.GetAsync($"api/room/{roomId}/groups");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetMessageGroupsResponse>();
    }

    public async Task<GetGroupMessagesResponse> GetMessages(GetGroupMessagesRequest request, string roomId, string groupId)
    {
        Console.WriteLine("GetMessages");
        HttpResponseMessage response = await client.GetAsync($"api/room/{roomId}/group/{groupId}/messages");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetGroupMessagesResponse>();
    }

}
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.game;

public class GroupClient : ISubterfugeGroupChatApi
{
    private SubterfugeHttpClient client;

    public GroupClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }

    public async Task<SubterfugeResponse<CreateMessageGroupResponse>> CreateMessageGroup(CreateMessageGroupRequest request, string roomId)
    {
        return await client.Post<CreateMessageGroupRequest, CreateMessageGroupResponse>($"api/room/{roomId}/group/create", request);
    }

    public async Task<SubterfugeResponse<SendMessageResponse>> SendMessage(SendMessageRequest request, string roomId, string groupId)
    {
        return await client.Post<SendMessageRequest, SendMessageResponse>($"api/room/{roomId}/group/{groupId}/send", request);
    }

    public async Task<SubterfugeResponse<GetMessageGroupsResponse>> GetMessageGroups(string roomId)
    {
        return await client.Get<GetMessageGroupsResponse>($"api/room/{roomId}/groups", null);
    }

    public async Task<SubterfugeResponse<GetGroupMessagesResponse>> GetMessages(GetGroupMessagesRequest request, string roomId, string groupId)
    {
        return await client.Get<GetGroupMessagesResponse>($"api/room/{roomId}/group/{groupId}/messages", null);
    }

}
using Subterfuge.Remake.Api.Client.controllers.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;

namespace Subterfuge.Remake.Api.Client.controllers.game;

public class GameAnnouncementClient : ISubterfugeAnnouncementApi
{
    private SubterfugeHttpClient client;

    public GameAnnouncementClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }
    
    public async Task<SubterfugeResponse<CreateAnnouncementResponse>> CreateAnnouncement(CreateAnnouncementRequest announcementRequest)
    {
        return await client.Post<CreateAnnouncementRequest, CreateAnnouncementResponse>($"api/announcements/create", announcementRequest);
    }

    public async Task<SubterfugeResponse<CreateAnnouncementResponse>> UpdateAnnouncement(string id, CreateAnnouncementRequest announcementRequest)
    {
        return await client.Put<CreateAnnouncementRequest, CreateAnnouncementResponse>($"api/announcements/{id}", announcementRequest);
    }

    public async Task<SubterfugeResponse<GetAnnouncementsResponse>> GetAnnouncements()
    {
        return await client.Get<GetAnnouncementsResponse>($"api/announcements", null);
    }
}
using NUnit.Framework;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.game;

public class GameAnnouncementControllerTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public async Task CanCreateAnAnnouncement()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();
        
        var announcement = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = "global",
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = "Sample title"
        });
        
        Assert.IsTrue(announcement.IsSuccess());
        Assert.NotNull(announcement.GetOrThrow().AnnouncementId);
    }
    
    [Test]
    public async Task CanViewAnAnnouncementAfterItIsCreated()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = "global",
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(1, announcementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(announcementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == title));
    }
    
    [Test]
    public async Task GlobalAnnouncementsAreVisibleToEveryone()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = "global",
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(1, announcementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(announcementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == title));
    }
    
    [Test]
    public async Task CanUpdateAnAnnouncementAfterPostingIt()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = "global",
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(1, announcementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(announcementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == title));
        
        TestUtils.GetClient().UserApi.SetToken(adminUser.Token);

        var updatedTitle = "This is an updated title!";
        var updateResponse = await TestUtils.GetClient().AnnouncementClient.UpdateAnnouncement(createResponse.GetOrThrow().AnnouncementId, new CreateAnnouncementRequest()
        {
            BroadcastTo = "global",
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = updatedTitle
        });
        Assert.IsTrue(updateResponse.IsSuccess());
        Assert.NotNull(updateResponse.GetOrThrow().AnnouncementId);
        
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        
        var updatedAnnouncementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(updatedAnnouncementResponse.IsSuccess());
        Assert.AreEqual(1, updatedAnnouncementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(updatedAnnouncementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == updatedTitle));
    }
    
    [Test]
    public async Task CanBroadcastAnnouncementsToSpecificUsers()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        
        TestUtils.GetClient().UserApi.SetToken(adminUser.Token);

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = userOne.User.Id,
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(1, announcementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(announcementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == title));
    }
    
    [Test]
    public async Task UsersNotInTheBroadcastListCannotSeeAnAnnouncement()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("userTwo");
        
        TestUtils.GetClient().UserApi.SetToken(adminUser.Token);

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = userOne.User.Id,
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(0, announcementResponse.GetOrThrow().Announcements.Count);
    }
    
    [Test]
    public async Task AdminsCanSeeAllAnnouncementsEvenIfTheyAreNotInTheBroadcastList()
    {
        var adminUser = await TestUtils.CreateSuperUserAndLogin();
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        
        TestUtils.GetClient().UserApi.SetToken(adminUser.Token);

        var title = "Sample title";
        var createResponse = await TestUtils.GetClient().AnnouncementClient.CreateAnnouncement(new CreateAnnouncementRequest()
        {
            BroadcastTo = userOne.User.Id,
            StartsAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            Message = "This is an announcement! Welcome to the game!",
            Title = title
        });
        
        Assert.IsTrue(createResponse.IsSuccess());
        Assert.NotNull(createResponse.GetOrThrow().AnnouncementId);

        var announcementResponse = await TestUtils.GetClient().AnnouncementClient.GetAnnouncements();
        Assert.IsTrue(announcementResponse.IsSuccess());
        Assert.AreEqual(1, announcementResponse.GetOrThrow().Announcements.Count);
        Assert.IsTrue(announcementResponse.GetOrThrow().Announcements.Any(announcement => announcement.Title == title));
    }
}
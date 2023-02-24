using NUnit.Framework;
using Subterfuge.Remake.Api.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test;

public class DatabaseSeeder
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    // [Ignore("Ignore until the dev enables it. This will seed an entire database with some sample data everywhere")]
    [Test]
    public async Task SeedDatabase()
    {
        // Create an admin
        var admin = await TestUtils.CreateSuperUserAndLogin();
        TestUtils.GetClient().UserApi.Logout();
        
        var userThree = await AccountUtils.AssertRegisterAccountAndAuthorized("UserThree");
        TestUtils.GetClient().UserApi.Logout();
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        TestUtils.GetClient().UserApi.Logout();
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
        
        var roomOne = await LobbyUtils.CreateLobby(
            "Room 1",
            maxPlayers: 2,
            isRanked: false,
            goal: Goal.Domination
        );
        
        // Login to a different account to test searching for players in a room.
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(TestUtils.CreateJoinRequest(), roomOne.GameConfiguration.Id);

        var groupOne = await TestUtils.GetClient().GroupClient.CreateMessageGroup(new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        }, roomOne.GameConfiguration.Id);

        await TestUtils.GetClient().GroupClient.SendMessage(new SendMessageRequest()
        {
            Message = "Hello there friend!"
        }, roomOne.GameConfiguration.Id, groupOne.GetOrThrow().GroupId);

        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.DrillMineEventData,
                SerializedEventData = Newtonsoft.Json.JsonConvert.SerializeObject(new DrillMineEventData()
                {
                    SourceId = "MyOtherOutpost"
                }),
                OccursAtTick = 15,
            }
        }, roomOne.GameConfiguration.Id);
        
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.DrillMineEventData,
                SerializedEventData = Newtonsoft.Json.JsonConvert.SerializeObject(new DrillMineEventData()
                {
                    SourceId = "MyOutpost"
                }),
                OccursAtTick = 23,
            }
        }, roomOne.GameConfiguration.Id);

        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userThree.User.Id); // User two has an outgoing friend request to user three
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id); // User one and two are friends.
        await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userThree.User.Id); // User one has blocked user three



        TestUtils.GetClient().UserApi.SetToken(admin.Token);

        await TestUtils.GetClient().AdminClient.BanPlayer(new BanPlayerRequest()
            {
                AdminNotes = "Generated from database seed",
                Reason = "Seeded ban",
                Until = DateTime.Now.AddHours(1),
                UserId = userThree.User.Id,
            });
    }
}
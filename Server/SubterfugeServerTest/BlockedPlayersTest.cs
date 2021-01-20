using System;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class BlockedPlayersTest
    {
        SubterfugeClient.SubterfugeClient client;
        // private const String Hostname = "server"; // For docker
        private const String Hostname = "localhost"; // For local
        private const int Port = 5000;
            
        // private const String dbHost = "db"; // For docker
        private const String dbHost = "localhost"; // For local
        private const int dbPort = 6379;


        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            RedisConnector db = new RedisConnector(dbHost, dbPort.ToString(), true);
            client = new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.loginToAccount("userOne");
        }

        [Test]
        public async void UserCanBlockAnotherPlayer()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = await client.BlockPlayerAsync(request);
            Assert.IsTrue(response.Success);
        }
        
        [Test]
        public async void UserCannotBlockInvalidGuid()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = "asdfasdfasdf"
            };

            var exception = Assert.Throws<RpcException>(() => client.BlockPlayerAsync(request));
            Assert.IsTrue(exception.Status.StatusCode == StatusCode.InvalidArgument);
        }
    }
}
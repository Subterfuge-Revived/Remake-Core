using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections.Models;

namespace Tests.AuthTestingHelper
{
    public class AuthTestHelper
    {
        private static Random random = new Random();
        private SubterfugeClient.SubterfugeClient client;
        private Dictionary<String, String> accounts = new Dictionary<String, String>();
        private Dictionary<String, String> userIds = new Dictionary<String, String>();
        
        
        public AuthTestHelper(SubterfugeClient.SubterfugeClient client)
        {
            this.client = client;
        }

        public CreateRoomResponse CreateGameRoom(string roomName)
        {
            return client.CreateNewRoom(new CreateRoomRequest()
            {
                GameSettings = new GameSettings()
                {
                    Anonymous = false,
                    Goal = Goal.Domination,
                    IsRanked = false,
                    MaxPlayers = 5,
                    MinutesPerTick = (1.0 / 60.0), // One second per tick
                },
                RoomName = roomName,
                MapConfiguration = new MapConfiguration()
                {
                    Seed = 123123,
                    OutpostsPerPlayer = 3,
                    MinimumOutpostDistance = 100,
                    MaximumOutpostDistance = 1200,
                    DormantsPerPlayer = 3,
                    OutpostDistribution = new OutpostWeighting()
                    {
                        FactoryWeight = 0.33f,
                        GeneratorWeight = 0.33f,
                        WatchtowerWeight = 0.33f,
                    }
                }
            });
        }

        public SuperUser CreateSuperUser()
        {
            return DbUserModel.CreateSuperUser().Result;
        }

        public String createAccount(String username)
        {
            String pass = RandomString(13);
            
            AccountRegistrationRequest request = new AccountRegistrationRequest()
            {
                Email = "Test@test.com",
                Password = pass,
                Username = username,
                DeviceIdentifier = Guid.NewGuid().ToString(),
            };
 
            AccountRegistrationResponse response = client.RegisterAccount(request);
            userIds.Add(username, response.User.Id);
            accounts.Add(username, pass);
            return response.User.Id;
        }

        public String getAccountId(String username)
        {
            return userIds[username];
        }

        public String loginToAccount(String username)
        {
            if (accounts[username] != null)
            {
                AuthorizationRequest request = new AuthorizationRequest()
                {
                    Password = accounts[username],
                    Username = username
                };

                AuthorizationResponse response = client.Login(request);
                return response.User.Id;
            }

            return "";
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
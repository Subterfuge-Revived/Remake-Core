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
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = roomName,
                AllowedSpecialists = { "a","b","c" },
            };

            return client.CreateNewRoom(createRequest);
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
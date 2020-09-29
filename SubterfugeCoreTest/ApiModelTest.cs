using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SubterfugeCore.Core.Network;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class ApiModelTest
    {

        [TestMethod]
        public void testLoginSuccess()
        {
            string response = @"{
            ""user"": {
                ""id"": 1,
                ""name"": ""asdfg1""
            },
            ""token"": ""S7NzKUoNcMCyQ7PKF4fegQI5I1vwSS4C1xzgvwcBCiR230UbCcOFGyyzn3rUSnSysM7fIjrIKCMh2Ugz""}";

            LoginResponse parsed = JsonConvert.DeserializeObject<LoginResponse>(response);
        }
        
        [TestMethod]
        public void testLoginFailure()
        {
            string response = @"{
                ""message"": ""The given data was invalid."",
                ""errors"": [
                    [
                        ""Authentication failed""
                    ]
                ]
            }";

            LoginResponse parsed = JsonConvert.DeserializeObject<LoginResponse>(response);
        }
        
        [TestMethod]
        public void testRoomListSuccess()
        {
            string response = @"{""array"": [
                {
                    ""room_id"": 1,
                    ""status"": ""open"",
                    ""creator_id"": 1,
                    ""rated"": 1,
                    ""min_rating"": 0,
                    ""description"": ""test"",
                    ""goal"": 1,
                    ""anonymity"": 1,
                    ""map"": 0,
                    ""seed"": 1601229023,
                    ""started_at"": null,
                    ""max_players"": 4,
                    ""players"": [
                        {
                            ""name"": ""Anonymous"",
                            ""id"": 1
                        },
                        {
                            ""name"": ""Anonymous"",
                            ""id"": 2
                        }
                    ],
                    ""message_groups"": []
                },
                {
                    ""room_id"": 2,
                    ""status"": ""open"",
                    ""creator_id"": 1,
                    ""rated"": 1,
                    ""min_rating"": 0,
                    ""description"": ""test"",
                    ""goal"": 1,
                    ""anonymity"": 1,
                    ""map"": 0,
                    ""seed"": 1601230013,
                    ""started_at"": null,
                    ""max_players"": 3,
                    ""players"": [
                        {
                            ""name"": ""Anonymous"",
                            ""id"": 1
                        }
                    ],
                    ""message_groups"": []
                }
            ]}";

            RoomListResponse parsed = JsonConvert.DeserializeObject<RoomListResponse>(response);
        }
        
        [TestMethod]
        public void testGameCreateFailure()
        {
            string response = @"{
                ""message"": ""The given data was invalid."",
                ""errors"": {
                    ""max_players"": [
                        ""The max players field is required.""
                    ],
                    ""goal"": [
                        ""The goal field is required.""
                    ],
                    ""description"": [
                        ""The description field is required.""
                    ],
                    ""map"": [
                        ""The map field is required.""
                    ],
                    ""min_rating"": [
                        ""The min rating field is required.""
                    ],
                    ""rated"": [
                        ""The rated field is required.""
                    ],
                    ""anonymity"": [
                        ""The anonymity field is required.""
                    ]
                }
            }";

            CreateLobbyResponse parsed = JsonConvert.DeserializeObject<CreateLobbyResponse>(response);
        }

    }
}
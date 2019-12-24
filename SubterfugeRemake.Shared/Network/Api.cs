using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SubterfugeFrontend.Shared.Content.Game.Network
{
    class Api
    {
        static readonly HttpClient client = new HttpClient();
        String url = "http://10.0.2.2:80/subterfuge-backend/sandbox/event_exec.php";

        // User settings for the API.
        private String SESSION_ID = null;
        public bool isAuthenticated { get; private set; } = false;

        public Api()
        {
        }

        public async Task<HttpResponseMessage> Register(String username, String email, String password)
        {
            try
            {
                
                var formContent = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("mail", email),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("type", "register")
                });

                HttpResponseMessage response = await client.PostAsync(url, formContent);
                
                // Read the response
                string responseContent = await response.Content.ReadAsStringAsync();
                
                // Parse the JSON response into a dynamic object
                dynamic responseObject = JsonConvert.DeserializeObject(responseContent);

                // Once the user has logged in, save their session token.
                SESSION_ID = responseObject.token;
                if (SESSION_ID != null)
                {
                    isAuthenticated = true;
                }
                
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<HttpResponseMessage> login(String username, String password)
        {
            try
            {
                var formContent = new FormUrlEncodedContent(new []
                {
                    new KeyValuePair<string, string>("player_name", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("type", "login")
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                // Read the response
                string responseContent = await response.Content.ReadAsStringAsync();
                
                // Parse the JSON response into a dynamic object
                dynamic responseObject = JsonConvert.DeserializeObject(responseContent);

                // Once the user has logged in, save their session token.
                SESSION_ID = responseObject.token; 
                if (SESSION_ID != null)
                {
                    isAuthenticated = true;
                }

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<HttpResponseMessage> JoinRoom(int roomId)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "join_room"),
                    new KeyValuePair<string, string>("room_id", roomId.ToString()),
                    new KeyValuePair<string, string>("session_id", SESSION_ID)
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public async Task<HttpResponseMessage> CreateRoom(String description, int maxPlayers, int minRating, bool rated, bool anonymous, String goal, String map)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "new_room"),
                    new KeyValuePair<string, string>("session_id", SESSION_ID),
                    new KeyValuePair<string, string>("description", description),
                    new KeyValuePair<string, string>("max_players", maxPlayers.ToString()),
                    new KeyValuePair<string, string>("min_rating", minRating.ToString()),
                    new KeyValuePair<string, string>("rated", rated.ToString()),
                    new KeyValuePair<string, string>("anonymity", anonymous.ToString()),
                    new KeyValuePair<string, string>("goal", goal),
                    new KeyValuePair<string, string>("map", map),
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public async Task<HttpResponseMessage> LeaveRoom(int roomId)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "leave_room"),
                    new KeyValuePair<string, string>("room_id", roomId.ToString()),
                    new KeyValuePair<string, string>("session_id", SESSION_ID)
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<HttpResponseMessage> StartGameEarly(int roomId)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "start_early"),
                    new KeyValuePair<string, string>("room_id", roomId.ToString()),
                    new KeyValuePair<string, string>("session_id", SESSION_ID)
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public async Task<HttpResponseMessage> SubmitGameEvent(int roomId, int occursAt, String eventJson)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "submit_event"),
                    new KeyValuePair<string, string>("room_id", roomId.ToString()),
                    new KeyValuePair<string, string>("occurs_at", occursAt.ToString()),
                    new KeyValuePair<string, string>("event_msg", eventJson),
                    new KeyValuePair<string, string>("session_id", SESSION_ID)
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public async Task<HttpResponseMessage> GetGameEvents(int roomId)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "get_events"),
                    new KeyValuePair<string, string>("room_id", roomId.ToString()),
                    new KeyValuePair<string, string>("session_id", SESSION_ID)
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public async Task<HttpResponseMessage> GetRoomData(int roomId, String roomStatusFilter, String playerNameFilter)
        {
            try
            {

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("type", "get_events"),
                    new KeyValuePair<string, string>("session_id", SESSION_ID),
                    new KeyValuePair<string, string>("room_status", roomStatusFilter),
                    new KeyValuePair<string, string>("filter_by_player", playerNameFilter),
                });

                HttpResponseMessage response = await client.PostAsync(this.url, formContent);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SubterfugeFrontend.Shared.Content.Game.Network
{
    class Api
    {
        static readonly HttpClient client = new HttpClient();
        String baseUrl = "http://10.0.2.2:80/subterfuge-backend/sandbox";

        String SESSION_ID = null;

        public Api()
        {
        }

        public async Task<HttpResponseMessage> register(String username, String email, String password)
        {
            try
            {
                JObject jsonObject = new JObject();
                jsonObject.Add("username", username);
                jsonObject.Add("mail", email);
                jsonObject.Add("password", password);

                HttpContent postContent = new StringContent(jsonObject.ToString());

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/register_reworked.php", postContent);

                // Get the session ID
                Console.WriteLine(response.Content);
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
                    new KeyValuePair<string, string>("password", password)
                });

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/login_reworked.php", formContent);

                // Get the session ID
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<HttpResponseMessage> joinRoom(int room_id)
        {
            try
            {
                JObject jsonObject = new JObject();
                jsonObject.Add("room_id", room_id);
                jsonObject.Add("session_id", this.SESSION_ID);

                HttpContent postContent = new StringContent(jsonObject.ToString());

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/join_room_reworked.php", postContent);

                // Get the session ID
                Console.WriteLine(response.Content);
                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SubterfugeFrontend.Shared.Content.Game.Network
{
    class Api
    {
        private static Api singleton = null;
        static readonly HttpClient client = new HttpClient();
        String baseUrl = "http://localhost/";

        String SESSION_ID = null;

        public Api()
        {
            if (singleton == null)
            {
                singleton = new Api();
            }
        }

        public async Task register(String username, String email, String password)
        {
            try
            {
                JObject jsonObject = new JObject();
                jsonObject.Add("username", username);
                jsonObject.Add("mail", email);
                jsonObject.Add("password", password);

                HttpContent postContent = new StringContent(jsonObject.ToString());

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/register.php", postContent);

                // Get the session ID
                Console.WriteLine(response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task login(String username, String password)
        {
            try
            {
                JObject jsonObject = new JObject();
                jsonObject.Add("username", username);
                jsonObject.Add("password", password);

                HttpContent postContent = new StringContent(jsonObject.ToString());

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/login.php", postContent);

                // Get the session ID
                Console.WriteLine(response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task joinRoom(int room_id)
        {
            try
            {
                JObject jsonObject = new JObject();
                jsonObject.Add("room_id", room_id);
                jsonObject.Add("session_id", this.SESSION_ID);

                HttpContent postContent = new StringContent(jsonObject.ToString());

                HttpResponseMessage response = await client.PostAsync(this.baseUrl + "/join_room.php", postContent);

                // Get the session ID
                Console.WriteLine(response.Content);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SubterfugeFrontend.Shared.Content.Network
{
    class NetworkRequester
    {
        private const string BASE_URL = "localhost";
        private HttpClient httpClient;

        private bool isLoggedIn = false;
        private string sessionToken;

        public NetworkRequester()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<ApplicationUser> login(string username, string password)
        {
            HttpContent requestContent = new StringContent(new {username= username, password = password}.ToString());
            HttpResponseMessage response = await httpClient.PostAsync(BASE_URL + "/login.php", requestContent);
            if (response.IsSuccessStatusCode)
            {
                String sessionToken = await response.Content.ReadAsStringAsync();

                // TODO: Make the server return the user's id
                int userId = 1;
                return new ApplicationUser(username, sessionToken, userId);
            } else
            {
                return null;
            }
        }

    }
}

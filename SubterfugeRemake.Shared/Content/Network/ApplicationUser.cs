using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Network
{
    class ApplicationUser
    {
        private string userName;
        private string sessionToken;
        private string refreshToken;
        private int playerId;

        public ApplicationUser(string username, string sessionToken, int playerId)
        {
            this.userName = userName;
            this.sessionToken = sessionToken;
            this.playerId = playerId;
        }

        public string getToken()
        {
            return this.sessionToken;
        }

        public string getUsername()
        {
            return this.userName;
        }

        public int getPlayerId()
        {
            return this.playerId;
        }

        public static ApplicationUser fromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<ApplicationUser>(jsonString);
        }

        public string toJson()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}

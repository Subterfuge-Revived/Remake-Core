using System;

namespace SubterfugeClient.Authorization
{
    public class Auth
    {
        public static string token { get; private set; } = "";
        public static bool isLoggedIn { get; private set; } = false;

        public static void Login(string userToken)
        {
            token = userToken;
            isLoggedIn = true;
        }

        public static void Logout()
        {
            token = "";
            isLoggedIn = false;
        }
    }
}
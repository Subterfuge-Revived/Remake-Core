using System;
using System.Security.Cryptography;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SuperUser
    {

        public RedisUserModel userModel;
        public readonly string password;
        
        public SuperUser(RedisUserModel user, string password)
        {
            this.userModel = user;
            this.password = password;
        }
        
    }
}
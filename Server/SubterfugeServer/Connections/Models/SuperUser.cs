using System;
using System.Security.Cryptography;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SuperUser
    {

        public DatabaseUserModel userModel;
        public readonly string password;
        
        public SuperUser(DatabaseUserModel user, string password)
        {
            this.userModel = user;
            this.password = password;
        }
        
    }
}
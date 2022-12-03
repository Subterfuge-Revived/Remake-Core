using System;
using System.Security.Cryptography;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SuperUser
    {

        public DbUserModel DbUserModel;
        public readonly string password;
        
        public SuperUser(DbUserModel dbUserModel, string password)
        {
            this.DbUserModel = dbUserModel;
            this.password = password;
        }
        
    }
}
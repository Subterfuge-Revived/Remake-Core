using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SubterfugeServerConsole.Connections.Models
{
    public class RedisUserModel
    {
        private Dictionary<RedisValue, RedisValue> userValues;

        public RedisUserModel(HashEntry[] record)
        {
            userValues = record.ToDictionary();
        }
        
        public RedisUserModel(Dictionary<RedisValue, RedisValue> record)
        {
            userValues = record;
        }

        public string GetUsername()
        {
            if (userValues.ContainsKey("username"))
            {
                return userValues["username"];
            }

            return "";
        }

        public string GetPassword()
        {
            if (userValues.ContainsKey("password"))
            {
                return userValues["password"];
            }

            return "";
        }

        public string GetUserId()
        {
            if (userValues.ContainsKey("id"))
            {
                return userValues["id"];
            }
            string guid = Guid.NewGuid().ToString();
            userValues["id"] = guid;

            return guid;
        }
        
        public string GetEmail()
        {
            if (userValues.ContainsKey("email"))
            {
                return userValues["email"];
            }

            return "";
        }
        
        public static async Task<RedisUserModel> getUser(string username)
        {
            HashEntry[] userIds = await RedisConnector.redis.HashGetAllAsync($"username:{username}");
            if (userIds.Length > 0)
            {
                return await getUser(Guid.Parse(userIds.ToDictionary()["id"]));
            }

            return null;
        }
        
        public static async Task<RedisUserModel> getUser(Guid guid)
        {
            
            HashEntry[] record = await RedisConnector.redis.HashGetAllAsync($"user:{guid.ToString()}");
            if (record.Length > 0)
            {
                return new RedisUserModel(record);
            }

            return null;
        }

        public async Task<bool> saveUser()
        {
            // Save to user list
            HashEntry[] userRecord =
            {
                new HashEntry("username", GetUsername()),
                new HashEntry("id", GetUserId()),
                new HashEntry("password", GetPassword()),
                new HashEntry("email", GetEmail()),
            };
            await RedisConnector.redis.HashSetAsync($"user:{GetUserId()}", userRecord);
            
            // Save to username lookup
            HashEntry[] usernameRecord =
            {
                new HashEntry("id", GetUserId()),
            };
            await RedisConnector.redis.HashSetAsync($"username:{GetUsername()}", usernameRecord);
            
            return true;
        }

        public static RedisUserModelBuilder newBuilder()
        {
            return new RedisUserModelBuilder();
        }
        
    }

    public class RedisUserModelBuilder
    {

        private Dictionary<RedisValue, RedisValue> record = new Dictionary<RedisValue, RedisValue>();

        public RedisUserModelBuilder setUsername(string username)
        {
            record["username"] = username;
            return this;
        }

        public RedisUserModelBuilder setPassword(string password)
        {
            record["password"] = password;
            return this;
        }
        
        public RedisUserModelBuilder setEmail(string email)
        {
            record["email"] = email;
            return this;
        }

        public RedisUserModel Build()
        {
            return new RedisUserModel(record);
        }

            
    }
}
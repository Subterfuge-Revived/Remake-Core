using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf;
using StackExchange.Redis;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections.Models
{
    public class RedisUserModel
    {
        public UserModel UserModel;

        public RedisUserModel(UserModel userModel)
        {
            this.UserModel = userModel;
        }
        
        public RedisUserModel(RedisValue byteArray)
        {
            UserModel = UserModel.Parser.ParseFrom(byteArray);
        }
        
        public RedisUserModel(AccountRegistrationRequest registration)
        {
            UserModel = new UserModel()
            {
                Id = Guid.NewGuid().ToString(),
                Username =  registration.Username,
                Email = registration.Email,
                EmailVerified = false,
                PasswordHash = JwtManager.HashPassword(registration.Password),
                Phone = registration.Phone,
                PhoneVerified = false,
                Claims = { UserClaim.User }
            };
        }

        public bool HasClaim(UserClaim claim)
        {
            return UserModel.Claims.Contains(claim);
        }
        
        public async Task<List<RedisUserModel>> GetFriends()
        {
            RedisValue[] friendIds = await RedisConnector.Redis.SetMembersAsync($"user:{UserModel.Id}:friends");
            List<RedisUserModel> friends = new List<RedisUserModel>();
            if (friendIds.Length > 0)
            {
                foreach(RedisValue value in friendIds)
                {
                    friends.Add(await GetUserFromGuid(value));
                }
            }

            return friends;
        }
        
        public async Task<List<RedisUserModel>> GetBlockedUsers()
        {
            RedisValue[] friendIds = await RedisConnector.Redis.SetMembersAsync($"user:{UserModel.Id}:blocks");
            List<RedisUserModel> friends = new List<RedisUserModel>();
            if (friendIds.Length > 0)
            {
                foreach(RedisValue value in friendIds)
                {
                    friends.Add(await GetUserFromGuid(value));
                }
            }

            return friends;
        }
        
        public async Task<List<RedisUserModel>> GetFriendRequests()
        {
            RedisValue[] friendIds = await RedisConnector.Redis.SetMembersAsync($"user:{UserModel.Id}:friendRequests");
            List<RedisUserModel> friends = new List<RedisUserModel>();
            if (friendIds.Length > 0)
            {
                foreach(RedisValue value in friendIds)
                {
                    friends.Add(await GetUserFromGuid(value));
                }
            }

            return friends;
        }
        
        public async Task<Boolean> BlockUser(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:blocks", requestingUser.UserModel.Id);
        }
        
        public async Task<Boolean> UnblockUser(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:blocks", requestingUser.UserModel.Id);
        }

        public async Task<Boolean> AddFriendRequest(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:friendRequests", requestingUser.UserModel.Id);
        }
        
        public async Task<Boolean> RemoveFriendRequest(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:friendRequests", requestingUser.UserModel.Id);
        }
        
        public async Task<Boolean> AddFriend(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:friends", requestingUser.UserModel.Id);
        }
        
        public async Task<Boolean> RemoveFriend(RedisUserModel requestingUser)
        {
            return await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:friends", requestingUser.UserModel.Id);
        }

        public static async Task<RedisUserModel> GetUserFromUsername(string username)
        {
            RedisValue userId = await RedisConnector.Redis.StringGetAsync($"username:{username}");
            if (userId.HasValue)
            {
                return await GetUserFromGuid(userId.ToString());
            }

            return null;
        }

        public static async Task<RedisUserModel> GetUserFromGuid(string guid)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(guid);
            }
            catch (FormatException e)
            {
                return null;
            }

            RedisValue user = await RedisConnector.Redis.HashGetAsync("users", new RedisValue(parsedGuid.ToString()));
            if (user.HasValue)
            {
                return new RedisUserModel(user);
            }

            return null;
        }

        public async Task<bool> SaveToDatabase()
        {
            // Save to user list
            HashEntry[] userRecord =
            {
                new HashEntry(UserModel.Id, UserModel.ToByteArray()),
            };
            await RedisConnector.Redis.HashSetAsync($"users", userRecord);
            
            // Save to username lookup
            await RedisConnector.Redis.StringSetAsync($"username:{UserModel.Username}", UserModel.Id);
            
            return true;
        }

        public User asUser()
        {
            return new User()
            {
                Id = UserModel.Id,
                Username = UserModel.Username,
            };
        }
        
    }
}
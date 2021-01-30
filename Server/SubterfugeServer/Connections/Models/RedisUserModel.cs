using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using StackExchange.Redis;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

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
            string deviceIdentifier;
            if (string.IsNullOrEmpty(registration.DeviceIdentifier))
            {
                deviceIdentifier = registration.Email;
            }
            else
            {
                deviceIdentifier = registration.DeviceIdentifier;
            }
            UserModel = new UserModel()
            {
                Id = Guid.NewGuid().ToString(),
                Username =  registration.Username,
                Email = registration.Email,
                EmailVerified = false,
                PasswordHash = JwtManager.HashPassword(registration.Password),
                Claims = { UserClaim.User },
                DeviceIdentifier = deviceIdentifier
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
        
        public async Task<ResponseStatus> BlockUser(RedisUserModel requestingUser)
        {
            if (requestingUser.HasClaim(UserClaim.Admin) || HasClaim(UserClaim.Admin))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);
            
            // When blocking a player we need to:
            // Remove players as friends if they were
            // Remove any pending friend requests
            if (await IsFriend(requestingUser))
            {
                await RemoveFriend(requestingUser);
            }

            if (await HasFriendRequestFrom(requestingUser))
            {
                await RemoveFriendRequestFrom(requestingUser);
            }

            if (await requestingUser.HasFriendRequestFrom(this))
            {
                await requestingUser.RemoveFriendRequestFrom(this);
            }
            
            await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:blocks", requestingUser.UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> UnblockUser(RedisUserModel requestingUser)
        {
            await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:blocks", requestingUser.UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsBlocked(RedisUserModel otherUser)
        {
            return await RedisConnector.Redis.SetContainsAsync($"user:{UserModel.Id}:blocks", otherUser.UserModel.Id);
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

        public async Task<bool> HasFriendRequestFrom(RedisUserModel friend)
        {
            return await RedisConnector.Redis.SetContainsAsync($"user:{UserModel.Id}:friendRequests", friend.UserModel.Id);
        }
        
        public async Task<ResponseStatus> AddFriendRequestFrom(RedisUserModel requestingUser)
        {
            await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:friendRequests", requestingUser.UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> RemoveFriendRequestFrom(RedisUserModel requestingUser)
        {
            await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:friendRequests", requestingUser.UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        /**
         * This method assumes that the passed in user has sent you a request already.
         */
        public async Task<ResponseStatus> AcceptFriendRequestFrom(RedisUserModel requestingUser)
        {
            await RemoveFriendRequestFrom(requestingUser);
            await AddFriend(requestingUser);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        private async Task<ResponseStatus> AddFriend(RedisUserModel requestingUser)
        {
            await RedisConnector.Redis.SetAddAsync($"user:{UserModel.Id}:friends", requestingUser.UserModel.Id);
            await RedisConnector.Redis.SetAddAsync($"user:{requestingUser.UserModel.Id}:friends", UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> RemoveFriend(RedisUserModel requestingUser)
        {
            await RedisConnector.Redis.SetRemoveAsync($"user:{UserModel.Id}:friends", requestingUser.UserModel.Id);
            await RedisConnector.Redis.SetRemoveAsync($"user:{requestingUser.UserModel.Id}:friends", UserModel.Id);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsFriend(RedisUserModel friend)
        {
            return await RedisConnector.Redis.SetContainsAsync($"user:{UserModel.Id}:friends", friend.UserModel.Id);
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
            catch (FormatException)
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

        public async Task<List<RedisRoomModel>> GetActiveRooms()
        {
            List<RedisRoomModel> userGames = new List<RedisRoomModel>();
            if (this.HasClaim(UserClaim.Admin))
            {
                // Admins can see every room.
                HashEntry[] games = await RedisConnector.Redis.HashGetAllAsync("games");
                foreach (var game in games)
                {
                    userGames.Add(new RedisRoomModel(game.Value));
                }

                return userGames;
            }
            
            RedisValue[] roomIds = await RedisConnector.Redis.SetMembersAsync($"users:{UserModel.Id}:games");
            foreach(var roomId in roomIds)
            {
                userGames.Add(await RedisRoomModel.GetRoomFromGuid(roomId));
            }
            return userGames;
        }

        public User asUser()
        {
            return new User()
            {
                Id = UserModel.Id,
                Username = UserModel.Username,
            };
        }

        public static async Task<SuperUser> CreateSuperUser()
        {
            String password = Guid.NewGuid().ToString();
            Console.WriteLine($"Password: {password}");
            RedisUserModel userModel = new RedisUserModel(new UserModel()
            {
                Id = Guid.NewGuid().ToString(),
                Username =  "SuperUser",
                Email = "SuperUser",
                EmailVerified = true,
                PasswordHash = JwtManager.HashPassword(password),
                Claims = { UserClaim.User, UserClaim.Admin, UserClaim.Dev, UserClaim.EmailVerified }
            });
            await userModel.SaveToDatabase();
            return new SuperUser(userModel, password);
        }
        
    }
}
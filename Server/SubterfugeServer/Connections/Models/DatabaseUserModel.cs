using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class DatabaseUserModel
    {
        public UserModel UserModel;

        public DatabaseUserModel(UserModel userModel)
        {
            this.UserModel = userModel;
        }
        
        public DatabaseUserModel(AccountRegistrationRequest registration)
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
        
        public async Task<List<FriendModel>> GetFriends()
        {
            List<FriendModel> friends = MongoConnector.getFriendCollection()
                .Find(it => it.PlayerId == UserModel.Id && it.FriendStatus == FriendStatus.StatusFriends).ToList();
            return friends;
        }
        
        public async Task<List<FriendModel>> GetBlockedUsers()
        {
            List<FriendModel> blockedUsers = MongoConnector.getFriendCollection()
                .Find(it => it.PlayerId == UserModel.Id && it.FriendStatus == FriendStatus.StatusBlocked).ToList();
            return blockedUsers;
        }
        
        public async Task<ResponseStatus> BlockUser(DatabaseUserModel requestingUser)
        {
            if (requestingUser.HasClaim(UserClaim.Admin) || HasClaim(UserClaim.Admin))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);
            
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusBlocked);
            MongoConnector.getFriendCollection().UpdateOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> UnblockUser(DatabaseUserModel requestingUser)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusNoRelation);
            MongoConnector.getFriendCollection().UpdateOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsBlocked(DatabaseUserModel otherUser)
        {
            FriendModel blockedPlayer = MongoConnector.getFriendCollection().Find(it =>
                it.FriendStatus == FriendStatus.StatusBlocked &&
                (it.PlayerId == UserModel.Id && it.FriendId == otherUser.UserModel.Id ||
                 it.FriendId == UserModel.Id && it.PlayerId == otherUser.UserModel.Id)
            ).FirstOrDefault();
            if (blockedPlayer != null)
            {
                return true;
            }
            return false;
        }
        
        public async Task<List<FriendModel>> GetFriendRequests()
        {
            List<FriendModel> friendRequests = MongoConnector.getFriendCollection()
                .Find(it => it.FriendId == UserModel.Id && it.FriendStatus == FriendStatus.StatusPending).ToList();
            return friendRequests;
        }

        public async Task<bool> HasFriendRequestFrom(DatabaseUserModel friend)
        {
            FriendModel friendRequest = MongoConnector.getFriendCollection().Find(it =>
                it.FriendStatus == FriendStatus.StatusPending &&
                (it.FriendId == UserModel.Id && it.PlayerId == friend.UserModel.Id)).FirstOrDefault();
            if (friendRequest == null)
            {
                return false;
            }
            return true;
        }
        
        public async Task<ResponseStatus> AddFriendRequestFrom(DatabaseUserModel requestingUser)
        {
            MongoConnector.getFriendCollection().InsertOne(new FriendModel()
            {
                PlayerId = UserModel.Id,
                FriendId = requestingUser.UserModel.Id,
                FriendStatus = FriendStatus.StatusPending,
                UnixTimeCreated = DateTime.UtcNow.ToFileTimeUtc(),
            });
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> RemoveFriendRequestFrom(DatabaseUserModel requestingUser)
        {
            MongoConnector.getFriendCollection().DeleteOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id));
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        /**
         * This method assumes that the passed in user has sent you a request already.
         */
        public async Task<ResponseStatus> AcceptFriendRequestFrom(DatabaseUserModel requestingUser)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusFriends);
            MongoConnector.getFriendCollection().UpdateOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<ResponseStatus> RemoveFriend(DatabaseUserModel requestingUser)
        {
            MongoConnector.getFriendCollection().DeleteOne(it =>
                (it.PlayerId == UserModel.Id && it.FriendId == requestingUser.UserModel.Id) ||
                (it.FriendId == UserModel.Id && it.PlayerId == requestingUser.UserModel.Id)
            );
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsFriend(DatabaseUserModel friend)
        {
            FriendModel friendModel = MongoConnector.getFriendCollection().Find(it =>
                it.FriendStatus == FriendStatus.StatusFriends && (
                    (it.PlayerId == this.UserModel.Id && it.FriendId == friend.UserModel.Id) ||
                    (it.FriendId == this.UserModel.Id && it.PlayerId == friend.UserModel.Id))).FirstOrDefault();
            if (friendModel == null)
            {
                return false;
            }

            return true;
        }

        public static async Task<DatabaseUserModel> GetUserFromUsername(string username)
        {
            UserModel user = MongoConnector.getUserCollection().Find(it => it.Username == username).FirstOrDefault();
            
            if (user != null)
            {
                return new DatabaseUserModel(user);
            }

            return null;
        }

        public static async Task<DatabaseUserModel> GetUserFromGuid(string guid)
        {
            UserModel user = MongoConnector.getUserCollection().Find(it => it.Id == guid).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            return new DatabaseUserModel(user);
        }

        public async Task<bool> SaveToDatabase()
        {
            // Save to user collection
            await MongoConnector.getUserCollection().InsertOneAsync(UserModel);
            return true;
        }

        public async Task<List<RoomModel>> GetActiveRooms()
        {
            List<RoomModel> userGames = new List<RoomModel>();
            if (this.HasClaim(UserClaim.Admin))
            {
                // Admins can see every room.
                userGames.AddRange(MongoConnector.getGameRoomCollection().Find(new BsonDocument()).ToList());
                return userGames;
            }
            
            // Otherwise, only show public games.
            userGames.AddRange(MongoConnector.getGameRoomCollection().Find(it => it.RoomStatus == RoomStatus.Open).ToList());
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
            DatabaseUserModel userModel = new DatabaseUserModel(new UserModel()
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
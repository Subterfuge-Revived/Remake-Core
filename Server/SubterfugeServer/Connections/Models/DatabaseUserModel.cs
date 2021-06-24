using System;
using System.Collections.Generic;
using System.Linq;
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
            List<FriendModel> friends = MongoConnector.GetFriendCollection()
                .Find(it => 
                    (it.PlayerId == UserModel.Id || it.FriendId == UserModel.Id) 
                    && it.FriendStatus == FriendStatus.StatusFriends).ToList();
            return friends;
        }
        
        public async Task<List<FriendModel>> GetBlockedUsers()
        {
            List<FriendModel> blockedUsers = MongoConnector.GetFriendCollection()
                .Find(it => (it.PlayerId == UserModel.Id)
                            && it.FriendStatus == FriendStatus.StatusBlocked).ToList();
            return blockedUsers;
        }
        
        public async Task<ResponseStatus> BlockUser(DatabaseUserModel requestingUser)
        {
            if (requestingUser.HasClaim(UserClaim.Admin) || HasClaim(UserClaim.Admin))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);
            
            // Check if a relationship already exists for the two players.
            var relationExists = MongoConnector.GetFriendCollection()
                .Find(it => (it.PlayerId == UserModel.Id && it.FriendId == requestingUser.UserModel.Id))
                .CountDocuments() != 0;
            
            // Remove all pre-existing relations
            await MongoConnector.GetFriendCollection().DeleteManyAsync(it =>
                (it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id) ||
                (it.PlayerId == requestingUser.UserModel.Id && it.FriendId == UserModel.Id));
            
            await MongoConnector.GetFriendCollection().InsertOneAsync(new FriendModel
            {
                Id = Guid.NewGuid().ToString(),
                FriendId = requestingUser.UserModel.Id,
                FriendStatus = FriendStatus.StatusBlocked,
                PlayerId = UserModel.Id,
                UnixTimeCreated = DateTime.UtcNow.ToFileTimeUtc(),
            });

            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> UnblockUser(DatabaseUserModel requestingUser)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusNoRelation);
            MongoConnector.GetFriendCollection().UpdateOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsBlocked(DatabaseUserModel otherUser)
        {
            FriendModel blockedPlayer = MongoConnector.GetFriendCollection().Find(it =>
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
            List<FriendModel> friendRequests = MongoConnector.GetFriendCollection()
                .Find(it => it.PlayerId == UserModel.Id && it.FriendStatus == FriendStatus.StatusPending).ToList();
            return friendRequests;
        }

        public async Task<bool> HasFriendRequestFrom(DatabaseUserModel friend)
        {
            FriendModel friendRequest = MongoConnector.GetFriendCollection().Find(it =>
                it.FriendStatus == FriendStatus.StatusPending &&
                (it.PlayerId == UserModel.Id && it.FriendId == friend.UserModel.Id)).FirstOrDefault();
            if (friendRequest == null)
            {
                return false;
            }
            return true;
        }
        
        public async Task<ResponseStatus> AddFriendRequestFrom(DatabaseUserModel requestingUser)
        {
            MongoConnector.GetFriendCollection().InsertOne(new FriendModel()
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
            MongoConnector.GetFriendCollection().DeleteOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id));
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        /**
         * This method assumes that the passed in user has sent you a request already.
         */
        public async Task<ResponseStatus> AcceptFriendRequestFrom(DatabaseUserModel requestingUser)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusFriends);
            MongoConnector.GetFriendCollection().UpdateOne((it => it.FriendId == requestingUser.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<ResponseStatus> RemoveFriend(DatabaseUserModel requestingUser)
        {
            MongoConnector.GetFriendCollection().DeleteOne(it =>
                (it.PlayerId == UserModel.Id && it.FriendId == requestingUser.UserModel.Id) ||
                (it.FriendId == UserModel.Id && it.PlayerId == requestingUser.UserModel.Id)
            );
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsFriend(DatabaseUserModel friend)
        {
            FriendModel friendModel = MongoConnector.GetFriendCollection().Find(it =>
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
            UserModel user = MongoConnector.GetUserCollection().Find(it => it.Username == username).FirstOrDefault();
            
            if (user != null)
            {
                return new DatabaseUserModel(user);
            }

            return null;
        }

        public static async Task<DatabaseUserModel> GetUserFromGuid(string guid)
        {
            UserModel user = MongoConnector.GetUserCollection().Find(it => it.Id == guid).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            return new DatabaseUserModel(user);
        }

        public async Task<bool> SaveToDatabase()
        {
            // Save to user collection
            await MongoConnector.GetUserCollection().InsertOneAsync(UserModel);
            return true;
        }

        public async Task<List<DatabaseRoomModel>> GetActiveRooms()
        {
            List<DatabaseRoomModel> userGames = new List<DatabaseRoomModel>();
            if (this.HasClaim(UserClaim.Admin))
            {
                // Admins can see every room.
                userGames.AddRange(MongoConnector.GetGameRoomCollection().Find(new BsonDocument()).ToList().Select(it => new DatabaseRoomModel(it)));
                return userGames;
            }
            
            // Otherwise, only show public games.
            userGames.AddRange(MongoConnector.GetGameRoomCollection().Find(it => it.RoomStatus == RoomStatus.Open).ToList().Select(it => new DatabaseRoomModel(it)));
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
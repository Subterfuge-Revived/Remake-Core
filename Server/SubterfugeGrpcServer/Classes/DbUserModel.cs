using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class DbUserModel
    {
        public UserModel UserModel;

        public DbUserModel(UserModel userModel)
        {
            this.UserModel = userModel;
        }
        
        public DbUserModel(AccountRegistrationRequest registration)
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
            return (await MongoConnector.GetFriendCollection()
                .FindAsync(it => 
                    (it.PlayerId == UserModel.Id || it.FriendId == UserModel.Id) 
                    && it.FriendStatus == FriendStatus.StatusFriends))
                .ToList();
        }
        
        public async Task<List<FriendModel>> GetBlockedUsers()
        {
            List<FriendModel> blockedUsers = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => (it.PlayerId == UserModel.Id)
                            && it.FriendStatus == FriendStatus.StatusBlocked))
                .ToList();
            
            return blockedUsers;
        }
        
        public async Task<ResponseStatus> BlockUser(DbUserModel requestingDbUserModel)
        {
            if (requestingDbUserModel.HasClaim(UserClaim.Admin) || HasClaim(UserClaim.Admin))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);
            
            // Check if a relationship already exists for the two players.
            var relationExists = MongoConnector.GetFriendCollection()
                .Find(it => (it.PlayerId == UserModel.Id && it.FriendId == requestingDbUserModel.UserModel.Id))
                .CountDocuments() != 0;
            
            // Remove all pre-existing relations
            await MongoConnector.GetFriendCollection().DeleteManyAsync(it =>
                (it.FriendId == requestingDbUserModel.UserModel.Id && it.PlayerId == UserModel.Id) ||
                (it.PlayerId == requestingDbUserModel.UserModel.Id && it.FriendId == UserModel.Id));
            
            await MongoConnector.GetFriendCollection().InsertOneAsync(new FriendModel
            {
                Id = Guid.NewGuid().ToString(),
                FriendId = requestingDbUserModel.UserModel.Id,
                FriendStatus = FriendStatus.StatusBlocked,
                PlayerId = UserModel.Id,
                UnixTimeCreated = DateTime.UtcNow.ToFileTimeUtc(),
            });

            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> UnblockUser(DbUserModel requestingDbUserModel)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusNoRelation);
            await MongoConnector.GetFriendCollection().UpdateOneAsync((it => it.FriendId == requestingDbUserModel.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsBlocked(DbUserModel otherDbUserModel)
        {
            var blockedPlayer = (await MongoConnector.GetFriendCollection()
                .FindAsync(it =>
                    it.FriendStatus == FriendStatus.StatusBlocked && 
                        (it.PlayerId == UserModel.Id && it.FriendId == otherDbUserModel.UserModel.Id ||
                        it.FriendId == UserModel.Id && it.PlayerId == otherDbUserModel.UserModel.Id)
                ))
                .FirstOrDefault();
            
            return blockedPlayer != null;
        }
        
        public async Task<List<FriendModel>> GetFriendRequests()
        {
            List<FriendModel> friendRequests = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => it.PlayerId == UserModel.Id && it.FriendStatus == FriendStatus.StatusPending))
                .ToList();
            
            return friendRequests;
        }

        public async Task<bool> HasFriendRequestFrom(DbUserModel friend)
        {
            FriendModel friendRequest = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => 
                    it.FriendStatus == FriendStatus.StatusPending &&
                    (it.PlayerId == UserModel.Id && it.FriendId == friend.UserModel.Id)))
                .ToList()
                .FirstOrDefault();
            
            if (friendRequest == null)
            {
                return false;
            }
            return true;
        }
        
        public async Task<ResponseStatus> AddFriendRequestFrom(DbUserModel requestingDbUserModel)
        {
            await MongoConnector.GetFriendCollection().InsertOneAsync(new FriendModel()
            {
                PlayerId = UserModel.Id,
                FriendId = requestingDbUserModel.UserModel.Id,
                FriendStatus = FriendStatus.StatusPending,
                UnixTimeCreated = DateTime.UtcNow.ToFileTimeUtc(),
            });
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> RemoveFriendRequestFrom(DbUserModel requestingDbUserModel)
        {
            await MongoConnector.GetFriendCollection().DeleteOneAsync((it => it.FriendId == requestingDbUserModel.UserModel.Id && it.PlayerId == UserModel.Id));
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        /**
         * This method assumes that the passed in user has sent you a request already.
         */
        public async Task<ResponseStatus> AcceptFriendRequestFrom(DbUserModel requestingDbUserModel)
        {
            var update = Builders<FriendModel>.Update.Set(it => it.FriendStatus, FriendStatus.StatusFriends);
            await MongoConnector.GetFriendCollection()
                .UpdateOneAsync(
                    (it => it.FriendId == requestingDbUserModel.UserModel.Id &&
                           it.PlayerId == UserModel.Id),
                    update);
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<ResponseStatus> RemoveFriend(DbUserModel requestingDbUserModel)
        {
            await MongoConnector.GetFriendCollection().DeleteOneAsync(it =>
                (it.PlayerId == UserModel.Id && it.FriendId == requestingDbUserModel.UserModel.Id) ||
                (it.FriendId == UserModel.Id && it.PlayerId == requestingDbUserModel.UserModel.Id)
            );
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsFriend(DbUserModel friend)
        {
            FriendModel friendModel = (await MongoConnector.GetFriendCollection()
                .FindAsync(it =>
                    it.FriendStatus == FriendStatus.StatusFriends && (
                        (it.PlayerId == this.UserModel.Id && it.FriendId == friend.UserModel.Id) ||
                        (it.FriendId == this.UserModel.Id && it.PlayerId == friend.UserModel.Id)
                    )
                ))
                .ToList()
                .FirstOrDefault();
            
            if (friendModel == null)
            {
                return false;
            }

            return true;
        }

        public static async Task<DbUserModel> GetUserFromUsername(string username)
        {
            var user = (await MongoConnector.GetUserCollection()
                .FindAsync(it => it.Username == username))
                .ToList()
                .FirstOrDefault();
            
            return user != null ? new DbUserModel(user.ToProto()) : null;
        }
        
        public async Task<List<SpecialistConfigurationModel>> GetSpecialistConfigurations()
        {
            return (await MongoConnector.GetSpecialistCollection()
                .FindAsync(it => it.Creator.Id == UserModel.Id))
                .ToList()
                .Select(it => new SpecialistConfigurationModel(it.ToProto()))
                .ToList();
        }
        
        public async Task<List<SpecialistPackageModel>> GetSpecialistPackages()
        {
            return (await MongoConnector.GetSpecialistPackageCollection()
                .FindAsync(it => it.Creator.Id == UserModel.Id))
                .ToList()
                .Select(it => new SpecialistPackageModel(it.ToProto()))
                .ToList();
        }

        public static async Task<DbUserModel> GetUserFromGuid(string guid)
        {
            UserModelMapper user = (await MongoConnector.GetUserCollection()
                .FindAsync(it => it.Id == guid))
                .FirstOrDefault();
            
            if (user == null)
            {
                return null;
            }
            return new DbUserModel(user.ToProto());
        }

        public async Task<bool> SaveToDatabase()
        {
            // Save to user collection
            await MongoConnector.GetUserCollection().InsertOneAsync(new UserModelMapper(UserModel));
            return true;
        }

        public async Task<List<Room>> GetActiveRooms()
        {
            List<Room> userGames = new List<Room>();
            if (this.HasClaim(UserClaim.Admin))
            {
                // Admins can see every room.
                userGames.AddRange((await MongoConnector.GetGameRoomCollection()
                    .FindAsync(new BsonDocument()))
                    .ToList()
                    .Select(it => new Room(it.ToProto())
                ));
                return userGames;
            }
            
            // Otherwise, only show public games.
            userGames.AddRange((await MongoConnector.GetGameRoomCollection()
                .FindAsync(it => it.PlayersInGame.Contains(AsUser())))
                .ToList()
                .Select(it => new Room(it.ToProto())
            ));
            return userGames;
        }

        public User AsUser()
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
            DbUserModel dbUserModel = new DbUserModel(new UserModel()
            {
                Id = Guid.NewGuid().ToString(),
                Username =  "SuperUser",
                Email = "SuperUser",
                EmailVerified = true,
                PasswordHash = JwtManager.HashPassword(password),
                Claims = { UserClaim.User, UserClaim.Admin, UserClaim.Dev, UserClaim.EmailVerified }
            });
            await dbUserModel.SaveToDatabase();
            return new SuperUser(dbUserModel, password);
        }
        
    }
}
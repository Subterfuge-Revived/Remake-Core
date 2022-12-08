using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
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
                Claims = new[] { UserClaim.User },
                DeviceIdentifier = deviceIdentifier
            };
        }

        public bool HasClaim(UserClaim claim)
        {
            return UserModel.Claims.Contains(claim);
        }
        
        public async Task<List<Friend>> GetFriends()
        {
            return (await MongoConnector.GetFriendCollection()
                .FindAsync(it => 
                    (it.PlayerId == UserModel.Id || it.FriendId == UserModel.Id) 
                    && it.RelationshipStatus == RelationshipStatus.Friends))
                .ToList();
        }
        
        public async Task<List<Friend>> GetBlockedUsers()
        {
            List<Friend> blockedUsers = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => (it.PlayerId == UserModel.Id)
                            && it.RelationshipStatus == RelationshipStatus.Blocked))
                .ToList();
            
            return blockedUsers;
        }
        
        public async Task<ResponseStatus> BlockUser(DbUserModel requestingDbUserModel)
        {
            if (requestingDbUserModel.HasClaim(UserClaim.Administrator) || HasClaim(UserClaim.Administrator))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);
            
            // Check if a relationship already exists for the two players.
            var relationExists = MongoConnector.GetFriendCollection()
                .Find(it => (it.PlayerId == UserModel.Id && it.FriendId == requestingDbUserModel.UserModel.Id))
                .CountDocuments() != 0;
            
            // Remove all pre-existing relations
            await MongoConnector.GetFriendCollection().DeleteManyAsync(it =>
                (it.FriendId == requestingDbUserModel.UserModel.Id && it.PlayerId == UserModel.Id) ||
                (it.PlayerId == requestingDbUserModel.UserModel.Id && it.FriendId == UserModel.Id));
            
            await MongoConnector.GetFriendCollection().InsertOneAsync(new Friend
            {
                Id = Guid.NewGuid().ToString(),
                FriendId = requestingDbUserModel.UserModel.Id,
                RelationshipStatus = RelationshipStatus.Blocked,
                PlayerId = UserModel.Id,
                UnixTimeCreated = DateTime.UtcNow.ToFileTimeUtc(),
            });

            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<ResponseStatus> UnblockUser(DbUserModel requestingDbUserModel)
        {
            var update = Builders<Friend>.Update.Set(it => it.RelationshipStatus, RelationshipStatus.NoRelation);
            await MongoConnector.GetFriendCollection().UpdateOneAsync((it => it.FriendId == requestingDbUserModel.UserModel.Id && it.PlayerId == UserModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }
        
        public async Task<bool> IsBlocked(DbUserModel otherDbUserModel)
        {
            var blockedPlayer = (await MongoConnector.GetFriendCollection()
                .FindAsync(it =>
                    it.RelationshipStatus == RelationshipStatus.Blocked && 
                        (it.PlayerId == UserModel.Id && it.FriendId == otherDbUserModel.UserModel.Id ||
                        it.FriendId == UserModel.Id && it.PlayerId == otherDbUserModel.UserModel.Id)
                ))
                .FirstOrDefault();
            
            return blockedPlayer != null;
        }
        
        public async Task<List<Friend>> GetFriendRequests()
        {
            List<Friend> friendRequests = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => it.PlayerId == UserModel.Id && it.RelationshipStatus == RelationshipStatus.Pending))
                .ToList();
            
            return friendRequests;
        }

        public async Task<bool> HasFriendRequestFrom(DbUserModel friend)
        {
            Friend friendRequest = (await MongoConnector.GetFriendCollection()
                .FindAsync(it => 
                    it.RelationshipStatus == RelationshipStatus.Pending &&
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
            await MongoConnector.GetFriendCollection().InsertOneAsync(new Friend()
            {
                PlayerId = UserModel.Id,
                FriendId = requestingDbUserModel.UserModel.Id,
                RelationshipStatus = RelationshipStatus.Pending,
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
            var update = Builders<Friend>.Update.Set(it => it.RelationshipStatus, RelationshipStatus.Friends);
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
            Friend friendModel = (await MongoConnector.GetFriendCollection()
                .FindAsync(it =>
                    it.RelationshipStatus == RelationshipStatus.Friends && (
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
            
            return user != null ? new DbUserModel(user) : null;
        }
        
        public async Task<List<SpecialistConfigurationModel>> GetSpecialistConfigurations()
        {
            return (await MongoConnector.GetSpecialistCollection()
                .FindAsync(it => it.Creator.Id == UserModel.Id))
                .ToList()
                .Select(it => new SpecialistConfigurationModel(it))
                .ToList();
        }
        
        public async Task<List<SpecialistPackageModel>> GetSpecialistPackages()
        {
            return (await MongoConnector.GetSpecialistPackageCollection()
                .FindAsync(it => it.Creator.Id == UserModel.Id))
                .ToList()
                .Select(it => new SpecialistPackageModel(it))
                .ToList();
        }

        public static async Task<DbUserModel?> GetUserFromGuid(string guid)
        {
            UserModel user = (await MongoConnector.GetUserCollection()
                .FindAsync(it => it.Id == guid))
                .FirstOrDefault();
            
            if (user == null)
            {
                return null;
            }
            return new DbUserModel(user);
        }

        public async Task<bool> SaveToDatabase()
        {
            // Save to user collection
            await MongoConnector.GetUserCollection().InsertOneAsync(UserModel);
            return true;
        }

        public async Task<List<Room>> GetActiveRooms()
        {
            List<Room> userGames = new List<Room>();
            if (this.HasClaim(UserClaim.Administrator))
            {
                // Admins can see every room.
                userGames.AddRange((await MongoConnector.GetGameRoomCollection()
                    .FindAsync(new BsonDocument()))
                    .ToList()
                    .Select(it => new Room(it)
                ));
                return userGames;
            }
            
            // Otherwise, only show public games.
            userGames.AddRange((await MongoConnector.GetGameRoomCollection()
                .FindAsync(it => it.PlayersInLobby.Contains(AsUser())))
                .ToList()
                .Select(it => new Room(it)
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
                Claims = new[] { UserClaim.User, UserClaim.Administrator, UserClaim.Moderator, UserClaim.EmailVerified }
            });
            await dbUserModel.SaveToDatabase();
            return new SuperUser(dbUserModel, password);
        }
        
    }
}
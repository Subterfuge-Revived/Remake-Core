using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;

namespace SubterfugeDatabaseProvider.Models;

public class DbGameLobbyConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime TimeStarted { get; set; }
    public DateTime TimeEnded { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public RoomStatus RoomStatus { get; set; } = RoomStatus.Open;
    public User Creator { get; set; }
    public GameSettings GameSettings { get; set; }
    public MapConfiguration MapConfiguration { get; set; }
    public string RoomName { get; set; }
    public string GameVersion { get; set; } = "0.0.1";
    public List<string> PlayerIdsInLobby { get; set; }

    public static DbGameLobbyConfiguration FromRequest(CreateRoomRequest request, User creator)
    {
        return new DbGameLobbyConfiguration()
        {
            Creator = creator,
            GameSettings = request.GameSettings,
            MapConfiguration = request.MapConfiguration,
            RoomName = request.RoomName,
            PlayerIdsInLobby = new List<string>() { creator.Id }
        };
    }

    public async Task<List<User>> GetPlayersInLobby(IDatabaseCollection<DbUserModel> _dbUserCollection)
    {
        return (await _dbUserCollection.Query()
                .Where(user => PlayerIdsInLobby.Contains(user.Id))
                .ToListAsync())
            .Select(it => it.ToUser())
            .ToList();
    }

    public async Task<GameConfiguration> ToGameConfiguration(IDatabaseCollection<DbUserModel> _dbUserCollection)
    {
        var usersInGame = (await GetPlayersInLobby(_dbUserCollection));
            
        return new GameConfiguration()
        {
            Id = Id,
            RoomName = RoomName,
            RoomStatus = RoomStatus,
            Creator = Creator,
            GameSettings = GameSettings,
            GameVersion = GameVersion,
            PlayersInLobby = usersInGame,
            TimeCreated = TimeCreated,
            TimeStarted = TimeStarted,
            ExpiresAt = ExpiresAt,
            MapConfiguration = MapConfiguration
        };
    }
    
}
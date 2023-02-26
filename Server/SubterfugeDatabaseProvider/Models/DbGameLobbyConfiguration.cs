using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Database.Models;

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
    public GameVersion GameVersion { get; set; } = GameVersion.ALPHA01;
    public List<string> PlayerIdsInLobby { get; set; }

    public Dictionary<string, List<SpecialistTypeId>> PlayerSpecialistDecks { get; set; } = new Dictionary<string, List<SpecialistTypeId>>();

    public static DbGameLobbyConfiguration FromRequest(CreateRoomRequest request, User creator)
    {
        var lobby = new DbGameLobbyConfiguration()
        {
            Creator = creator,
            GameSettings = request.GameSettings,
            MapConfiguration = request.MapConfiguration,
            RoomName = request.RoomName,
            PlayerIdsInLobby = new List<string>() { creator.Id },
        };
        lobby.PlayerSpecialistDecks.Add(creator.Id, request.CreatorSpecialistDeck);
        return lobby;
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
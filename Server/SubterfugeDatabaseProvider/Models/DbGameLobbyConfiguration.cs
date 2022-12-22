using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbGameLobbyConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime TimeStarted { get; set; }
    public DateTime TimeEnded { get; set; }
    public DateTime ExpiresAt { get; set; }
    public RoomStatus RoomStatus { get; set; } = RoomStatus.Open;
    public User Creator { get; set; }
    public GameSettings GameSettings { get; set; }
    public MapConfiguration MapConfiguration { get; set; }
    public string RoomName { get; set; }
    public string GameVersion { get; set; } = "0.0.1";
    public List<User> PlayersInLobby { get; set; }

    public static DbGameLobbyConfiguration FromRequest(CreateRoomRequest request, User creator)
    {
        return new DbGameLobbyConfiguration()
        {
            Creator = creator,
            GameSettings = request.GameSettings,
            MapConfiguration = request.MapConfiguration,
            RoomName = request.RoomName,
            PlayersInLobby = new List<User>() { creator }
        };
    }

    public GameConfiguration ToGameConfiguration()
    {
        return new GameConfiguration()
        {
            Id = Id,
            RoomName = RoomName,
            RoomStatus = RoomStatus,
            Creator = Creator,
            GameSettings = GameSettings,
            GameVersion = GameVersion,
            PlayersInLobby = PlayersInLobby,
            TimeCreated = TimeCreated,
            TimeStarted = TimeStarted,
            ExpiresAt = ExpiresAt,
            MapConfiguration = MapConfiguration
        };
    }
    
}
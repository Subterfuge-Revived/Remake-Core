namespace SubterfugeCore.Players
{
    public class Player
    {
        string playerName;
        int playerId;

        public Player(int playerId)
        {
            this.playerId = playerId;
        }

        public int getId()
        {
            return this.playerId;
        }

        public string getPlayerName()
        {
            return this.playerName;
        }
    }
}

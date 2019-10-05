namespace SubterfugeCore.Components
{
    public interface IOwnable
    {
        Players.Player getOwner();
        void setOwner(Players.Player newOwner);
    }
}

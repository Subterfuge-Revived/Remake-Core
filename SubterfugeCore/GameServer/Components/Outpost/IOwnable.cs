namespace SubterfugeCore.Components
{
    interface IOwnable : Component
    {
        Players.Player getOwner();
        void setOwner(Players.Player newOwner);
    }
}

namespace SubterfugeCore.Core.GameEvents
{
    public interface IReversible
    {
        bool forwardAction();
        bool backwardAction();
    }
}
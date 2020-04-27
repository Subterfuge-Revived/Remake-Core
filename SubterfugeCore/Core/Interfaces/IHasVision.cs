namespace SubterfugeCore.Core.Interfaces
{
    public interface IHasVision
    {
        bool IsTargetVisible(ITargetable target);
        float GetVisionRange();
        void SetVisionRange(float newRange);
    }
}
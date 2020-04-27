namespace SubterfugeCore.Core.Interfaces
{
    public interface IHasVision
    {
        bool IsVisible(ITargetable target);
        float GetVisionRange();
        void SetVisionRange(float newRange);
    }
}
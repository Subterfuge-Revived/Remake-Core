using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can be shielded
    /// </summary>
    public interface IShieldable
    {
        ShieldManager GetShieldManager();
    }
}

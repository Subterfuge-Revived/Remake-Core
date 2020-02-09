using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Components.Outpost;

namespace SubterfugeCore.Core.Interfaces.Outpost
{
    /// <summary>
    /// Anything that can perform combat
    /// </summary>
    public interface ICombatable : IDrillerCarrier, ISpecialistCarrier, ITargetable
    {
    }
}

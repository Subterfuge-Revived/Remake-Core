using Microsoft.Xna.Framework;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Components.Outpost
{
    public interface ITargetable : ILocation
    {
        Vector2 getTargetLocation(Vector2 targetFrom, double speed);
    }
}

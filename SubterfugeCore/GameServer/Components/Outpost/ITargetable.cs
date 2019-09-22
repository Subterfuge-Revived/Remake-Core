using Microsoft.Xna.Framework;
using SubterfugeCore.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Components.Outpost
{
    public interface ITargetable
    {
        Vector2 getTargetLocation(Vector2 targetFrom, double speed);
    }
}

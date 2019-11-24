using Microsoft.Xna.Framework;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Components.Outpost
{
    public interface ILaunchable : ILocation
    {
        Sub launchSub(int drillerCount, ITargetable destination);
        void undoLaunch(Sub sub);
    }
}

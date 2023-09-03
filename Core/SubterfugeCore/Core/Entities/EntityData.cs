using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities
{
    public class EntityData
    {
        public int DrillerCount { get; set; } = 0;
        public int ShieldCount { get; set; } = 0;
        public Player Owner { get; set; } = null;
        public List<SpecialistTypeId> Specialists
        {
            get;
            set;
        } = null;
        
    }
}
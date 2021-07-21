using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class SubCombatEffect : GameStateEffect
    {

        private readonly ICombatable _attacker;
        private readonly ICombatable _reciever;

        SubCombatEffect(ICombatable attacker, ICombatable reciever)
        {
            _attacker = attacker;
            _reciever = reciever;
        }
        public override List<IReversible> GetEvents()
        {
            if (_attacker.GetOwner() == _reciever.GetOwner())
            {
                return FriendlySubArriveEffects();
            }

            return CombatEffects();
        }

        public List<IReversible> FriendlySubArriveEffects()
        {
            return new List<IReversible>()
            {
                new AlterDrillerEffect(_reciever, _attacker.GetDrillerCount())
            };
        }
        
        public List<IReversible> CombatEffects()
        {
            return new List<IReversible>()
            {
                new AlterDrillerEffect(_reciever, _attacker.GetDrillerCount())
            };
        }
    }
}
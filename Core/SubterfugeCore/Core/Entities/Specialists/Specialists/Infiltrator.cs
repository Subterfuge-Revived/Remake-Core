using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Infiltrator : Specialist
    {
        public Infiltrator(Player owner, Priority priority) : base(owner, priority)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            entity.GetComponent<PositionManager>().OnPreCombat += OnPreCombat;
            if (GetLevel() >= 2)
            {
                entity.GetComponent<PositionManager>().OnPostCombat += OnPostCombat;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            entity.GetComponent<PositionManager>().OnPreCombat -= OnPreCombat;
            if (GetLevel() >= 2)
            {
                entity.GetComponent<PositionManager>().OnPostCombat -= OnPostCombat;
            }
        }

        public override SpecialistIds GetSpecialistId()
        {
            return SpecialistIds.Infiltrator;
        }
        
        private void OnPreCombat(object positionManager, OnPreCombatEventArgs preCombatEventArgs)
        {
            // TODO: Figure out combat effects.
        }

        private void OnPostCombat(object positionManager, PostCombatEventArgs postCombatEventArgs)
        {
            if (postCombatEventArgs.Direction == TimeMachineDirection.FORWARD)
            {
                if (GetLevel() >= 2)
                {
                    if (postCombatEventArgs.WinningPlayer.PlayerInstance == _owner.PlayerInstance && postCombatEventArgs.WasOutpostCaptured)
                    {
                        RestoreLostShields(postCombatEventArgs.SurvivingEntity);
                    }
                }
            }
        }

        private void RestoreLostShields(IEntity RestoreLocation)
        {
            var shieldManager = RestoreLocation.GetComponent<ShieldManager>();
            var capacity = shieldManager.GetShieldCapacity();
            if (GetLevel() == 2)
            {
                shieldManager.SetShields((int)(capacity * 0.50));
            }

            if (GetLevel() == 3)
            {
                shieldManager.SetShields((int)(capacity * 0.75));
            }
        }
    }
}
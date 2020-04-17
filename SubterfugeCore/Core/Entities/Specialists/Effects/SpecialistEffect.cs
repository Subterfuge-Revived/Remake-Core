using System;
using System.Linq;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An abstract class for implementing a specialist effect
    /// </summary>
    public abstract class SpecialistEffect : ISpecialistEffect
    {
        /// <summary>
        /// The base value of the effect
        /// </summary>
        public float _effectValue { private get; set; } = 0;
        
        /// <summary>
        /// The scalar value to scale the effect if the EffectScale is set.
        /// </summary>
        public float _scalar { get; set; } = 1;
        
        /// <summary>
        /// The Type of scaling that should be applied to the event.
        /// This applies a multiplication on the event's base value to determine the total effect.
        /// </summary>
        public EffectScale _effectScale { get; set; } = EffectScale.None;
        
        /// <summary>
        /// How the effect is triggered
        /// </summary>
        public EffectTrigger _effectTrigger { get; set; } = EffectTrigger.None;
        
        /// <summary>
        /// Who the effect targets
        /// </summary>
        public EffectTarget _effectTarget { get; set; } = EffectTarget.None;
        
        /// <summary>
        /// How large to apply the trigger effect to search for event triggers.
        /// </summary>
        public EffectTriggerRange _effectTriggerRange { get; set; } = EffectTriggerRange.Self;
        
        /// <summary>
        /// Applies the event's forward action
        /// </summary>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract void ForwardEffect(ICombatable friendly, ICombatable enemy);
        
        /// <summary>
        /// Applies the event's backwards action
        /// </summary>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract void BackwardEffect(ICombatable friendly, ICombatable enemy);

        /// <summary>
        /// Applies the EffectScale value to the effect's value. In implementations of the SpecialistEffect class
        /// this method should be used in order to determine the total magnitude of the effect.
        /// </summary>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        /// <returns></returns>
        public float getEffectValue(ICombatable friendly, ICombatable enemy)
        {
            // TODO: Apply specialist scaling here
            float scalar = this._scalar;

            switch (_effectScale)
            {
                case EffectScale.None:
                    break;
                case EffectScale.PlayerFactoryCount:
                    if (this._effectTarget == EffectTarget.Friendly)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Factory).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Enemy)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Factory).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Both)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Factory).Count + Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Factory).Count;
                    }

                    break;
                case EffectScale.PlayerMineCount:
                    if (this._effectTarget == EffectTarget.Friendly)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Mine).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Enemy)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Mine).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Both)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Mine).Count + Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).FindAll(outpost => outpost.GetOutpostType() == OutpostType.Mine).Count;
                    }
                    break;
                case EffectScale.PlayerOutpostCount:
                    if (this._effectTarget == EffectTarget.Friendly)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Enemy)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).Count;
                    }
                    else if (this._effectTarget == EffectTarget.Both)
                    {
                        scalar = Game.TimeMachine.GetState().GetPlayerOutposts(enemy.GetOwner()).Count + Game.TimeMachine.GetState().GetPlayerOutposts(friendly.GetOwner()).Count;
                    }
                    break;
            }
            
            return this._effectValue * _scalar;
        }
    }
}
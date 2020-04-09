using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Friendly sub arrival
    /// </summary>
    public class FriendlySubArrive : IReversible
    {
        Sub _arrivingSub;
        Outpost _outpost;
        private bool _eventSuccess = false;

        /// <summary>
        /// Friendly sub arrival event
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public FriendlySubArrive(ICombatable combatant1, ICombatable combatant2)
        {
            this._arrivingSub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this._outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        /// <summary>
        /// Undoes the sub's arrival
        /// </summary>
        /// <returns>If the event was undone</returns>
        public bool BackwardAction()
        {
            if (this._eventSuccess)
            {
                this._outpost.RemoveDrillers(this._arrivingSub.GetDrillerCount());
                this._outpost.GetSpecialistManager()
                    .RemoveSpecialists(this._arrivingSub.GetSpecialistManager().GetSpecialists());
                Game.TimeMachine.GetState().AddSub(this._arrivingSub);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Perfoms a friendly sub arrival
        /// </summary>
        /// <returns>If the event was successful</returns>
        public bool ForwardAction()
        {
            if (Game.TimeMachine.GetState().SubExists(this._arrivingSub))
            {
                this._outpost.AddDrillers(this._arrivingSub.GetDrillerCount());
                this._outpost.GetSpecialistManager().AddSpecialists(this._arrivingSub.GetSpecialistManager().GetSpecialists());
                Game.TimeMachine.GetState().RemoveSub(this._arrivingSub);
                this._eventSuccess = true;
            }
            else
            {
                this._eventSuccess = false;
            }
            return this._eventSuccess;
        }
        
        /// <summary>
        /// Determines if the event was successful.
        /// </summary>
        /// <returns>If the event is successful</returns>
        public bool WasEventSuccessful()
        {
            return this._eventSuccess;
        }
    }
}

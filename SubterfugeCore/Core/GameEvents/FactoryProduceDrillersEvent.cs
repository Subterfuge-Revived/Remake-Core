using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// Driller production event
    /// </summary>
    public class FactoryProduceDrillersEvent : GameEvent
    {
        /// <summary>
        /// The outpost producing the drillers
        /// </summary>
        Outpost _outpost;
        
        /// <summary>
        /// If the event was successful
        /// </summary>
        bool _eventSuccess = false;
        
        /// <summary>
        /// If the net production event was added to the time machine
        /// </summary>
        bool _addedNextProduction = false;
        
        /// <summary>
        /// A reference to the next production event.
        /// </summary>
        GameEvent _nextEvent;

        /// <summary>
        /// Constructor for the driller production event
        /// </summary>
        /// <param name="outpost">The outpost to produce drillers at</param>
        /// <param name="tick">The gameTick to produce them at</param>
        public FactoryProduceDrillersEvent(Outpost outpost, GameTick tick) : base(tick)
        {
            this._outpost = outpost;
            this.EventName = "Factory Production Event";
        }
        
        /// <summary>
        /// Undo a production event
        /// </summary>
        public override bool BackwardAction()
        {
            if (_eventSuccess)
            {
                _outpost.RemoveDrillers(6);
                Game.TimeMachine.RemoveEvent(this._nextEvent);
                this._nextEvent = null;
            }

            return this._eventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this._eventSuccess;
        }

        /// <summary>
        /// Forward production event
        /// </summary>
        public override bool ForwardAction()
        {
            if (Validator.ValidateOutpost(_outpost))
            {
                if(this._outpost.GetOwner() != null && this._outpost.GetOutpostType() == OutpostType.Factory)
                {
                    this._nextEvent = new FactoryProduceDrillersEvent(this._outpost, this._gameTick.Advance(40));
                    Game.TimeMachine.AddEvent(this._nextEvent);
                    _outpost.AddDrillers(6);
                    _eventSuccess = true;
                }
                else
                {
                    this._eventSuccess = false;
                }
            }
            else
            {
                this._eventSuccess = false;
            }

            return this._eventSuccess;
        }
    }
}

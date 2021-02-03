using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Config;

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
        /// The tick the outpost is expected to produce drillers on
        /// </summary>
        GameTick _productionTick;

        /// <summary>
        /// If the event was successful
        /// </summary>
        bool _eventSuccess = false;

        /// <summary>
        /// A reference to the next production event.
        /// </summary>
        GameEvent _nextEvent;

        /// <summary>
        /// Constructor for the driller production event
        /// </summary>
        /// <param name="outpost">The outpost to produce drillers at</param>
        /// <param name="tick">The gameTick to produce them at</param>
        public FactoryProduceDrillersEvent(Outpost outpost, GameTick tick)
        {
            this._outpost = outpost;
            this._productionTick = tick;
            this.EventName = "Factory Production Event";
        }

        /// <summary>
        /// Undo a production event
        /// </summary>
        public override bool BackwardAction()
        {
            if (_eventSuccess)
            {
                _outpost.RemoveDrillers(Constants.BASE_FACTORY_PRODUCTION);
                Game.TimeMachine.RemoveEvent(this._nextEvent);
                this._nextEvent = null;
            }

            return this._eventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this._eventSuccess;
        }

        public override string ToJson()
        {
            // Production events don't need to be in the database. No need for JSON.
            return "";
        }

        /// <summary>
        /// Forward production event
        /// </summary>
        public override bool ForwardAction()
        {
            if (Validator.ValidateOutpost(_outpost))
            {
                if(this._outpost.GetOwner() != null && this._outpost.GetOutpostType() == OutpostType.Factory &&
                    this._outpost.GetOwner().GetDrillerCount() < this._outpost.GetOwner().GetDrillerCapacity())
                {
                    this._nextEvent = new FactoryProduceDrillersEvent(this._outpost, this._productionTick.Advance(40));
                    Game.TimeMachine.AddEvent(this._nextEvent);
                    int drillerDifference = this._outpost.GetOwner().GetDrillerCapacity() - this._outpost.GetOwner().GetDrillerCount();

                    // If capacity would be exceeded, produce only as much drillers as possible without exceeding capacity
                    if (drillerDifference >= Constants.BASE_FACTORY_PRODUCTION)
                    {
                        _outpost.AddDrillers(Constants.BASE_FACTORY_PRODUCTION);
                    } else
                    {
                        //_outpost.AddDrillers(drillerDifference); // TODO: revert this properly

						// Temporary solution, Factories produce 6 drillers or nothing.
						this._eventSuccess = false;
						return this._eventSuccess;
                    }

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

        /// <summary>
        /// Get the tick of the production
        /// </summary>
        /// <returns>The production event's tick</returns>
        public override GameTick GetTick()
        {
            return this._productionTick;
        }
    }
}

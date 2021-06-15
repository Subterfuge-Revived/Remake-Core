using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost
{
	/// <summary>
	/// Factory Production cycle
	/// </summary>
	public class FactoryProduction : NaturalGameEvent
	{
		Factory _producingFactory;
		int _productionAmount;
		private bool _eventSuccess = false;
		FactoryProduction _nextProduction;

		public FactoryProduction(Factory factory, GameTick occursAt) : base(occursAt, Base.Priority.NATURAL_PRIORITY_9)
		{
			this._producingFactory = factory;
			this._nextProduction = null;
		}

		public override bool ForwardAction(TimeMachine timemachine, GameState state)
		{
			this._productionAmount = this._producingFactory.GetDrillerProduction(state);
			if (state.OutpostExists(_producingFactory) && this._productionAmount > 0)
			{
				this._producingFactory.AddDrillers(this._productionAmount);
				this._eventSuccess = true;
				if (this._nextProduction == null)
				{
					this._nextProduction = new FactoryProduction(this._producingFactory, base.GetOccursAt().Advance(this._producingFactory.GetTicksPerProduction()));
					timemachine.AddEvent(this._nextProduction);
				}
			}
			else
			{
				this._eventSuccess = false;
			}
			return this._eventSuccess;
		}

		public override bool BackwardAction(TimeMachine timemachine, GameState state)
		{
			if (_eventSuccess)
			{
				this._producingFactory.RemoveDrillers(this._productionAmount);
				return true;
			}
			return false;
		}

		public override bool WasEventSuccessful()
		{
			return this._eventSuccess;
		}
	}
}
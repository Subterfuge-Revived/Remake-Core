using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Components;
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
		private Factory _producingFactory;
		private int _productionAmount;
		private FactoryProduction _nextProduction;

		public FactoryProduction(Factory factory, GameTick occursAt) : base(occursAt, Base.Priority.NATURAL_PRIORITY_9)
		{
			this._producingFactory = factory;
			this._nextProduction = null;
		}

		public override bool ForwardAction(TimeMachine timemachine, GameState state)
		{
			this._productionAmount = this._producingFactory.GetDrillerProduction(state);
			if (state.OutpostExists(_producingFactory) && this._productionAmount > 0 && !this._producingFactory.GetComponent<DrillerCarrier>().IsDestroyed())
			{
				this._producingFactory.GetComponent<DrillerCarrier>().AddDrillers(this._productionAmount);
				base.EventSuccess = true;
				if (this._nextProduction == null)
				{
					this._nextProduction = new FactoryProduction(this._producingFactory, base.GetOccursAt().Advance(this._producingFactory.GetTicksPerProduction()));
					timemachine.AddEvent(this._nextProduction);
				}
			}
			else
			{
				base.EventSuccess = false;
			}
			return base.EventSuccess;
		}

		public override bool BackwardAction(TimeMachine timemachine, GameState state)
		{
			if (base.EventSuccess)
			{
				this._producingFactory.GetComponent<DrillerCarrier>().RemoveDrillers(this._productionAmount);
				return true;
			}
			return false;
		}

		public override bool WasEventSuccessful()
		{
			return base.EventSuccess;
		}
	}
}
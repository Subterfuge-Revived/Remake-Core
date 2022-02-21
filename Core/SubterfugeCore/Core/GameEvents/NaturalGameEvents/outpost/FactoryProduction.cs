using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.combat;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost
{
	/// <summary>
	/// Factory Production cycle
	/// </summary>
	public class FactoryProduction : NaturalGameEvent
	{
		private readonly Factory _producingFactory;
		private int _productionAmount;
		private FactoryProduction _nextProduction;

		public FactoryProduction(Factory factory, GameTick occursAt) : base(occursAt, Base.Priority.NaturalPriority9)
		{
			this._producingFactory = factory;
			this._nextProduction = null;
		}

		public override bool ForwardAction(TimeMachine timemachine, GameState.GameState state)
		{
			_productionAmount = this._producingFactory.GetDrillerProduction(state);
			if (state.OutpostExists(_producingFactory) && this._productionAmount > 0 && !this._producingFactory.GetComponent<DrillerCarrier>().IsDestroyed())
			{
				_producingFactory.GetComponent<DrillerCarrier>().AddDrillers(this._productionAmount);
				EventSuccess = true;
				if (_nextProduction == null)
				{
					_nextProduction = new FactoryProduction(_producingFactory, base.GetOccursAt().Advance(this._producingFactory.GetTicksPerProduction()));
					timemachine.AddEvent(this._nextProduction);
				}
			}
			else
			{
				EventSuccess = false;
			}
			return EventSuccess;
		}

		public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
		{
			if (EventSuccess)
			{
				_producingFactory.GetComponent<DrillerCarrier>().RemoveDrillers(_productionAmount);
				return true;
			}
			return false;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}
	}
}
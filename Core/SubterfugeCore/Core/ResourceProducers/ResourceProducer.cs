using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public abstract class ResourceProducer : IResourceProductionEventPublisher
    {
        /// <summary>
	    /// Ticks to the first production cycle
	    /// </summary>
	    private GameTick _nextProductionTick;
        
	    /// <summary>
	    /// The number of ticks that need to pass before the next production event occurs.
	    /// One neptunium per day (1440 minutes) per outpost
	    /// </summary>
	    private int _ticksPerProductionCycle;

	    /// <summary>
	    /// Amount of drillers produced each production cycle
	    /// </summary>
	    private int _baseValuePerProduction;

	    private List<ProductionEventArgs> _productionEvents = new List<ProductionEventArgs>();

	    public ResourceProducer(
		    int ticksPerProductionCycle,
		    int baseValuePerProduction,
            TimeMachine timeMachine
        ) {
		    _ticksPerProductionCycle = ticksPerProductionCycle;
		    _baseValuePerProduction = baseValuePerProduction;
            this._nextProductionTick = timeMachine.GetCurrentTick().Advance(_ticksPerProductionCycle);
            
            timeMachine.OnTick += ProductionTickListener;
        }

	    public void ChangeAmountProducedPerCycle(int delta)
	    {
		    _baseValuePerProduction = Math.Max(0, _baseValuePerProduction + delta);
	    }

	    public void ChangeTicksPerProductionCycle(int delta)
	    {
		    _ticksPerProductionCycle = Math.Max(0, _ticksPerProductionCycle + delta);
	    }

        private void ProductionTickListener(object timeMachine, OnTickEventArgs tickEventArgs)
		{
			if (tickEventArgs.Direction == TimeMachineDirection.FORWARD)
			{
				if (_nextProductionTick == tickEventArgs.CurrentTick)
				{
					
					var productionAmount = this.GetNextProductionAmount(tickEventArgs.CurrentState, _baseValuePerProduction);
					_nextProductionTick = tickEventArgs.CurrentTick.Advance(_ticksPerProductionCycle);
					
					Produce(productionAmount);
					
					var productionEvent = new ProductionEventArgs()
					{
						TickProducedAt = tickEventArgs.CurrentTick,
						Direction = tickEventArgs.Direction,
						ValueProduced = productionAmount,
						NextProduction = _nextProductionTick
					};

					_productionEvents.Add(productionEvent);
					OnResourceProduced?.Invoke(this, productionEvent);
				}
			}
			else
			{
				var lastProductionEvent = _productionEvents[_productionEvents.Count - 1];
				if (lastProductionEvent.TickProducedAt.Rewind(1) == tickEventArgs.CurrentTick)
				{
					UndoProduce(lastProductionEvent.ValueProduced);
					_nextProductionTick = tickEventArgs.CurrentTick.Rewind(_ticksPerProductionCycle);
					OnResourceProduced?.Invoke(this, lastProductionEvent);
				}
			}
		}

        public abstract void Produce(int productionAmount);

        public abstract void UndoProduce(int amountToRevert);

		/// <summary>
		/// Gets the number of units that would be produced by this mine if
		/// a production cycle occurred in the current GameState.
		/// </summary>
		/// <param name="state">The current game state</param>
		/// <returns>The total amount of neptunium produced if the production occurred now.</returns>
		public abstract int GetNextProductionAmount(GameState.GameState state, int baseValuePerProduction);

		public event EventHandler<ProductionEventArgs>? OnResourceProduced;
    }
}
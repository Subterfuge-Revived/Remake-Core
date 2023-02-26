using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Resources.Producers
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
	    protected int BaseValuePerProduction;

	    private bool _isPaused = false;

	    private List<ProductionEventArgs> _productionEvents = new List<ProductionEventArgs>();

	    public ResourceProducer(
		    int ticksPerProductionCycle,
		    int baseValuePerProduction,
            TimeMachine timeMachine
        ) {
		    _ticksPerProductionCycle = ticksPerProductionCycle;
		    BaseValuePerProduction = baseValuePerProduction;
            this._nextProductionTick = timeMachine.GetCurrentTick().Advance(_ticksPerProductionCycle);
            
            timeMachine.OnTick += ProductionTickListener;
        }

	    public void SetPaused(bool isPaused)
	    {
		    _isPaused = isPaused;
	    }

	    public void ChangeAmountProducedPerCycle(int delta)
	    {
		    BaseValuePerProduction = Math.Max(0, BaseValuePerProduction + delta);
	    }

	    public void ChangeTicksPerProductionCycle(int delta)
	    {
		    _ticksPerProductionCycle = Math.Max(0, _ticksPerProductionCycle + delta);
	    }

	    public GameTick GetNextProductionTick()
	    {
		    return _nextProductionTick;
	    }

	    public int GetTicksPerProductionCycle()
	    {
		    return _ticksPerProductionCycle;
	    }

        private void ProductionTickListener(object timeMachine, OnTickEventArgs tickEventArgs)
		{
			if (tickEventArgs.Direction == TimeMachineDirection.FORWARD)
			{
				if (_nextProductionTick == tickEventArgs.CurrentTick)
				{
					var productionAmount = this.GetNextProductionAmount(tickEventArgs.CurrentState);
					_nextProductionTick = tickEventArgs.CurrentTick.Advance(_ticksPerProductionCycle);
					
					Produce(productionAmount);
					
					var productionEvent = new ProductionEventArgs()
					{
						TickProducedAt = tickEventArgs.CurrentTick,
						Direction = tickEventArgs.Direction,
						ValueProduced = productionAmount,
						NextProduction = _nextProductionTick,
					};

					_productionEvents.Add(productionEvent);
					OnResourceProduced?.Invoke(this, productionEvent);
				}
			}
			else
			{
				if (_productionEvents.Count > 0)
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
		}

        protected abstract void Produce(int productionAmount);

        protected abstract void UndoProduce(int amountToRevert);

		/// <summary>
		/// Gets the number of units that would be produced by this mine if
		/// a production cycle occurred in the current GameState.
		/// </summary>
		/// <param name="state">The current game state</param>
		/// <returns>The total amount of neptunium produced if the production occurred now.</returns>
		public abstract int GetNextProductionAmount(GameState state);

		public event EventHandler<ProductionEventArgs>? OnResourceProduced;
    }
}
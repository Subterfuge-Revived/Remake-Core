using System;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	/// <summary>
	/// Factory Class
	/// TODO: be able to halt production (e.g. electrical runs out)
	/// TODO: randomize first production time
	/// TODO: be able to change production cycle time
	/// </summary>
	public class Factory : Outpost
	{
		public static int STANDARD_TICKS_PER_PRODUCTION = (int)(Constants.MinutesPerProduction / GameTick.MinutesPerTick);

		/// <summary>
		/// Production time in game ticks
		/// </summary>
		private int _ticksPerProduction;

		/// <summary>
		/// Ticks to the first production cycle
		/// </summary>
		private GameTick _ticksToFirstProduction;

		/// <summary>
		/// Amount of drillers produced each production cycle
		/// </summary>
		private int _drillersPerProduction;

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Factory(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
			this._ticksToFirstProduction = new GameTick(STANDARD_TICKS_PER_PRODUCTION); //TODO: randomize
			this._drillersPerProduction = Constants.BaseFactoryProduction;
		}

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Factory(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
			this._ticksToFirstProduction = new GameTick(STANDARD_TICKS_PER_PRODUCTION); //TODO: randomize
			this._drillersPerProduction = Constants.BaseFactoryProduction;
		}
		public override OutpostType GetOutpostType()
		{
			return OutpostType.Factory;
		}

		/// <summary>
		/// Gets the number of drillers that would be produced by this factory if
		/// a production cycle occurred in the current GameState.
		/// </summary>
		/// <param name="state">The current game state</param>
		/// <returns>Either the remaining extra driller capacity or
		/// the driller production of the factory, whichever is lower. </returns>
		public int GetDrillerProduction(GameState.GameState state)
		{
			return Math.Min(state.GetExtraDrillerCapcity(GetComponent<DrillerCarrier>().GetOwner()), this._drillersPerProduction);
		}

		/// <summary>
		/// Gets the maximum driller production per cycle of this factory.
		/// </summary>
		/// <returns>The maximum driller production per cycle.</returns>
		public int GetDrillerProductionCapacity()
		{
			return this._drillersPerProduction;
		}

		/// <summary>
		/// Gets the number of ticks between two production cycle of this factory.
		/// </summary>
		/// <returns>An integer representing the number of ticks a production cycle takes.</returns>
		public int GetTicksPerProduction()
		{
			return this._ticksPerProduction;
		}

		/// <summary>
		/// Gets the tick of the first production at this factory, which is not necessarily the same as the number of ticks between two production cycles.
		/// </summary>
		/// <returns>The GameTick during which the first production of this factory occurs.</returns>
		public GameTick GetTicksToFirstProduction()
		{
			return this._ticksToFirstProduction;
		}
	}
}
using System;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	/// <summary>
	/// Factory Class
	/// TODO: be able to halt production (e.g. electrical runs out)
	/// TODO: randomize first production time
	/// TODO: be able to change production cycle time
	/// </summary>
	public class Factory : Outpost
	{
		public static int STANDARD_TICKS_PER_PRODUCTION = (int)(Constants.MINUTES_PER_PRODUCTION / GameTick.MINUTES_PER_TICK);

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
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Factory(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
			this._ticksToFirstProduction = new GameTick(STANDARD_TICKS_PER_PRODUCTION); //TODO: randomize
			this._drillersPerProduction = Constants.BASE_FACTORY_PRODUCTION;
		}

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Factory(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
			this._ticksToFirstProduction = new GameTick(STANDARD_TICKS_PER_PRODUCTION); //TODO: randomize
			this._drillersPerProduction = Constants.BASE_FACTORY_PRODUCTION;
		}
		public override OutpostType GetOutpostType()
		{
			return OutpostType.Factory;
		}

		public override float GetVisionRange()
		{
			return Constants.BASE_OUTPOST_VISION_RADIUS;
		}

		/// <summary>
		/// Gets the number of drillers that would be produced by this factory if
		/// a production cycle occurred in the current GameState.
		/// </summary>
		/// <param name="state">The current game state</param>
		/// <returns>Either the remaining extra driller capacity or
		/// the driller production of the factory, whichever is lower. </returns>
		public int GetDrillerProduction(GameState state)
		{
			return Math.Min(state.GetExtraDrillerCapcity(GetOwner()), this._drillersPerProduction);
		}

		/// <summary>
		/// Gets the maximum driller production per cycle of this factory.
		/// </summary>
		/// <returns>The maximum driller production per cycle.</returns>
		public int GetDrillerProductionCapacity()
		{
			return this._drillersPerProduction;
		}

		public int GetTicksPerProduction()
		{
			return this._ticksPerProduction;
		}

		public GameTick GetTicksToFirstProduction()
		{
			return this._ticksToFirstProduction;
		}
	}
}
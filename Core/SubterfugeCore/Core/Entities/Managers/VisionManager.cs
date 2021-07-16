using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Entities.Managers
{
	/// <summary>
	/// Stores information on all VisionEvents in a game.
	/// </summary>
	public class VisionManager
	{
		/// <summary>
		/// Stores all vision events by player.
		/// </summary>
		private Dictionary<Player, PriorityQueue<IVisionEvent>> _visionEvents;

		private List<RecalculateVisionEvent> _recalculateVisionEvents;

		/// <summary>
		/// VisionManager constructor
		/// </summary>
		/// <param name="players">Players to construct the dictionary</param>
		public VisionManager(List<Player> players)
		{
			this._visionEvents = new Dictionary<Player, PriorityQueue<IVisionEvent>>();
			this._recalculateVisionEvents = new List<RecalculateVisionEvent>();
			foreach(Player p in players)
			{
				this._visionEvents.Add(p, new PriorityQueue<IVisionEvent>());
			}
		}

		/// <summary>
		/// Adds a vision event to the VisionManager.
		/// </summary>
		/// <param name="visionEvent">The vision event to add.</param>
		public void AddVisionEvent(IVisionEvent visionEvent)
		{
			this._visionEvents[visionEvent.GetPlayer()].Enqueue(visionEvent);
		}

		/// <summary>
		/// Gets a list of all vision events for a player. From this list, it can be deduced what information a player has access to.
		/// </summary>
		/// <param name="player">The player for whom to get vision events.</param>
		/// <returns>A list of all vision events for the passed player.</returns>
		public PriorityQueue<IVisionEvent> GetPlayerVisionEvents(Player player)
		{
			//TODO: Consider the future
			return this._visionEvents[player];
		}

		/// <summary>
		/// Adds a RecalculateVisionEvent to the VisionManager's list of RecalculationVisionEvents. As the Time Machine goes forwards a tick at a time, the tick number of the visionEvent should always be the current size of the list, and therefore visionEvent.GetOccursAt() == _recalculateVisionEvents.IndexOf(visionEvent).
		/// </summary>
		/// <param name="visionEvent">The RecalculateVisionEvent to add.</param>
		public void AddVisionRecalculation(RecalculateVisionEvent visionEvent)
		{
			this._recalculateVisionEvents.Add(visionEvent);
		}

		/// <summary>
		/// Gets the RecalculateVisionEvent for a specific game tick.
		/// </summary>
		/// <param name="tick">The tick to get the RecalculateVisionEvent of.</param>
		/// <returns>The specified RecalculateVisionEvent if it exists, and null otherwise.</returns>
		public RecalculateVisionEvent GetRecalculateVisionEvent(GameTick tick)
		{
			return GetRecalculateVisionEvent(tick.GetTick());
		}

		/// <summary>
		/// Gets the RecalculateVisionEvent for a specific game tick.
		/// </summary>
		/// <param name="tick">The tick to get the RecalculateVisionEvent of.</param>
		/// <returns>The specified RecalculateVisionEvent if it exists, and null otherwise.</returns>
		public RecalculateVisionEvent GetRecalculateVisionEvent(int tick)
		{
			if (tick <= this._recalculateVisionEvents.Count)
			{
				return _recalculateVisionEvents[tick];
			}
			return null;
		}

		/// <summary>
		/// Checks if a RecalculateVisionEvent already exists for a given GameTick.
		/// </summary>
		/// <param name="tick">The tick for which to check the prior creation of a RecalulateVisionEvent.</param>
		/// <returns>True if the event already exists, and false otherwise.</returns>
		public bool DoesRecalculateVisonEventExist(GameTick tick)
		{
			return tick.GetTick() >= 0 && tick.GetTick() < this._recalculateVisionEvents.Count;
		}

		public Dictionary<Player, PriorityQueue<IVisionEvent>> GetDictionary()
		{
			return this._visionEvents;
		}

		public List<RecalculateVisionEvent> GetRecalculateVisionEvents()
		{
			return this._recalculateVisionEvents;
		}
	}
}

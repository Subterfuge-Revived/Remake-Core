using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision
{
	/// <summary>
	/// VisionEvents keep track of when Combatables enter or leave the Field of View (FOV) of players in the game. All VisionEvents are handled by the VisionManager of the game.
	/// </summary>
	public abstract class IVisionEvent : GameEvent
	{
		/// <summary>
		/// The id of the VisionEvent.
		/// </summary>
		protected string _id;

		/// <summary>
		/// The entity which either enters or leaves FOV.
		/// </summary>
		protected ICombatable _entity;

		/// <summary>
		/// The player whose FOV the Combatable entity has entered or left.
		/// </summary>
		protected Player _player;

		/// <summary>
		/// The game tick which the vision event occurs.
		/// </summary>
		protected GameTick _occursAt;

		public IVisionEvent()
		{
			this._id = Guid.NewGuid().ToString();
		}

		public ICombatable GetEntity()
		{
			return this._entity;
		}

		public Player GetPlayer()
		{
			return this._player;
		}

		public override GameTick GetOccursAt()
		{
			return this._occursAt;
		}

		public override string GetEventId()
		{
			return this._id;
		}

		public override Priority GetPriority()
		{
			return Priority.LOW_PRIORTY;
		}
	}
}

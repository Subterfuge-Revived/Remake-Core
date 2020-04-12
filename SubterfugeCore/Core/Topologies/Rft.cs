using System;
using System.Numerics;
using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Topologies
{
	/// <summary>
	/// A rectangular flat torus map. Contains height, width.
	/// </summary>
	public class Rft
	{
		/// <summary>
		/// The height of the rectangular flat torus map.
		/// </summary>
		public float Height;
		
		/// <summary>
		/// The width of the rectangular flat torus map.
		/// </summary>
		public float Width;

		public Rft(float height, float width)
		{
			this.Height = height;
			this.Width = width;
		}
	}
}
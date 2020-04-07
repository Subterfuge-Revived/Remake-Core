using System;
using System.Numerics;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Map
{
	/// <summary>
	/// A rectangular flat torus map. Contains height, width.
	/// </summary>
	public class Rft
	{
		/// <summary>
		/// The height of the rectangular flat torus map.
		/// </summary>
		public double height;
		
		/// <summary>
		/// The width of the rectangular flat torus map.
		/// </summary>
		public double width;

		public Rft(double height, double width)
		{
			this.height = height;
			this.width = width;
		}
	}
}
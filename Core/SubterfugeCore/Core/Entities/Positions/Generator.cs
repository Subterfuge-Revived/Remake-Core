using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	public class Generator : Outpost
	{
		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Generator(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
		}

		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Generator(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Generator;
		}
	}
}

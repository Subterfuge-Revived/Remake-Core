using System;
using System.Numerics;

namespace SubterfugeCore.Core.Map
{
	
	/// <summary>
	/// A Vector in a Rectangular Flat Torus (Rft) map. (x,y) is in
	/// (-map.width/2, map.width/2] x (-map.height/2, map.height/2]
	/// </summary>
	public class RftVector
	{
		/// <summary3>
		/// The x-value of the RftVector. This is in the interval (-map.width/2, map.width/2]. 
		/// </summary>
		private double _x;

		public double x
		{
			get { return _x; }
			set
			{
				_x = (value % map.width + map.width) % map.width;
				if (_x > map.width / 2) _x -= map.width;
			}
		}
		
		/// <summary>
		/// The y value of the RftVector. This is in the interval (-map.height/2, map.height/2]. 
		/// </summary>
		private double _y;

		public double y
		{
			get { return _y; }
			set
			{
				_y = (value % map.height + map.height) % map.height;
				if (_y > map.height / 2) _y -= map.height;
			}
		}
		
		/// <summary>
		/// The map that the vector resides in. An RftVector only makes sense in the context of a Rft map.
		/// </summary>
		public Rft map;

		public RftVector(Rft map, double x, double y)
		{
			this.map = map;
			this._x = (x % map.width + map.width) % map.width;
			this._y = (y % map.height + map.height) % map.height;
			if (this._x > map.width / 2) this._x -= map.width;
			if (this._y > map.height / 2) this._y -= map.height;
		}
		
		public double Dist(RftVector a, RftVector b) { return (b-a).Magnitude(); }

		public double Magnitude() {return Math.Sqrt(_x*_x + _y*_y);}
		
		public static RftVector operator +(RftVector a) => a;
		public static RftVector operator -(RftVector a) => new RftVector(a.map, -a._x, -a._y);

		public static RftVector operator +(RftVector a, RftVector b) =>
			new RftVector(a.map, a._x + b._x, a._y + b._y);

		public static RftVector operator -(RftVector a, RftVector b) => a + (-b);
	}
}
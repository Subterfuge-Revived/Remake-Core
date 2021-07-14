using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace SubterfugeCore.Core.Topologies
{
	
	/// <summary>
	/// A Vector in a Rectangular Flat Torus (Rft) map. (x,y) is in
	/// (-map.width/2, map.width/2] x (-map.height/2, map.height/2]
	/// </summary>
	public class RftVector
	{
		/// <summary>
		/// The x-value of the RftVector. This is in the interval (-map.width/2, map.width/2]. 
		/// </summary>
		private float _x;

		public float X
		{
			get { return _x; }
			set
			{
				_x = (value % Map.Width + Map.Width) % Map.Width;
				if (_x > Map.Width / 2) _x -= Map.Width;
			}
		}
		
		/// <summary>
		/// The y value of the RftVector. This is in the interval (-map.height/2, map.height/2]. 
		/// </summary>
		private float _y;

		public float Y
		{
			get { return _y; }
			set
			{
				_y = (value % Map.Height + Map.Height) % Map.Height;
				if (_y > Map.Height / 2) _y -= Map.Height;
			}
		}
		
		/// <summary>
		/// The map that the vector resides in. An RftVector only makes sense in the context of a Rft map.
		/// </summary>
		public static Rft Map;
		
		/// <summary>
		/// Constructs a new RftVector with coords (0, 0).
		/// </summary>
		/// <param name="map">The map to wrap RftVectors by</param>
		public RftVector(Rft map)
		{
			Map = map;
			this._x = 0;
			this._y = 0;
		}

		/// <summary>
		/// Constructs a new RftVector with coords (x, y) modulo map dimensions.
		/// </summary>
		/// <param name="x">The x position</param>
		/// <param name="y">The y position</param>
		public RftVector(Rft map, float x, float y)
		{
			Map = map;
			this._x = (x % map.Width + map.Width) % map.Width;
			this._y = (y % map.Height + map.Height) % map.Height;
			if (this._x > map.Width / 2) this._x -= map.Width;
			if (this._y > map.Height / 2) this._y -= map.Height;
		}

		/// <summary>
		/// Create an instance of an RftVector with coords (x, y)
		/// </summary>
		/// <param name="x">The x position</param>
		/// <param name="y">The y position</param>
		/// <exception cref="NotSupportedException">If the RftVector's static 'Map' has not been defined, an error is thrown</exception>
		public RftVector(float x, float y)
		{
			if (Map == null)
				throw new NotSupportedException("Cannot initialize an RftVector without having previously defined the Map.");
			this._x = (x % Map.Width + Map.Width) % Map.Width;
			this._y = (y % Map.Height + Map.Height) % Map.Height;
			if (this._x > Map.Width / 2) this._x -= Map.Width;
			if (this._y > Map.Height / 2) this._y -= Map.Height;
		}
		
		/// <summary>
		/// Constructs a new RftVector with coords (v.x, v.y) modulo map dimensions.
		/// </summary>
		public RftVector(Rft map, Vector2 v)
		{
			Map = map;
			this._x = (v.X % map.Width + map.Width) % map.Width;
			this._y = (v.Y % map.Height + map.Height) % map.Height;
			if (this._x > map.Width / 2) this._x -= map.Width;
			if (this._y > map.Height / 2) this._y -= map.Height;
		}

		// Use (b-a).Magnitude();
		// public float Dist(RftVector a, RftVector b) { return (b-a).Magnitude(); }

		/// <summary>
		/// Returns float: the magnitude of the RftVector as if it were a Vector2.
		/// </summary>
		public float Magnitude() {return (float)Math.Sqrt(_x*_x + _y*_y);}

		/// <summary>
		/// Calculates the shortest distance between this RftVector instance and another RftVector instance on a Map.
		/// </summary>
		/// <param name="other">The other RftVector from which the distance is desired</param>
		/// <returns>The distance between the two RftVectors.</returns>
		public float Distance(RftVector other)
		{
			return (this - other).Magnitude();
		}

		/// <summary>
		/// Returns Vector2: the RftVector normalized to have magnitude 1. (0, 0) normalized will return (1, 0).
		/// </summary>
		public Vector2 Normalize()
		{
			if ((this._x == 0) && (this._y == 0)) { return new Vector2(1, 0); }
			return new Vector2(_x / this.Magnitude(), _y / this.Magnitude());
		}

		/// <summary>
		/// Returns a Vector2 from the RftVector
		/// </summary>
		/// <returns>A Vector2 representing the RftVector</returns>
		public Vector2 ToVector2()
		{
			return new Vector2(this._x, this._y);
		}

		/// <summary>
		/// No args: returns 3x3 lattice centered at origin. 8 args: returns sufficient lattice to cover viewport.
		/// </summary>
		// Implemented directly in subterfuge-unity instead.
		/*public List<Vector2> RenderPos()
		{
			var positions = new List<Vector2>();
			for (int i = 1; i >= -1; i--)
			{
				for (int j = -1; j <= 1; j++)
				{
					positions.Append(new Vector2(j * this.map.width + this._x, i * this.map.height + this._y));
				}
			}
			return positions;
		}

		public List<Vector2> RenderPos(float objLeft, float objRight, float objTop, float objBottom, float objWidth, 
			float viewportLeft, float viewportRight, float viewportTop, float viewportBottom)
		{
			var positions = new List<Vector2>();
			for (float i = this._y + (float)Math.Floor((viewportTop+objBottom-this._y)/this.map.height) * this.map.height;
				i > viewportBottom - objTop; i -= this.map.height)
			{
				for (float j = this._x + (float)Math.Floor((viewportLeft+objRight-this._x)/this.map.width) * this.map.width;
					j < viewportRight + objLeft; j += this.map.width)
				{
					positions.Append(new Vector2(j, i));
				}
			}
			return positions;
		}*/

		/// <summary>
		/// Static Distance overload to calculate the distance between two RftVectors.
		/// </summary>
		/// <param name="vector1">The first RftVector</param>
		/// <param name="vector2">The second RftVector</param>
		/// <returns>The distance between the two vectors</returns>		
		public static float Distance(RftVector vector1, RftVector vector2)
		{
			return vector1.Distance(vector2);
		}

		public static RftVector operator +(RftVector a) => a;
		public static RftVector operator -(RftVector a) => new RftVector(Map, -a._x, -a._y);

		public static RftVector operator +(RftVector a, RftVector b) =>
			new RftVector(Map, a._x + b._x, a._y + b._y);

		public static RftVector operator -(RftVector a, RftVector b) => a + (-b);

		public static RftVector operator +(RftVector a, Vector2 b) => new RftVector(Map, a._x + b.X, a._y + b.Y);

		public static RftVector operator -(RftVector a, Vector2 b) => a + (-b);
	}
}
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Gui.Components.Base
{
    public struct Thickness : IEquatable<Thickness>
    {
        public Thickness(int all)
            : this(all, all, all, all)
        {
        }

        public Thickness(int leftRight, int topBottom)
            : this(leftRight, topBottom, leftRight, topBottom)
        {
        }

        public Thickness(int left, int top, int right, int bottom)
            : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Width => Left + Right;
        public int Height => Top + Bottom;
        public Rectangle Size => new Rectangle(0, 0, Width, Height);

        public static implicit operator Thickness(int value)
        {
            return new Thickness(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Thickness)
            {
                var other = (Thickness)obj;
                return Equals(other);
            }

            return base.Equals(obj);
        }

        public bool Equals(Thickness other)
        {
            return Left == other.Left && Right == other.Right && Top == other.Top && Bottom == other.Bottom;
        }

        public static Thickness FromValues(int[] values)
        {
            switch (values.Length)
            {
                case 1:
                    return new Thickness(values[0]);
                case 2:
                    return new Thickness(values[0], values[1]);
                case 4:
                    return new Thickness(values[0], values[1], values[2], values[3]);
                default:
                    throw new FormatException("Invalid thickness");
            }
        }
    }
}

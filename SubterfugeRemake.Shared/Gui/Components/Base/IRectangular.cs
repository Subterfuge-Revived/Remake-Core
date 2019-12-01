using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Gui.Components.Base
{
    public interface IRectangular
    {
        Rectangle BoundingRectangle { get; }
    }

    public interface IRectangularF
    {
        System.Drawing.RectangleF BoundingRectangle { get; }
    }
}

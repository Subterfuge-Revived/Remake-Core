using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Components
{
    interface ISerializable
    {
        String toJSON();
        void fromJSON(string jsonString);
    }
}

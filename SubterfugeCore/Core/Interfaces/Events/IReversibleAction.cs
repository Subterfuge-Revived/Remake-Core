using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces
{
    interface IReversibleAction
    {
        void forwardAction();
        void backwardAction();
    }
}

﻿namespace SubterfugeCore.Core.Players.Currency
{
    class Currency
    {
        public int value = 0;
        public bool? canBeNegative = true;

        public Currency(int Cvalue, bool? CcanBeNegative)
        {
            value = Cvalue;
            canBeNegative = CcanBeNegative;
        }
    }
}

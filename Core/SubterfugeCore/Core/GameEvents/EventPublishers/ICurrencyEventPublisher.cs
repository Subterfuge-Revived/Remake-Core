using System;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public class ICurrencyEventPublisher
    {
        private event EventHandler<GainCurrencyEventArgs> OnGainCurrency;
        private event EventHandler<SpendCurrencyEventArgs> OnSpendCurrency;
    }

    public class GainCurrencyEventArgs: DirectionalEventArgs
    {
        public Player PlayerGainingCurrency { get; set; }
        public int AmountGained { get; set; }
    }

    public class SpendCurrencyEventArgs: DirectionalEventArgs
    {
        public Player PlayerSpendingCurrency { get; set; }
        public int AmountSpent { get; set; }
    }
}
using System;
using System.Threading.Tasks;

namespace DripKings.Economy
{
    // Offline session rules. In multiplayer these rules and proximity validation must run on the server.
    public sealed class StreetRunSession
    {
        public const long RunReward = 100;
        public const long CrownPrice = 250;
        public const string CrownItem = "neon-crown";
        private readonly ICurrencyService currency;
        private int checkpoint;
        private int run = 1;
        private bool busy;
        public int NextCheckpoint => checkpoint;
        public int CompletedRuns => run - 1;
        public StreetRunSession(ICurrencyService currency) { this.currency = currency ?? throw new ArgumentNullException(nameof(currency)); }

        public async Task<bool> ReachCheckpointAsync(int index)
        {
            if (busy || index != checkpoint) return false;
            if (checkpoint < 2) { checkpoint++; return false; }
            busy = true;
            try
            {
                await currency.AwardAsync("street-run:" + run, RunReward);
                run++;
                checkpoint = 0;
                return true;
            }
            finally { busy = false; }
        }

        public Task<Receipt> BuyCrownAsync() => currency.PurchaseAsync("purchase:" + CrownItem, CrownItem, CrownPrice);
        public Task<long> GetBalanceAsync() => currency.GetBalanceAsync();
        public Task<bool> OwnsCrownAsync() => currency.OwnsAsync(CrownItem);
    }
}

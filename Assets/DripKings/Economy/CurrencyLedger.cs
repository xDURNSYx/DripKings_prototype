using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DripKings.Economy
{
    public sealed class CurrencyDefinition
    {
        public const string Id = "kings-credits-v1";
        public const string Name = "Kings Credits";
        public const string Disclosure = "In-game currency only. No real-world value. No cash-out or transfers. No promised future conversion.";
    }

    public sealed class Receipt
    {
        public string OperationId { get; }
        public long Delta { get; }
        public long BalanceAfter { get; }
        public string ItemId { get; }
        public Receipt(string operationId, long delta, long balanceAfter, string itemId)
        { OperationId = operationId; Delta = delta; BalanceAfter = balanceAfter; ItemId = itemId; }
    }

    // Run on a trusted authority in production. This local implementation is for offline playtests.
    // Idempotency is scoped to one ledger/player; persistent adapters must retain this scope.
    public interface ICurrencyService
    {
        Task<long> GetBalanceAsync();
        Task<Receipt> AwardAsync(string operationId, long amount);
        Task<Receipt> PurchaseAsync(string operationId, string itemId, long price);
        Task<bool> OwnsAsync(string itemId);
    }

    public sealed class CurrencyLedger
    {
        private readonly object gate = new object();
        private readonly Dictionary<string, Receipt> receipts = new Dictionary<string, Receipt>();
        private readonly HashSet<string> inventory = new HashSet<string>();
        private readonly List<Receipt> journal = new List<Receipt>();
        private long balance;
        public long Balance { get { lock (gate) return balance; } }
        public Receipt[] History { get { lock (gate) return journal.ToArray(); } }
        public bool Owns(string itemId) { lock (gate) return inventory.Contains(itemId); }

        public Receipt Award(string operationId, long amount)
        {
            Validate(operationId, amount);
            lock (gate)
            {
                var prior = Replay(operationId, amount, null);
                if (prior != null) return prior;
                long next = checked(balance + amount);
                return Commit(operationId, amount, next, null);
            }
        }

        public Receipt Purchase(string operationId, string itemId, long price)
        {
            Validate(operationId, price);
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Item ID is required.");
            lock (gate)
            {
                var prior = Replay(operationId, -price, itemId);
                if (prior != null) return prior;
                if (inventory.Contains(itemId)) throw new InvalidOperationException("Already owned.");
                if (balance < price) throw new InvalidOperationException("Not enough Kings Credits.");
                return Commit(operationId, -price, balance - price, itemId);
            }
        }

        private Receipt Replay(string id, long delta, string item)
        {
            if (!receipts.TryGetValue(id, out var prior)) return null;
            if (prior.Delta != delta || prior.ItemId != item)
                throw new InvalidOperationException("Operation ID was reused with different details.");
            return prior;
        }

        private Receipt Commit(string id, long delta, long next, string item)
        {
            var receipt = new Receipt(id, delta, next, item);
            receipts.Add(id, receipt);
            journal.Add(receipt);
            if (item != null) inventory.Add(item);
            balance = next;
            return receipt;
        }

        private static void Validate(string id, long amount)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128) throw new ArgumentException("Invalid operation ID.");
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        }
    }

    // Gameplay depends on this asynchronous seam. Future adapters can call a trusted service.
    // Neither wallet keys nor a blockchain dependency belong in gameplay code.
    public sealed class LocalCurrencyService : ICurrencyService
    {
        private readonly CurrencyLedger ledger;
        public LocalCurrencyService(CurrencyLedger ledger) { this.ledger = ledger ?? throw new ArgumentNullException(nameof(ledger)); }
        public Task<long> GetBalanceAsync() => Task.FromResult(ledger.Balance);
        public Task<bool> OwnsAsync(string itemId) => Task.FromResult(ledger.Owns(itemId));
        public Task<Receipt> AwardAsync(string operationId, long amount) => Task.FromResult(ledger.Award(operationId, amount));
        public Task<Receipt> PurchaseAsync(string operationId, string itemId, long price) => Task.FromResult(ledger.Purchase(operationId, itemId, price));
    }
}

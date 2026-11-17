using System;
using System.Linq;
using System.Threading.Tasks;
using DripKings.Economy;

public static class EconomyChecks
{
    public static int Run()
    {
        int passed = 0;
        Action<bool, string> check = (condition, name) => { if (!condition) throw new Exception(name); passed++; };
        var ledger = new CurrencyLedger();
        check(ledger.Balance == 0, "Starts at zero");
        ledger.Award("first", 100);
        ledger.Award("first", 100);
        check(ledger.Balance == 100 && ledger.History.Length == 1, "Reward retries are idempotent");
        ExpectFailure(() => ledger.Award("first", 200)); passed++;
        ExpectFailure(() => ledger.Award("bad", -100)); passed++;
        ExpectFailure(() => ledger.Award("bad", 0)); passed++;
        ExpectFailure(() => ledger.Award(" ", 100)); passed++;
        ExpectFailure(() => ledger.Purchase("buy", "crown", 250)); passed++;
        check(ledger.Balance == 100 && !ledger.Owns("crown"), "Failed purchase changes neither balance nor inventory");
        ledger.Award("second", 200);
        var receipt = ledger.Purchase("buy", "crown", 250);
        check(ledger.Balance == 50 && ledger.Owns("crown"), "Purchase debits and grants together");
        check(object.ReferenceEquals(receipt, ledger.Purchase("buy", "crown", 250)), "Purchase retry returns original receipt");
        ExpectFailure(() => ledger.Purchase("buy", "other", 250)); passed++;
        ExpectFailure(() => ledger.Purchase("duplicate-item", "crown", 1)); passed++;
        ExpectFailure(() => ledger.Purchase("free", "other", 0)); passed++;
        check(ledger.History.Sum(r => r.Delta) == ledger.Balance, "Journal reconciles");
        var history = ledger.History;
        history[0] = null;
        check(ledger.History[0] != null, "History cannot mutate ledger");
        var capped = new CurrencyLedger();
        capped.Award("max", long.MaxValue);
        ExpectFailure(() => capped.Award("overflow", 1)); passed++;
        check(capped.Balance == long.MaxValue && capped.History.Length == 1, "Overflow is atomic");
        var concurrent = new CurrencyLedger();
        Parallel.For(0, 100, i => concurrent.Award("same", 100));
        check(concurrent.Balance == 100 && concurrent.History.Length == 1, "Concurrent retries award once");
        Parallel.For(0, 100, i => concurrent.Award("award-" + i, 1));
        check(concurrent.Balance == 200, "Concurrent distinct awards reconcile");
        var sessionLedger = new CurrencyLedger();
        var session = new StreetRunSession(new LocalCurrencyService(sessionLedger));
        session.ReachCheckpointAsync(2).GetAwaiter().GetResult();
        check(session.NextCheckpoint == 0 && sessionLedger.Balance == 0, "Cannot skip checkpoints");
        session.ReachCheckpointAsync(0).GetAwaiter().GetResult();
        session.ReachCheckpointAsync(0).GetAwaiter().GetResult();
        check(session.NextCheckpoint == 1 && sessionLedger.Balance == 0, "Repeated checkpoint does not progress");
        for (int run = 0; run < 3; run++)
        {
            if (run != 0) session.ReachCheckpointAsync(0).GetAwaiter().GetResult();
            session.ReachCheckpointAsync(1).GetAwaiter().GetResult();
            check(session.ReachCheckpointAsync(2).GetAwaiter().GetResult(), "Route completion succeeds");
        }
        check(session.CompletedRuns == 3 && sessionLedger.Balance == 300, "Three routes award 300");
        session.BuyCrownAsync().GetAwaiter().GetResult();
        session.BuyCrownAsync().GetAwaiter().GetResult();
        check(sessionLedger.Balance == 50 && session.OwnsCrownAsync().Result, "Three runs buy crown with retry protection");
        var delayed = new DelayedCurrency();
        var delayedSession = new StreetRunSession(delayed);
        delayedSession.ReachCheckpointAsync(0).GetAwaiter().GetResult();
        delayedSession.ReachCheckpointAsync(1).GetAwaiter().GetResult();
        var waiting = delayedSession.ReachCheckpointAsync(2);
        check(!delayedSession.ReachCheckpointAsync(2).Result && delayed.Calls == 1, "Pending award prevents duplicate submission");
        delayed.Pending.SetException(new InvalidOperationException("Simulated service outage"));
        ExpectFailure(() => waiting.GetAwaiter().GetResult()); passed++;
        check(delayedSession.NextCheckpoint == 2 && delayedSession.CompletedRuns == 0, "Failed settlement preserves mission");
        delayed.Pending = new TaskCompletionSource<Receipt>();
        var retry = delayedSession.ReachCheckpointAsync(2);
        check(delayed.LastId == "street-run:1", "Retry retains settlement ID");
        delayed.Pending.SetResult(new Receipt(delayed.LastId, 100, 100, null));
        check(retry.Result && delayedSession.CompletedRuns == 1, "Settlement retry completes mission");
        return passed;
    }

    private static void ExpectFailure(Action action)
    {
        bool failed = false;
        try { action(); }
        catch (ArgumentException) { failed = true; }
        catch (InvalidOperationException) { failed = true; }
        catch (OverflowException) { failed = true; }
        if (!failed) throw new Exception("Expected rejection");
    }

    private sealed class DelayedCurrency : ICurrencyService
    {
        public TaskCompletionSource<Receipt> Pending = new TaskCompletionSource<Receipt>();
        public int Calls;
        public string LastId;
        public Task<long> GetBalanceAsync() => Task.FromResult(0L);
        public Task<bool> OwnsAsync(string itemId) => Task.FromResult(false);
        public Task<Receipt> PurchaseAsync(string operationId, string itemId, long price) => throw new NotSupportedException();
        public Task<Receipt> AwardAsync(string operationId, long amount)
        { Calls++; LastId = operationId; return Pending.Task; }
    }
}

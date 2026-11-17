# Drip Kings: Street Run — first playable slice

Built against the existing Unity 6000.3.2f1 prototype. This is an offline gameplay and currency foundation, not the finished multiplayer game.

## Play

1. Check out `feat/game-only-economy` and open this repository in Unity 6000.3.2f1.
2. Choose **Drip Kings → Open Street Run Prototype**, or open `Assets/DripKings/Prototype/StreetRun.unity`.
3. Press Play. Move with WASD, arrow keys, or the gamepad left stick.
4. Reach the three gold beacons in order. Each route awards 100 Kings Credits (KC).
5. Complete three routes, then use **Equip Neon Crown · 250 KC**. The crown appears on the runner and 50 KC remains.

The standalone scene leaves the existing Netcode player, scenes and Build Settings unchanged. There is no save file: stopping Play or restarting the scene resets progress. The gray beacons are inactive; the tall gold beacon is your next destination. Prototype art is made from primitives.

## Currency rules now

- Asset identity: `kings-credits-v1`. Display: Kings Credits / KC. Whole-number amounts, stored as signed 64-bit integers with checked arithmetic.
- Earn through gameplay; spend on game cosmetics. No purchase with money, exchange rate, withdrawals, external transfers, wallet connection, token minting or blockchain integration.
- Credits have no real-world value. There is no promise or entitlement to convert them into a future currency.
- Per-session journal, idempotent operation IDs, rejected conflicting retries, no overdrafts, and atomic debit/item grants. A repeated purchase returns the original receipt rather than charging again.
- The local ledger is a trusted-code boundary only, not an anti-cheat system. A modified client can change offline progress. Never accept these balances as production or financial records.

## Replaceable architecture

`StreetRunPrototype → StreetRunSession → ICurrencyService → LocalCurrencyService → CurrencyLedger`

Gameplay calls an asynchronous service interface. The local adapter is the only implementation enabled. Reward amounts and item prices live in session rules, not UI fields. The core has no Unity dependency and can run on a server. The current award/purchase methods are **authority-internal**, not public client API endpoints.

A future adapter can use a durable backend and a different asset identity without changing the movement or beacon logic. This is an extension point, not a live migration implementation or a claim that a financially valuable currency can safely be switched on with one setting.

Before multiplayer release:

1. Run mission validation, balances, prices, item grants and operation deduplication on a trusted server. Clients submit mission/item IDs, never arbitrary award amounts or prices. Validate checkpoint proximity against server-authoritative movement; the existing `PlayerMover` currently trusts client-supplied deltas and must be hardened first.
2. Add authenticated stable player IDs and durable transactional storage. Persist journal, inventory and idempotency receipts together, scoped by player and currency. Use unique constraints and database transactions; an in-memory lock is insufficient across server processes.
3. Add reconnect recovery, reconciliation, rate limits, inventory synchronization, telemetry and server-side reward abuse checks. Do not trust an offline save or NGO connection ID as permanent account identity.

If a real digital currency is later chosen:

1. Define a new asset ID, supported network, units/decimals, custody, settlement and failure/reversal policy. Preserve integer quantities; do not reuse floating point amounts or silently reinterpret KC decimals.
2. Implement a separate server adapter with durable pending/confirmed/failed settlement records and reconciliation. Never embed signing keys in a game build.
3. Explicitly decide whether old KC remains separate, resets, or qualifies for a reviewed migration. No automatic 1:1 mapping exists. Never import untrusted local playtest balances.
4. Keep any migration disabled until its conversion rules and release requirements are deliberately approved and tested. Use snapshots, unique claims and an audit trail if a migration is ever implemented.

## Verification

- Run `bash Tests/Economy/run-tests.sh` with the .NET 8 SDK. It compiles the actual C# core and runs the same checks used by the Unity EditMode wrapper, without external packages.
- In Unity Test Runner, run `EconomyTests.CurrencyAndMissionInvariants` in EditMode.
- Checks cover duplicate/concurrent rewards, conflicting operation IDs, invalid amounts, overdrafts, overflow atomicity, inventory consistency, journal reconciliation, checkpoint ordering, delayed settlement, retry after service failure and the three-route crown purchase.
- Manual Unity acceptance: open the scene, verify all input options, run three routes, buy the crown, verify 50 KC remains, try a second purchase, and restart to confirm the session reset disclosure.

The C# core has been executed independently. Unity Editor, graphics, Input System and player builds have not been executed in this workspace; those checks remain required before calling this a tested Unity build.

## Next playable milestone

Replace the capsule with the chosen character and camera, then add persistent server-owned accounts, authoritative movement and a two-player mission test. Keep Kings Credits isolated from the separate DRIP token project throughout this phase.

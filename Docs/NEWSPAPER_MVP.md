# Drip Kings Gazette — first working classifieds economy

## Goal
The Newspaper is the central entrypoint for jobs, vehicle parts, paid mechanic labor, paint services, and businesses for sale. This first milestone is **single-player local simulation using in-game credits**. Nothing here is a live marketplace, player-to-player transaction, real-money charge, or token transfer.

## Player flow
1. Start the game -> START -> NEWSPAPER.
2. **PARTS FOR SALE**: buy an Intake (60 credits), Brake Kit (85), or Performance Tires (110). Purchased parts become OWNED.
3. Use INSTALL for an owned part to hire a mechanic and pay installation labor. Installation is saved; repeat purchases and duplicate installation charges are blocked.
4. **MECHANIC SERVICES**: custom paint costs 25 credits and uses the shared Garage color save. Link back to parts to purchase installation.
5. **JOBS + PROPERTY**: apply for a mechanic role (saved as a local application only, no simulated wages yet) or buy a mechanic shop (500 credits).
6. Owning a mechanic shop unlocks 25% discounted wholesale part pricing and free self-installation skill.
7. Reopening the Newspaper shows saved ownership/application state. The same local credits are shared with the Garage and race rewards.

## Architecture
- `Assets/Scripts/MVP/ClassifiedsMarket.cs`: catalog, prices, credit debits, ownership, installation, shop, and application flags.
- `Assets/Scripts/MVP/DripKingsNewspaperView.cs`: the classifieds UI tabs and purchase controls.
- `Assets/Scripts/MVP/DripKingsMenuController.cs`: persistent menu routing.
- `Assets/Editor/ClassifiedsSmokeTests.cs`: automated transaction smoke tests with exact PlayerPrefs backup/restore.

## Not yet implemented
- Real player-created listings, server authoritative balances, buyer/seller escrow, shops as transferable property, offline/online order fulfillment, genuine job acceptance/wages, component installation affecting car performance, NFT/token exchange, real-money payments, asset ownership transfer.
- We must introduce backend/server validation and transaction history **before** multiplayer or anything with real-world value. Never trust a Unity client to set balance or ownership.

## Verification
Run the Editor test (Unity 6000.3.2f1 on Windows with the project closed in the Editor):

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'C:\Users\cmhcm\DripKings_prototype' -executeMethod DripKings.EditorTools.ClassifiedsSmokeTests.Run -logFile 'C:\Users\cmhcm\DripKings_prototype\Logs\classifieds-smoke.log'
```

Require `DRIP_KINGS_CLASSIFIEDS_PASS` and a Unity exit code 0 before reporting a pass. Rebuild a separate timestamped Windows preview (never overwrite the live executable), navigate each tab, buy a part, install it, reopen the game and check saved states. The automated test restores all preexisting saved credits, paint and Newspaper state; manual purchases change the local game save.

## Verified November 25, 2026 (HP / Unity 6000.3.2f1)
- Newspaper purchase/ownership/installation/job/shop tests: `DRIP_KINGS_CLASSIFIEDS_PASS: 20 assertions passed` with Unity exit code 0.
- Existing race and Garage regression tests: `DRIP_KINGS_MVP_SMOKE_PASS: 25 assertions passed` with Unity exit code 0.
- Windows build produced a distinct executable in `Builds/MVP-preview-20261126-024012` with `Build Finished, Result: Success.`
- Actual Windows app was opened; screenshots confirm START -> hub -> NEWSPAPER and all 3 tabs visible (parts, services, jobs/property).
- No purchases were clicked during the visual check, to preserve the operator's own game save; simulated transactions were exercised only in tests which restore PlayerPrefs.
- Steering root-cause discovered: the Unity racing scene serialized outdated 115 degrees/sec and grip 9. The scene was edited to 76 degrees/sec and grip 11, with smoothed steering and Space e-brake.
- A second timestamped build is required to verify the updated serialized steering values (the first build predates the scene correction).

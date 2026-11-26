# Drip Kings Tablet | Player-facing app hub

**Decision made:** 2026-11-26 (America/Chicago)
**Scope:** Offline MVP in Unity 6000.3.2f1 and C#; same `feature/mvp-driving-sprint` branch.

## What changed
The old **NEWSPAPER** launcher on the player hub becomes a **TABLET**. The tablet is a device-style launcher; players pick an app to interact with the automotive economy.

| Tablet app | Purpose | What works in the offline prototype |
| --- | --- | --- |
| **D-BAY** | Player classifieds and marketplace | Browse/buy three seeded prototype parts using game credits. True player listings, auctioning, selling and escrow are not connected. |
| **DEP BOYS** | Auto service, repairs, installation, custom paint | Purchase saved Garage paint or install a previously purchased part for local game-credit labor. Mechanic shop owners have zero labor cost in this prototype. |
| **DAUTO ZONE** | Retail automotive parts catalog | Buy from the same inventory catalog, synced with D-Bay purchase ownership. Later: stock levels, make/model fitment and pricing. |
| **DK JOBS** | Mechanics hiring and business/property opportunities | Save one local mechanic job application; buy a simulated mechanic shop, which unlocks wholesale pricing. Actual hiring/wages/shop income not yet implemented. |
| **DK GAZETTE** | Game newspaper, news, race/event announcements | Static offline bulletin and future event placeholder. No live server feed. |

The app names are **working fictional brands**. Do not import third-party logos, trade dress or artwork. Review trademark clearance before distributing the game.

## Navigation acceptance tests
1. START -> main menu -> TABLET opens tablet home, not the old Gazette list.
2. Home displays D-BAY, DEP BOYS, DAUTO ZONE, DK JOBS and DK GAZETTE app icons.
3. OPEN enters each app, HOME returns to tablet home. ESC from an app returns home; ESC from home returns to the main hub.
4. Each app shows one shared, saved game-credit balance.
5. A part bought in D-BAY is owned in DAUTO ZONE, and can be installed through DEP BOYS; repeat purchases are blocked.
6. Garage paint and shop/job statuses retain their existing PlayerPrefs keys. None of these features are real-money or cross-player.
7. Back/Close Tablet restores the hub and, when a race is active, pause/resume behavior remains unchanged.
8. Run `DripKings.EditorTools.TabletSmokeTests.Run` and verify its PASS marker alongside existing classifieds and race tests.
9. Build into **new** `Builds/MVP-preview-<timestamp>` folder; never overwrite the known-good executable before visual QA.

## Runtime architecture
- `Assets/Scripts/MVP/DripKingsTabletView.cs`: tablet UI, app navigation and informational screens.
- `Assets/Scripts/MVP/DripKingsMenuController.cs`: open tablet from hub, ESC navigation and return.
- `Assets/Scripts/MVP/ClassifiedsMarket.cs`: shared local catalog, credits and purchase/install/job/shop operations.
- `Assets/Scripts/MVP/DripKingsNewspaperView.cs`: retained legacy view for history/rollback; no longer attached by the menu controller.
- `Assets/Editor/TabletSmokeTests.cs`: route and commerce/regression smoke checks.
- `Assets/Editor/MvpWindowsBuild.cs`: timestamped development build.

## Next app milestones
- **D-BAY:** searchable categories, player-created listings, buyer/seller stock, seller identification, order history and atomic order fulfillment using server-backed records when online.
- **DEP BOYS:** mechanic qualifications, labor quotes, schedules, installation choices and actual car-part effects.
- **DAUTO ZONE:** compatible vehicle part catalog, visual thumbnails, prices, stocks, supply/distribution.
- **DK JOBS:** application review, shift completion, mechanic wages, shop property lifecycle.
- **DK GAZETTE:** event feed, clubs, ads, parts requests, district news.
- **Cross-app:** alert badges, digital wallet for in-game credits, saved order history, inventory tabs, repair requests.

## Safety
These are fiction-inspired shop names and locally simulated transactions. No actual eBay, Pep Boys or AutoZone integration exists; do not imply a commercial affiliation. **Do not** put any AWS credentials, server purchasing authority, cryptocurrency logic, or private keys in the Unity client. Add real multi-user commerce only after a server-authoritative ledger and explicit approval.

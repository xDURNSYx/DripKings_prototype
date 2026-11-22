# Drip Kings — Front-end navigation (offline MVP)

## Player flow
- Launching the Windows player opens `DripKings_StartMenu` first, showing DRIP KINGS branding and START.
- START (or Enter) opens the central hub with **RACE**, **GARAGE**, and **NEWSPAPER**.
- RACE loads `DripKings_MVP_Test` and enables driving, checkpoint timing, and rewards.
- In a race, the top-right **MENU [ESC]** button opens the hub and pauses simulation. RACE then resumes.
- GARAGE shows local credits and the selected paint style. A 25-credit paint purchase saves locally; colors are applied when racing.
- NEWSPAPER opens the **Drip Kings Gazette** placeholder, with no fabricated live articles or network dependency.
- BACK TO MENU returns from GARAGE or NEWSPAPER to the hub; Escape also navigates back.

## Design
- Dark navy/charcoal panels, amber accent, DRIP KINGS text branding, and DURNS production mark.
- Built-in Unity IMGUI; no third-party fonts, image assets, plugins, or cloud services.
- Currency is local game credits with **no real-world value**.

## Implementation
- Runtime: `Assets/Scripts/MVP/DripKingsMenuController.cs`.
- Scene generator: `Assets/Editor/CreateDripKingsStartMenu.cs`.
- Build settings: startup menu first, racing scene second, previous Unity sample scenes retained.
- Windows build: `DripKings.EditorTools.MvpWindowsBuild.BuildMenuPreview` first built a separate preview. After verification, the new menu build was moved to `Builds/MVP/DripKings.exe`. The previous playable build was preserved at `Builds/MVP-pre-menu-20261122/DripKings.exe`.

## Verification checklist
- [x] Windows executable opens with the branded title rather than the race.
- [x] START opens the hub with RACE / GARAGE / NEWSPAPER.
- [x] GARAGE displays saved local credits and the paint purchase control (purchase itself not exercised).
- [x] NEWSPAPER displays the placeholder and returns to hub.
- [x] RACE loads the drivable scene from the hub.
- [x] MENU pauses and returns to the hub; RACE resumes the game.
- [x] Unity 6000.3.2f1 checkpoint smoke: `DRIP_KINGS_MVP_SMOKE_PASS: 6 assertions passed`; exit code 0.
- [x] Windows menu build: `DRIP_KINGS_MENU_BUILD_PASS`; batch Unity exit code 0.

Verified on 2026-11-22 on the connected HP. Screenshots and logs are in the local `Logs` folder. The garage purchase transaction, full driving course/lap completion, and save/reload after purchase still need dedicated manual testing.

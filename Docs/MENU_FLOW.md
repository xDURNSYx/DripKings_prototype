# Drip Kings — Front-end navigation (offline MVP)

**November 26, 2026 update:** The former NEWSPAPER hub button now opens the **TABLET** launcher with D-BAY, DEP BOYS, DAUTO ZONE, DK JOBS and DK GAZETTE apps. See `Docs/TABLET_APPS.md` for the current layout and navigation. The NEWSPAPER-specific flow below describes the older prototype for historical reference.

## Player flow
- Launching the Windows player opens `DripKings_StartMenu` first, showing DRIP KINGS branding and START.
- START (or Enter) opens the central hub with **RACE**, **GARAGE**, and **NEWSPAPER**.
- RACE loads `DripKings_MVP_Test` and enables driving, checkpoint timing, and rewards.
- In a race, the top-right **MENU [ESC]** button opens the hub and pauses simulation. RACE then resumes.
- GARAGE shows local credits, a rotating offline 3D showcase model, and the selected paint style. A 25-credit paint purchase saves locally; paint updates the preview and is applied to the racing vehicle. This simple showroom model is a temporary low-poly stand-in, not the final vehicle asset.
- NEWSPAPER opens the **Drip Kings Gazette** placeholder, with no fabricated live articles or network dependency.
- BACK TO MENU returns from GARAGE or NEWSPAPER to the hub; Escape also navigates back.

## Design
- Dark navy/charcoal panels, amber accent, DRIP KINGS text branding, and DURNS production mark.
- Built-in Unity IMGUI; no third-party fonts, image assets, plugins, or cloud services.
- Currency is local game credits with **no real-world value**.

## Implementation
- Runtime navigation: `Assets/Scripts/MVP/DripKingsMenuController.cs`.
- Runtime preview: `Assets/Scripts/MVP/GarageVehiclePreview.cs` creates a private 3D showroom model and RenderTexture, uses existing saved paint colors, and rotates the model without changing the race car.
- Scene generator: `Assets/Editor/CreateDripKingsStartMenu.cs`.
- Build settings: startup menu first, racing scene second, previous Unity sample scenes retained.
- Windows build: `DripKings.EditorTools.MvpWindowsBuild.BuildMenuPreview` first built a separate preview. After verification, the new menu build was moved to `Builds/MVP/DripKings.exe`. The previous playable build was preserved at `Builds/MVP-pre-menu-20261122/DripKings.exe`.

## Verification checklist
- [x] Windows executable opens with the branded title rather than the race.
- [x] START opens the hub with RACE / GARAGE / NEWSPAPER.
- [x] GARAGE displays saved local credits and the paint purchase control (purchase itself not exercised).
- [x] GARAGE 3D model rendered in the Windows preview; successive screenshots confirm rotation and the stored yellow paint style (4/5).
- [x] NEWSPAPER displays the placeholder and returns to hub.
- [x] RACE loads the drivable scene from the hub.
- [x] MENU pauses and returns to the hub; RACE resumes the game.
- [x] Unity 6000.3.2f1 checkpoint smoke: `DRIP_KINGS_MVP_SMOKE_PASS: 6 assertions passed`; exit code 0.
- [x] Windows menu build: `DRIP_KINGS_MENU_BUILD_PASS`; batch Unity exit code 0.

Verified on 2026-11-22 on the connected HP. Screenshots and logs are in the local `Logs` folder. The garage purchase transaction, full driving course/lap completion, and save/reload after purchase still need dedicated manual testing.

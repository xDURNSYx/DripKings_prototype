# Drip Kings: Codex instructions and execution handover

Updated 2026-10-15. Read this before changing the project. This is a recovered Unity game, not a new engine migration. Continue authorized reversible work, verify results, and report blockers honestly.

## Goal and scope
Recover a reliable build, then deliver a small playable driving/drifting slice. Preserve the long-term game blueprint without treating planned features as shipped. Use the existing project unless evidence shows it cannot be recovered. Authorized spending is $0. Do not upgrade Unity, migrate accounts/repositories, publish releases, buy services, or perform crypto transactions without specific authorization.

DRIP is a separate meme token with NO game utility or promised future utility. The game may later evaluate an EXISTING working payment asset. USDC on Solana is a research candidate, not an approved or implemented integration. Use ordinary test credits for the playable slice. Crypto must not block driving development. Book publishing is a separate parallel workstream; no publishing progress is verified here.

## Verified baseline
- Repository: xDURNSYx/DripKings_prototype, public, default branch main.
- Baseline commit: 5e9b0eae1b93c69b121142620fc3668b0e606f20.
- Unity 6000.3.2f1, revision a9779f353c9b. Project root contains Assets, Packages and ProjectSettings.
- NGO 2.7.0; Unity Transport 2.6.0; Input System 1.17.0; Cinemachine 3.1.2; URP 17.3.0; Test Framework 1.6.0.
- Existing cloud project ID in ProjectSettings.asset: bde1dbf8-8164-492e-b97b-dcbde1717b0e. A similarly named new cloud project also exists. Do not silently relink the game.
- Static GUID/version checks passed. No Unity import, compilation, runtime networking, driving loop or packaged build has been verified during recovery.
- Earlier cloud checkouts were sparse. Inspect git sparse-checkout list and restore all required tracked assets before any build; sparse omissions are not source defects.

## Source map
| Area | Existing path / behavior |
| --- | --- |
| Build entry | Assets/Editor/BuildScript.cs: BuildScript.BuildWindows |
| Build output | Builds/Windows/DripKings.exe by default; optional -buildName |
| Build scenes | Enabled EditorBuildSettings scenes; do not replace with a guessed SampleScene |
| Main bootstrap | Assets/Scenes/NetcodeBootstrap.unity |
| Additional enabled scenes | Terminal, Oasis, Garden and Cockpit scene paths in ProjectSettings/EditorBuildSettings.asset |
| Network setup | Assets/Scripts/NetcodeBootstrap/NetcodeBootstrap.cs |
| Player prefab | Assets/Prefabs/NetcodeBootstrap/PlayerCube.prefab |
| Network test scene | Assets/DripKings_MVP/NetcodeTest.unity |
| Legacy automation | .github/workflows/queue-unity-job.yml and _automation/ |

The bootstrap sets NetworkManager/UnityTransport at runtime and defaults to 127.0.0.1:7777. Zero serialized manager references alone do not prove a defect. Preserve the assigned player prefab and all .meta GUIDs. Review queue: PlayerMover accepts client-calculated displacement; disconnected handler is defined but not registered. These are static findings, not reproduced runtime failures.

## Current execution blockers
1. Sandbox Unity CLI 1.0.0-beta.13 runs, but sign-in completion was blocked by network policy reaching api.unity.com. Phone/browser login is not CLI authentication. Do not bypass policy or invent activation.
2. The exact Editor install dry run resolved 6000.3.2f1; the Editor itself has not been installed. A valid appropriate license and accepted terms are still required before local/headless Editor execution. No license bypasses.
3. Unity Dashboard browser access now works. Build Automation account allowance showed Windows 0/200 min, Linux 0/100 min, Mac 0/100 min, storage 0/25 GB and egress 0/100 GB. Pay-as-you-go is active: these numbers are a dated observation, not a spending cap. Recheck before any build.
4. Unity Manual Git rejected both canonical HTTPS and SSH repository URLs as private/inaccessible, although GitHub metadata and fresh git ls-remote confirm the repository is public/reachable. Root cause is unproven.
5. User approved adding Unity's generated read-only deploy key. GitHub rejected the submission: key too short, recommends RSA at least 3072 bits. NO key was added. Do not claim the source connection is saved. Do not regenerate a team key without assessing existing consumers; prefer a supported minimally scoped connection.
6. GitHub browser owner access works. The separate ChatGPT GitHub connector is signed in as Durnsy and has read-only access to this repository. Neither proves Codex has remote write access. Verify Codex's actual account/repository permissions separately.

## Branch and execution rules
- Work on a named task branch, preserve main, show diffs and create reviewable commits/PRs.
- IMPORTANT: pushes to main trigger the legacy Command Center workflow, which posts a build job to an old HTTP endpoint using a repository secret. Do not push/merge main or dispatch that workflow until its ownership, intended behavior and security have been reviewed. Never print that secret or probe the old endpoint.
- Keep generated Library/Temp/Obj/build caches and credentials out of Git. Preserve source .meta files. No blind deletes, renames, GUID rewrites or bulk reformatting.
- Start with git status and project/version inspection. Use the actual shell (PowerShell on Windows, Bash on Linux). Record commit, exact command, exit status, timestamped sanitized logs and output artifact for meaningful checks.
- Source editing and static review may continue without Unity. Mark Unity-dependent acceptance BLOCKED until run in a licensed exact-version Editor. Do not substitute a standalone C# compile for a Unity compile.
- Headless Editor uses -batchmode -nographics. Existing build entry: -quit -projectPath <root> -executeMethod BuildScript.BuildWindows -logFile <timestamped-log>. It uses enabled scenes and its own output path; generic output flags do not override that method.
- Headless build success does not prove rendering, input feel, drift handling or multiplayer runtime correctness.

## Ordered work plan and acceptance gates
| Task | Work | Exit evidence |
| --- | --- | --- |
| DK-01 | Restore repo/context; preserve handover on a work branch | Inspectable commit; exact version and source map |
| DK-02 | Establish Codex repository read/write access | Open repo/branch, read this file, write a small reviewed branch commit or PR; no secrets copied |
| DK-03 | Resolve Unity cloud source authentication; configure manual Windows x64 Micro build, main or reviewed work branch, project root, 6000.3.2f1; automatic triggers off | Saved settings, free allowance/timeout checked; no unauthorized billing |
| DK-04 | Import/compile baseline and package Windows player | Exact commit, successful log and fresh downloadable artifact; record actual errors first |
| DK-05 | Reproduce bootstrap host/client, spawn/move/disconnect/reconnect behavior | Runtime evidence on two instances; targeted fixes plus suitable verification |
| DK-06 | Inspect/reuse vehicle assets; implement one car, camera, compact track, restart and basic drift scoring | Playable local driving loop, manual handling check and relevant automated checks |
| DK-07 | Add checkpoint/timer race, minimal HUD, garage cosmetics/basic upgrades and local save | Race/save/restart acceptance and packaged smoke test |
| DK-08 | Optimize, document controls and known issues, prepare reviewed demo | Profiler evidence, reproducible build and release checklist; no automatic public release |

Planning horizon: first establish DK-01 through DK-05, then build the local driving slice, then progression/UX, then packaging and review. These are dependency milestones, not a guarantee of a finished game on a date. Multiplayer production, payments and large-world features follow a verified slice.

## Long-term blueprint (planned, not shipped)
Tokyo/San Francisco-inspired cities; roads/traffic/weather/day-night/GPS; social hubs. Realistic driving/drift; sprint/drag/circuit racing; police pursuits/fines/stealth/radar. Performance upgrades and electric/hybrid concepts; paint/decals/body kits/neon and collectible cars/parts. Multiplayer lobbies, authoritative results, accounts/persistence, proximity communication, clubs/teams, rankings, events/tournaments, replays/sharing. Economy/progression, inventory and garages/property/business concepts need separate design and legal/platform review before real-money mechanisms. NFTs, cash conversion, staking/leasing or betting are unapproved backlog concepts, not promised income. Later platforms/cross-play/VR need explicit scope decisions.

## Seven specialist roles
Commander owns scope, integration, acceptance and records. Divide independent tasks only when available/authorized concurrency permits; a prompt is not seven running workers.
1. Unity/systems: architecture, packages, scenes, input/save, tooling.
2. Vehicles/racing: handling, drift, camera, checkpoints, timing/rules.
3. World/art/audio: roads/city, traffic/weather/lighting, police AI, asset performance.
4. Multiplayer/backend: lobby/spawn/sync, server authority, accounts/persistence, abuse controls.
5. UX/customization: menus/HUD, garage, upgrades/cosmetics, settings/accessibility.
6. Economy/ownership: test credits/sources/sinks, inventory and optional payment research; keep DRIP separate.
7. QA/release/operations: compile/runtime/build gates, profiling, asset licenses, sanitized logs, publishing coordination.

## Continuity and first next task
For each task record ID/owner/objective/allowed files/dependencies/acceptance, then files changed, tests actually run, limits, rollback commit and next action. Statuses: PLANNED, READY, IN PROGRESS, BLOCKED, IMPLEMENTED, VERIFIED. Update a dated repository handover when state changes and the connected Google Docs master when available. Never put credentials, auth links or license material in either. Hourly progress reporting was scheduled for the top of the hour in America/Chicago; reporting is not proof of a continuous worker or live Unity monitoring.

Start by reading this file and the current branch diff. Verify branch write access; then resolve DK-03 with supported read-only source authentication. If blocked on access, continue an independently useful source review and record the precise dependency. Do not start a new project just to hide an authentication problem.

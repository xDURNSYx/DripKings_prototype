# Drip Kings offline MVP verification

Status: **not yet compiled, played, or built in Unity**. These are reproducible local steps, not completed test results.

Unity project version: 6000.3.2f1. Use the development branch `feature/mvp-driving-sprint` in a clean working tree; do not overwrite local changes.

## Editor smoke test
1. Open this project in the matching Unity Editor, allow imports to complete, and resolve any Console compile errors.
2. Select **Drip Kings > MVP > Run Smoke Tests**.
3. Require the Console message `DRIP_KINGS_MVP_SMOKE_PASS: 6 assertions passed`; exceptions are failures.

## Manual driving test
1. Select **Drip Kings > MVP > Create Test Scene**. This does not overwrite an existing scene and prompts to save modified scenes.
2. Open `Assets/DripKings_MVP_Test.unity` and enter Play Mode.
3. Drive using WASD/arrows; Space reduces grip; Shift brakes; R resets car and lap.
4. Cross checkpoint triggers in order 0, 1, 2, 3. Confirm timer finishes, exactly 50 local credits are awarded, and G spends 25 credits to repaint.
5. Restart and check timer/checkpoint state; quit/re-enter Play Mode and confirm local credit/paint persistence.
6. Record actual outcomes, Unity Console errors, and Editor version. The course is only a provisional test layout.

## Batch smoke test (Windows PowerShell)
Replace `<UNITY_EDITOR_EXE>` with the installed Unity.exe path and run outside an active Editor session:

```powershell
& "<UNITY_EDITOR_EXE>" -batchmode -nographics -quit -projectPath (Get-Location).Path -executeMethod DripKings.EditorTools.MvpSmokeTests.Run -logFile ".\mvp-smoke.log"
$LASTEXITCODE
```

Check both exit code and `mvp-smoke.log` for the PASS marker. An exit code alone does not establish passing assertions. A playable Windows build and multiplayer test have **not** been verified.

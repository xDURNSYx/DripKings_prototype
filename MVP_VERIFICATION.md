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

## Windows development build (not yet executed)
1. Run the smoke test and inspect the Console. Save the generated MVP test scene.
2. Select **Drip Kings > MVP > Build Windows Development Player**.
3. A successful build logs `DRIP_KINGS_MVP_BUILD_PASS` and creates `Builds/MVP/DripKings.exe` with supporting Unity data.
4. If `Builds/MVP` already exists, rename or move it first; the build command intentionally refuses to overwrite it.
5. Run the executable locally, complete one race, repaint the car, restart, and record outcomes. A build success alone does not prove gameplay.

For a headless build, run the Unity Editor with `-batchmode -nographics -quit -projectPath <PROJECT_ROOT> -executeMethod DripKings.EditorTools.MvpWindowsBuild.Build -logFile <LOG_PATH>`. Verify the log marker and output files. A Unity Editor license and Windows build support are required. This does not publish or upload the game.

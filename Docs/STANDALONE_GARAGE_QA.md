# Standalone Garage QA — Windows

Purpose: manually verify the built game's Garage purchase and paint persistence without sacrificing real save data. Automated smoke assertions are not a substitute for this check.

## Preconditions

- Confirm `git status --short`, local/remote branch HEAD, and active Unity/Codex sessions before editing or building.
- Use a separate Windows user profile or an isolated test machine/account. **Do not delete, clear, or overwrite the operator's PlayerPrefs or saved progress.**
- Record the executable's full path, modification time, build log, Unity commit, and test profile.
- Test `Builds/MVPMenu/DripKings.exe` explicitly; a running `Builds/MVP/DripKings.exe` is **not** evidence that the newer menu build was launched.

## Manual acceptance sequence

1. Start the executable; confirm the branded Start screen appears before gameplay.
2. Press Start; verify Race, Garage, and Newspaper navigation and Back actions.
3. Enter Garage; record displayed credit balance, selected paint, and vehicle preview color.
4. If the isolated profile has enough credits, purchase a paint; verify balance decreases exactly once, selected paint and preview update, and the purchase remains selected after navigating away and back.
5. Try a purchase with insufficient credits; verify no credit deduction or paint unlock occurs.
6. Exit and relaunch the same build in the isolated profile; confirm credits and selected paint persist.
7. Enter Race, complete checkpoints, and verify the reward is granted only once for the intended completion event. Return to Garage; confirm reward credits and selected paint are both correct.
8. Check pause/resume and restart behavior, and confirm the Garage state is not unexpectedly reset.
9. Record pass/fail, exact reproduction steps, screenshots, log paths, and the tested executable path.

## Release gate

Do not mark standalone Garage persistence as verified until steps 1–9 are actually observed and recorded. Preserve previous builds, `_Recovery`, and unrelated untracked files. Never merge to `main`, publish, or enable real-money systems as part of this QA.

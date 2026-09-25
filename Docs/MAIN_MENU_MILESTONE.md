# Main menu milestone

**Current update:** use STARS_AND_PLATFORM_CHALLENGES.md and its combined ZIP. This file records the earlier milestone; the latest JSON and challenge guide define current layouts and scoring.


**Combined integration:** the newer THREE_LEVELS_AND_MENU.md supersedes these standalone setup steps when using Hat_Hop_Menu_And_Three_Levels.zip. Use **Create Menu and Three Levels** to generate everything together.

Target: Unity 6000.3.23f1, 2D Built-In. This is a source patch; Unity import and playtesting are still required.

## Outcome

A separate MainMenu scene with Play, Level Select and Controls. Play uses the existing GameplayTest until Easy is assigned. Easy, Medium and Hard are visible but disabled with Coming Soon labels until their scenes are assigned and enabled in the build list. GameplayHUD gains Main Menu in the bottom-right, including during death/win. Returning abandons the current run; Play starts a fresh run.

The menu uses Unity UI (uGUI), a scaled Canvas, standard buttons, a hopping geometric rabbit and a hat illustration. All decoration is generated from UI rectangles and the built-in font. The menu's decorative hop is independent from the gameplay player's chosen 0.4 visual hop height. Keyboard navigation uses arrow keys and Enter; mouse clicks are the primary path. There are no touch movement controls or pause screen in this milestone.

## Import and setup

1. Stop Play Mode and save GameplayTest. Keep its current Player Hop Visual height at 0.4. If 0.4 was changed only during Play Mode, set it again outside Play Mode and save.
2. From your project terminal, check `git status`. If the movement checkpoint is saved and you are still on feature/movement-camera, run `git switch -c feature/main-menu`. If that branch already exists, switch to it without `-c`. Do not switch to an older main that lacks the tested movement update.
3. Extract Hat_Hop_Main_Menu_Milestone.zip outside the project. Copy its **Assets**, **Docs** and **README.md** into the project root; merge folders and replace the included files. The patch contains no .git, Packages, ProjectSettings or .unity files. Existing gameplay scenes and movement scripts are preserved. GameplayHUD.cs is the only replaced gameplay script; retain its existing .meta.
4. Return to Unity and wait for compilation. The menu requires Unity UI (com.unity.ugui), normally installed in the template, and uses the project's existing Input System when enabled. If Unity reports errors, copy the first complete red error before proceeding.
5. Select **Hat Hop > Create Main Menu Scene**. Accept saving the current scene if prompted. This creates and opens `Assets/HatHop/Scenes/MainMenu.unity`, and creates `Assets/HatHop/Settings/LevelCatalog.asset`. A second run asks before replacing MainMenu; it preserves the existing catalog and all gameplay scenes.
6. Open **File > Build Profiles > Scene List**. Confirm MainMenu is first and enabled, with GameplayTest also enabled. The generator updates the global scene list. If the active profile has **Override Global Scene List** checked, include those scenes in that profile too, with MainMenu first. Do not regenerate GameplayTest or rerun the movement setup for this milestone.
7. Return to MainMenu, press Play and click inside the Game view.

## Focused checks

| Action | Expected result |
| --- | --- |
| Open MainMenu | Title, animated rabbit, Play, Level Select and Controls; no red Console errors. |
| Controls, then Back | Correct A/D, Space and R instructions; return to home. |
| Level Select | Easy/Medium/Hard show Coming Soon and cannot be selected yet; Back works. |
| Play | Existing GameplayTest loads, with the saved movement, camera and 0.4 hop setting. |
| Main Menu in gameplay | Menu loads and the old gameplay run is discarded. |
| Repeat Play > Main Menu three times | Each run starts fresh; no duplicate camera, EventSystem, timers or controls. |
| Return during a flip and after winning | Menu remains usable; next Play starts normally. |
| Rapid repeated Play clicks | Only one scene load request is accepted. |
| Arrow keys + Enter on menu | Enabled buttons can be selected/activated; disabled difficulty buttons are skipped. |
| Resize Game view, including 16:9 and 4:3 | Entire menu remains visible; labels and buttons are readable. |

Once Editor checks pass, make a **new localhost Web Build And Run** from MainMenu and verify Play and Main Menu there too. This is local testing, not deployment. Existing browser builds do not include these changes.

## Connect levels later

In the Project window select `Assets/HatHop/Settings/LevelCatalog.asset`. Its Inspector has SceneAsset fields for Prototype, Easy, Medium and Hard. Drag each real scene to the corresponding field once it exists, then click **Refresh Menu Build Scenes**. Keep Prototype as GameplayTest while testing. Play automatically prefers Easy once assigned. A missing/unbuilt scene is disabled, not silently replaced by a different difficulty. Refresh registration after moving or renaming a mapped scene and reassign the picker if needed; paths do not follow asset moves automatically.

LevelCatalog is the single mapping source. New playable scenes should include the existing LevelFlow and GameplayHUD so return navigation works. Next Level completion behavior will be implemented when the real scenes exist. Rebuilding the menu is not necessary to change mappings.

If Play is disabled: verify Prototype is GameplayTest, the scene exists, and it is enabled in the active build scene list. If Main Menu reports unavailable: verify MainMenu is enabled there too. A build profile override can make its list differ from the global list updated by the setup command.

## Git checkpoint after testing

Stop Play Mode and save any scene changes, then run:

```sh
git add Assets/HatHop Docs README.md ProjectSettings/EditorBuildSettings.asset
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Add main menu and configurable level selection"
git push -u origin HEAD
git status
```

Include the generated MainMenu scene, LevelCatalog asset and all their metadata. If your custom Build Profile lives elsewhere under Assets, review and stage that changed profile explicitly as well. Review other ProjectSettings changes individually. Do not commit Builds/Web or Unity caches. Keep public deployment on hold until all three levels and menu are ready.

## Review notes and references

Authored: five new C# scripts with metadata, GameplayHUD navigation changes, and docs. The generator is isolated from gameplay geometry and motor tuning. Runtime level catalog is serialized into MainMenu through an explicit reference; no persistent singleton or Resources load is needed. SceneNavigation validates enabled scene paths, prevents duplicate loads and resets its static state when entering Play Mode. This environment cannot compile or run Unity; all acceptance rows above are pending user testing.

- [Unity Canvas Scaler](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html): Expand scaling fits the reference layout into different aspect ratios.
- [Unity Input System UI module](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html): the generated EventSystem uses default UI actions with the active Input System backend.
- [Unity scene loading](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html): scenes are loaded asynchronously in Single mode.
- [Build Profile scene override](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Build.Profile.BuildProfile-overrideGlobalScenes.html): profile-specific scene lists can override the global scene list.

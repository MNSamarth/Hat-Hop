# Hat Hop Unity Development Guide

Working reference for USC CSCI 526. Updated September 24, 2026.

Read this guide and PROGRESS.md before changing the project. The latest explicit user decisions take priority over the supplied design reference. In particular, grounded movement with visual hopping now replaces automatic physical hopping. Earlier milestone documents are historical records.

## 1. Goal and scope

Build a 2D vertical platformer where warned, game-controlled 180-degree map rotations turn climbs into controlled descents. The rabbit escaping a magician's hat is the theme; movement and level geometry must communicate the game without story, elaborate art or cutscenes.

The user has tested the revised movement and closer camera in GameplayTest. The user now requested one combined delivery of the main menu and all three larger levels, to integrate and test together. Browser Web builds on GitHub Pages remain the delivery target. Public deployment is deferred until the movement and game are ready; the initial local browser build has passed according to the user.

Avoid enemies, inventory, combat, collectibles, fall damage and new platform mechanics unless playtests establish a need. Use self-created assets under the assignment requirements recorded in the design reference.

## 2. Current gameplay rules

| Status | Rule |
| --- | --- |
| Agreed revision | A/D moves the grounded collision body and steers it in the air. |
| Agreed revision | Small hops are visual animation only. The collision body stays grounded until a real jump or fall. |
| Agreed revision | Space starts a real jump immediately when grounded. No midair double jump. |
| Starting tuning | A 0.12-second landing buffer and 0.08-second edge grace forgive slightly mistimed input. Holding Space does not repeat jumps. |
| Agreed | Game controls 180-degree rotation. No player rotation button in the finished game. |
| Agreed | Background, platforms, goal and hazards rotate together; both longitudinal ends are lethal. |
| Tested prototype default | Player position rotates with the map, character stays upright, gravity remains screen-down. |
| Tested prototype default | Freeze physics during the 0.6-second turn, clear velocity/input, and refresh contacts before movement resumes. |
| Current deterministic test | Six seconds traversal and two seconds warning, then one turn. Random scheduling comes later. |
| Agreed revision | Camera follows the body at approximately 2x the former overview magnification, remains upright and keeps the player visible during rotation. |
| Implemented loop | Red kills, green wins, death auto-respawns after 0.6 seconds, R fully restarts. Death takes priority over goal in the same physics step. |

Numerical values remain tuning choices. Do not silently restore the obsolete automatic physics hop or indefinite jump queue.

## 3. Environment and setup

- Editor: Unity 6.3 LTS, **6000.3.23f1**. Both teammates should use this exact version.
- Template: **2D Built-In Render Pipeline**. Windows development machine; matching Web support installed.
- Repository: https://github.com/MNSamarth/Hat-Hop.git
- Scripts support Unity Input System when enabled, otherwise legacy input. Inspect Packages and Player settings before changing the backend.
- No third-party runtime package is required for the current mechanics.
- Use Visible Meta Files and Force Text asset serialization; commit Assets metadata, Packages and ProjectSettings.

The user's local project contains Editor-generated settings and scenes. The authoring workspace contains source scaffolding and does not automatically mirror those local files or the latest GitHub commits. Inspect a current checkout before a merge or scene replacement. Do not fabricate project settings or claim Unity testing from static source inspection.

For the current revision, follow MOVEMENT_CAMERA_UPDATE.md: import the patch, open GameplayTest, use Hat Hop > Apply Movement and Camera Update outside Play Mode, and save. Preserve existing .meta files when replacing scripts. Do not recreate the project or regenerate a customized scene.

## 4. Scene architecture

| Object or component | Responsibility |
| --- | --- |
| MapRoot | Fixed pivot with unit scale; parents background, platforms, hazards and exit. |
| Player root outside MapRoot | Dynamic Rigidbody2D, centered symmetric collider, PlayerMotor2D. |
| Player/Visual | SpriteRenderer and cosmetic local movement; no collider or Rigidbody2D. |
| PlayerHopVisual | Applies cosmetic hopping only while grounded and active. |
| Main Camera outside MapRoot | Orthographic camera plus PlayerFollowCamera. |
| PlayerMotor2D | Update input, FixedUpdate velocity, grounded contact checks, one real jump, buffer/grace and reset/suspend hooks. |
| RotationController | Traversal, Warning, Turning and Resuming phases. Optional external lifecycle mode. |
| LevelFlow | Playing, Dead and Won states; death/goal arbitration; respawn and full restart. |
| LevelTrigger2D | Hazard/goal reports restricted to the configured player body. |
| GameplayHUD | Temporary upright controls, countdown, outcomes and restart button. |
| RotationScheduler, planned | Seeded, constrained timing after level design passes deterministic tests. |

MovementTest, RotationTest and GameplayTest are separate scenes. Updating shared scripts affects all scenes using them. Never promise an old scene retains old movement behavior merely because its scene file is unchanged.

Use solid two-sided platforms and ordinary BoxCollider2D geometry initially. Hazard and goal colliders are triggers and must never provide ground support. Explicit layers can be added when needed; the current trigger code identifies the configured Rigidbody2D directly.

## 5. Movement and camera contracts

Read fresh key presses in Update. Apply velocity in FixedUpdate. Ground support requires an upward contact normal and a nonascending body; a wall or ceiling is not ground. A/D has direct horizontal control and releasing it stops horizontal motion in this prototype. Gravity remains physical. Space requires support or the brief edge grace and consumes one request. Clear jump input and support history after launch, reset and rotation suspension.

Preserve scene tuning when renaming serialized fields. The new Jump Speed migrates from Big Hop Speed using FormerlySerializedAs. Starting values are horizontal speed 4.5, jump speed 8, gravity scale 2 and minimum ground-normal Y 0.65. At default gravity magnitude 9.81, theoretical maximum jump rise is approximately 1.63 units. Layout must leave clearance and a margin rather than using this as a guaranteed platform gap.

Cosmetic hopping moves only the Visual child. The user has set the scene hop height to 0.4 world units; keep that tuning (the original script default is 0.12). The period is 0.32 seconds; animation continues at idle when grounded and stops during real jumps, suspension or finished runs. Keep it small enough that collision behavior remains visually understandable. The player's root collision box stays still while idle.

The camera follows the player root, never Visual. Halving orthographic size gives 2x linear magnification at a fixed aspect ratio: GameplayTest's size 10 becomes 5. Preserve the original reference size so repeated setup is idempotent. Start with modest velocity-based look-ahead and smoothing; directly track the player during rotation and snap after reset. Do not zoom out to reveal the whole route automatically. The closer camera does not guarantee the exit is always hidden; placement and layout must support discovery while preserving readable landings.

## 6. Rotation and lifecycle contracts

1. Warning begins only in active traversal. Movement stays enabled throughout the warning; UI reads the controller's authoritative timer.
2. Suspend the motor and simulation for the turn. Animate from stored initial positions: P_new = C + R(theta) * (P - C). Do not accumulate incremental transforms.
3. Keep the character and camera upright. Snap map orientation to exactly 0 or 180 degrees at endpoints.
4. Restore the player pose, clear velocity/input, synchronize transforms and check solid overlap. A meaningful invalid penetration is a logged bug with a safe reset, not permission to teleport through geometry.
5. Refresh physics contacts before resuming the motor. Externally requested resets must also preserve this refresh step.
6. LevelFlow handles reset input in gameplay scenes; standalone RotationController handles it in the rotation test. Never attach two competing reset owners.
7. Resolve pending trigger outcomes before the next rotation update. Death wins over goal in the same physics step. Ignore contacts during frozen turns, and accept valid contacts after resume.
8. Death and win halt simulation and scheduling through the controller's halt method. Do not disable the controller to implement win; its OnDisable performs cleanup.
9. Manual R clears all run state and counters. Auto-respawn preserves death count. No stale delayed respawn or turn may fire after a restart.

## 7. Three-level design plan

The first larger layouts are authored in Assets/HatHop/Editor/LevelData/ThreeLevels.json. ThreeLevelSceneBuilder generates complete Easy, Medium and Hard scenes plus MainMenu through **Hat Hop > Create Menu and Three Levels**. Refer to THREE_LEVELS_AND_MENU.md for integration and tests.

| Level | Room size / route landings | Learning or challenge |
| --- | --- | --- |
| Easy: The Foyer | 16 x 22.52 / 15 | Three sections, broad landings, no interior hazards, 10-second traversal before warning. |
| Medium: False Bottom | 18 x 31.12 / 21 | Five sections, longer sweeps, narrower landings, four edge hazards, 8-second traversal. |
| Hard: The Last Act | 20 x 42.96 / 29 | Seven sections, precision platforms, eight edge hazards, 6-second traversal. |

Warnings stay at two seconds and turns at 0.6 seconds. All layouts use the same motor, 0.4 visual hop height and camera size 5. The level boundary-distance guard scales with the room radius instead of the old fixed 25 units. The generator creates a separate material and sprite and preserves older test scenes.

Side-alcove exits have solid roofs/floors/back walls to block direct vertical wins in both orientations. Descending into an inverted exit requires going around its open side. Static sampled trajectory checks cover adjacent platforms and both exit approaches; they do not establish that live flips or final difficulty are fair. Playtest each layout in Unity and tune from actual results. Keep both orientations traversable and avoid offscreen lethal surprises.

After deterministic layouts pass, schedule single-use progress bands in original map-local coordinates. Track maximum progress, use reproducible seeds, enforce a minimum gap and never overlap warnings/turns. If a band cannot offer a fair preparation route within the warning, relocate or remove it. The initial three/four-flip idea remains tunable per level.

## 8. Menus and level progression plan

The combined source now includes MainMenu with Play, Level Select and Controls, and three mapped level scenes generated together. Play opens Easy; all difficulties are available directly. GameplayHUD shows the level name, Restart/Main Menu and outcome controls. Easy and Medium have Next Level; Hard shows final completion with Retry/Main Menu. Selecting Hard directly does not imply all three were cleared. There is no saved progress or unlock system.

LevelCatalog is the single scene-path mapping source for both menu and Next Level. Its custom Inspector uses SceneAsset pickers. MainMenu is first in the global build list, followed by Easy, Medium and Hard. Profile-specific overrides must include the same enabled scenes. Refresh Menu Build Scenes after mapping changes; reassign moved scene paths.

SceneNavigation uses validated asynchronous Single-mode loads and guards repeated requests. No persistent gameplay objects carry timers/input between levels. Existing LevelFlow owns restart/death/win; menu navigation does not disable rotation as a substitute for win. Future pause behavior must freeze relevant timers and restore time on navigation.

## 9. Git workflow

Work on a feature branch based on the latest tested checkpoint. The user has pushed gameplay and Web settings and reports merging through the GUI. Check current branch and history before further merges; do not assume the authoring checkout has those remote commits.

```sh
git status
git branch --show-current
git fetch origin
```

For a new feature starting from updated main, after the working tree is clean:

```sh
git switch main
git pull --ff-only origin main
git switch -c feature/your-feature
```

Use the existing feature/main-menu branch or create feature/menu-levels from the tested movement checkpoint; do not assume that checkpoint has already merged to main. Save scenes outside Play Mode, then:

```sh
git add Assets/HatHop Docs
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Describe the behavior changed"
git push -u origin HEAD
```

Include package/settings changes explicitly when relevant. Commit Assets and corresponding .meta files, Packages including its lock, ProjectSettings and Docs. Ignore Library, Temp, Obj, Logs, UserSettings, Builds, IDE caches and generated solution files. Never overwrite another working .git directory or force-push shared history. Move assets in Unity to preserve GUIDs. Agree on ownership of shared scenes; use separate scenes or prefabs for concurrent work. Merge tested features into main and use real human Git identities for human commits. Track actual contributions accurately.

## 10. Current milestone order and delivery

1. Revised movement and closer camera: user reports working; visual hop height chosen as 0.4.
2. Combined main menu, all three levels and completion progression: source authored, static layout checks passed, Unity validation pending.
3. Integrate and playtest the complete set; tune jump routes, live flip fairness, warning pacing and presentation.
4. Deploy menu plus all three levels together, then automate builds/deployment from main.

Completed user-reported checkpoints: initial movement, rotation, gameplay loop and a localhost Web build. Public hosting is deferred by user preference. Rebuild after code/scene changes; pushing C# source alone does not update a hosted game. Publish generated build files separately, for example in gh-pages, and configure Pages to that output. Build into ignored Builds/Web. Start with disabled compression or configure decompression fallback if the host cannot provide required headers. Serve via HTTP/HTTPS and verify the actual hosted URL, keyboard focus, full loop and restart. The user wants automatic rebuild/deployment for changes merged into main after the combined release. GitHub Actions Unity build automation is not configured yet.

The design reference records submission on October 2, 2026 at 12:59 PM, via Brightspace and Discord, with GitHub Pages hosting and a video under one minute. Reserve time on October 1 for submission checks. Confirm current course announcements before final delivery. Still required: three researched genre games, actual Week 3 matrix columns, diagram, final controls, repository/build/video links and truthful contribution/AI records. This development guide is not the final graded design document. User reports this year's TA/grader guidance permits complete AI use.

## 11. Acceptance gates

- Revised movement: presses during all cosmetic-hop phases work; held Space does not repeat; no double jump, wall jump or wall sticking; reset clears pending input.
- Visual/camera: body grounded while sprite hops; camera does not bob with sprite; 2x view remains readable; player visible through turn and reset.
- Rotation: exactly one warned half-turn, correct endpoints, no clipping/impulses, reliable full reset.
- Lifecycle: lethal ends, deterministic death priority, clean respawn, win freezes the run, all restart paths work.
- Levels: reachable in both orientations; descent requires deliberate landings; preparation options are visible and reachable.
- Menus: each level loads and returns correctly; final level completes; no timer/input leakage across scenes.
- Delivery: fresh clone opens with matching Editor; hosted browser handles controls, death, win and restart; all submission links work.

Record what was actually tested, where and by whom. Source authored is not source compiled, and a compile is not a playtest. Earlier user tests do not establish that the current revision passes.

## 12. AI collaboration rules and references

Read guide and progress, inspect existing code and settings, and implement one milestone at a time. Preserve latest agreed behavior; label proposed changes. Provide exact menu/component setup and focused Play Mode checks. Keep components small and references explicit. Record bugs and decisions after tests; never invent contributions or fill checklists from assumptions. Keep private discussion out of shared files.

Official references matching the target Editor:
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Camera-orthographicSize.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Serialization.FormerlySerializedAsAttribute.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/2d-physics/physics-material-2d-reference.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnTriggerStay2D.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-deploying.html

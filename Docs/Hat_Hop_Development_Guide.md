# Hat Hop Unity Development Guide

Working reference for USC CSCI 526. Updated September 24, 2026.

Read this guide and PROGRESS.md before changing the project. The latest explicit user decisions take priority over the supplied design reference. In particular, grounded movement with visual hopping now replaces automatic physical hopping. Earlier milestone documents are historical records.

## 1. Goal and scope

Build a 2D vertical platformer where warned, game-controlled 180-degree map rotations turn climbs into controlled descents. The rabbit escaping a magician's hat is the theme; movement and level geometry must communicate the game without story, elaborate art or cutscenes.

First validate the revised movement and closer camera in the existing GameplayTest. Then create larger Easy, Medium and Hard levels and simple menus. Browser Web builds on GitHub Pages remain the delivery target. Public deployment is deferred until the movement and game are ready; the initial local browser build has passed according to the user.

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

Cosmetic hopping moves only the Visual child. Default height is 0.12 world units and period 0.32 seconds; animation continues at idle when grounded and stops during real jumps, suspension or finished runs. Keep it small enough that collision behavior remains visually understandable. The player's root collision box stays still while idle.

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

All levels should be larger than the original compact test room. Build Easy first; do not freeze all three layouts before validating the revised jump and viewport.

| Level | Starting scope | Learning or challenge |
| --- | --- | --- |
| Easy | Three traversal sections, broad landings, generous preparation spots, simple side-alcove exit. | Understand movement, warning and ascent/descent transitions. |
| Medium | Five sections, alternating routes, narrower landings, visible interior hazards. | Choose routes that remain useful after the next rotation. |
| Hard | Seven sections, offset platforms, fewer safe preparation choices, more demanding exit approach. | Combine precise movement, controlled descent and preparation. |

Section counts are planning targets, not committed geometry or guaranteed duration. Tune section dimensions from measured jump reach and the new camera view. Keep platforms reachable with margin, break straight falls with staggered surfaces and make the same exit accessible in either orientation. Test warnings during jumps, edge departures and descent. Never use offscreen lethal surprises as a substitute for difficulty.

After deterministic layouts pass, schedule single-use progress bands in original map-local coordinates. Track maximum progress, use reproducible seeds, enforce a minimum gap and never overlap warnings/turns. If a band cannot offer a fair preparation route within the warning, relocate or remove it. The initial three/four-flip idea remains tunable per level.

## 8. Menus and level progression plan

Still to implement: a main menu with Play, Level Select and Controls; in-game Restart and Main Menu; completion options Next Level, Retry and Main Menu. Hard should show completion instead of a nonexistent next level. Define scene names and build-list order explicitly. Any pause flow must freeze gameplay timers and restore time on restart or scene change. Keep implementation details out of the player's UI.

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

For the current movement revision, stay on the existing feature/movement-camera branch. Save scenes outside Play Mode, then:

```sh
git add Assets/HatHop Docs
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Describe the behavior changed"
git push -u origin HEAD
```

Include package/settings changes explicitly when relevant. Commit Assets and corresponding .meta files, Packages including its lock, ProjectSettings and Docs. Ignore Library, Temp, Obj, Logs, UserSettings, Builds, IDE caches and generated solution files. Never overwrite another working .git directory or force-push shared history. Move assets in Unity to preserve GUIDs. Agree on ownership of shared scenes; use separate scenes or prefabs for concurrent work. Merge tested features into main and use real human Git identities for human commits. Track actual contributions accurately.

## 10. Current milestone order and delivery

1. Revised grounded movement, visual hopping and closer camera.
2. Build and playtest Easy with the new movement and visibility.
3. Build Medium and Hard from the tested movement ranges.
4. Add menu and level progression.
5. Tune timing, difficulty and presentation, then publish.

Completed user-reported checkpoints: initial movement, rotation, gameplay loop and a localhost Web build. Public hosting is deferred by user preference. Rebuild after code/scene changes; pushing C# source alone does not update a hosted game. Publish generated build files separately, for example in gh-pages, and configure Pages to that output. Build into ignored Builds/Web. Start with disabled compression or configure decompression fallback if the host cannot provide required headers. Serve via HTTP/HTTPS and verify the actual hosted URL, keyboard focus, full loop and restart. GitHub Actions Unity build automation is optional later work, not configured.

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

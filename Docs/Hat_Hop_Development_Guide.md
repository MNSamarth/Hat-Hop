# Hat Hop Unity Development Guide

Working reference for CSCI 526, USC. Updated September 24, 2026.
Read this file before changing gameplay. The supplied Hat Hop Design Reference and explicit user decisions establish the design; this guide turns them into implementation steps. This is a project reference, not an installed AI skill.

## 1. Goal and scope

Build a small 2D vertical platformer in Unity: steer a continuously hopping rabbit toward an exit while warned, game-controlled 180-degree map flips turn climbs into dangerous descents. The greybox must communicate this through movement, geometry and feedback without story or art.

First prove one room. Then expand to one short level with several traversal sections. Browser WebGL on GitHub Pages is the delivery target. No enemies, inventory, collectibles, combat, fall damage or special platform types in the initial scope. Use self-created geometry; external assets are outside the assignment rules recorded in the reference.

## 2. Decisions and provisional rules

| Status | Rule |
| --- | --- |
| Agreed | A/D steers; there is no ground walking. |
| Agreed | Landing automatically launches a small hop. |
| Agreed | Space enables a larger jump. |
| Agreed | Game controls rotation; the player cannot trigger it in the final game. |
| Agreed | Background, tiles, hazards and goal rotate together by 180 degrees. |
| Agreed | Initial traversal is upward; flipping creates downward traversal. |
| Agreed | Both longitudinal ends have lethal boundaries. |
| Initial tuning | Roughly two seconds of warning; around three or four flip opportunities in a level. |
| Prototype default | A fresh Space press within 0.2 seconds before takeoff replaces one small hop with one big jump. Holding Space does not repeat it. |
| Prototype default | Player position rotates around the map center with the map; player remains upright. Gravity remains screen-down. |
| Prototype default | Freeze gameplay physics during the turn, animate for 0.6 seconds, clear velocity and buffered input, then resume. |
| Prototype default | R restarts; fixed overview camera; solid two-sided platforms. |

Do not present provisional rules as user-confirmed decisions. Change them after an observed playtest problem and record the reason.

## 3. Environment and dependencies

The user's installed Editor version has not been supplied. The starter targets Unity 6 APIs and includes a velocity compatibility branch for older Editors; compatibility still requires an actual import test. Do not upgrade an existing project silently.

1. Use the same exact Editor version on both teammates' machines; record it below after creating the project.
2. Create a 2D Built-In Render Pipeline project for this initial starter. If using Universal 2D, first verify that the generated sprite is visible with that pipeline.
3. Install the Web Build Support module for that Editor through Unity Hub.
4. Use C# in an IDE with Unity integration. No third-party runtime packages are needed for this milestone.
5. Starter input supports the Input System when enabled and legacy input otherwise. When using Input System, install it through Package Manager. When using legacy input, enable Input Manager (Old) or Both in Player settings. Do not switch systems mid-milestone.
6. Editor settings: Visible Meta Files and Force Text asset serialization. Commit the generated ProjectSettings and Packages, including the package lock.

Environment record: Editor **pending**; template **proposed 2D Built-In**; input backend **pending**; OS **pending**; remote URL **pending**.

## 4. Start the supplied movement milestone

This repository is an importable starter, not a complete Editor-generated Unity project. It intentionally does not fabricate ProjectSettings or a package manifest for an unknown Editor.

1. Clone or extract the starter to a working folder. Create a new Unity project in a separate temporary folder using your chosen Editor.
2. Close Unity. Copy the generated Assets, Packages and ProjectSettings into this repository root, merging Assets and preserving Assets/HatHop. Do not copy Library or the temporary project's Git metadata.
3. Open the repository root through Unity Hub. Let Unity generate/import metadata. Resolve any Console errors before continuing.
4. Choose **Hat Hop > Create Movement Test Scene**. The tool prompts before discarding unsaved scene edits and before replacing its test scene.
5. It creates a camera, MapRoot with ground and three platforms, a blue player, and a movement-test component. The player has Rigidbody2D, BoxCollider2D and PlayerMotor2D. The tool saves Assets/HatHop/Scenes/MovementTest.unity.
6. Enter Play Mode: A/D steers, Space just before landing makes one bigger jump, R restores the starting position. Falling below the test area also resets. This fallback reset is a test helper, not the final death system.
7. Run the checks in section 11, record results in Docs/PROGRESS.md, then commit the scene, sprite and all generated .meta files.

## 5. Scene and component architecture

| Object or script | Responsibility |
| --- | --- |
| MapRoot at (0,0,0), unit scale | Pivot for the complete room, including all traversable geometry. |
| MapRoot/Background | Self-created visual backdrop; no gameplay collision. |
| MapRoot/Platforms | Solid BoxCollider2D surfaces, initially without individual rigidbodies. |
| MapRoot/Hazards | Visible lethal triggers at both ends and only where descent needs them. |
| MapRoot/Exit | Trigger in a side alcove; rotates with the map. |
| Player outside MapRoot | Dynamic Rigidbody2D, frozen Z rotation, collider, motor. |
| Main Camera outside MapRoot | Orthographic, fixed overview for the first room. |
| Canvas and EventSystem outside MapRoot | Controls, warning countdown, win/restart feedback; stay upright. |
| PlayerMotor2D | Input capture, air steering, contact-based launch, jump buffering, suspension/reset hooks. Implemented in starter. |
| MovementTestSession | Temporary R/fall reset for the movement room. Implemented in starter. |
| RotationController | Traversal → Warning → Turning → Traversal state machine. Planned. |
| RotationScheduler | Requests a turn only when eligible; constrained randomness after deterministic testing. Planned. |
| LevelFlow | Death, full reset, win and state priority. Planned. |
| WarningUI | Presents authoritative controller countdown; does not maintain its own timer. Planned. |

Use layers Player, Solid, Hazard and Goal when implementing the full room. Hazards and Goal are triggers; they must never count as grounded surfaces. Keep collision rules explicit. Start with ordinary boxes; a tilemap and Cinemachine are unnecessary dependencies for this scope.

## 6. Movement implementation contract

Read key events in Update; apply Rigidbody2D velocity in FixedUpdate. Ground support must have an upward contact normal, not just any collision or a ray touching a wall. Reject launch while ascending to prevent stale landing contacts from double-launching. Use collision detection and interpolation on the player. Never drive active Rigidbody2D movement by editing its transform every frame.

Starting tuning: horizontal speed 4.5 units/s, small takeoff speed 4, big takeoff speed 8, gravity scale 2, buffer 0.2 seconds, minimum ground-normal Y 0.65. With default gravity magnitude 9.81, approximate hop heights are 0.41 and 1.63 units. These are tuning estimates, not verified reachability guarantees; account for collider dimensions, fixed timestep and clearance. The generated room uses approximately one-unit rises.

Small hops launch without input. Space is consumed once on takeoff or expires; it does not apply midair thrust. A/D gives immediate air control; releasing stops horizontal velocity in this first version. Simultaneous A and D cancel. Reassess momentum only after testing this baseline.

## 7. Rotation implementation contract

Build rotation only after movement passes. Start with a predictable interval and an Editor-only debug trigger; defer randomness.

1. Warning begins only during active traversal. Display the same countdown used by the controller; leave movement enabled for preparation.
2. At countdown completion, suspend the motor and store the initial player position and map orientation. Disable player physics simulation during the animation. Block death/win trigger processing while turning.
3. Animate map angle and player position from the stored initial values, never by repeated incremental rotation. For pivot C, player P and angle theta, use P_new = C + R(theta) * (P - C). Keep player art upright.
4. Snap to the exact final orientation (0 or 180 degrees). Synchronize transforms before collision queries. Restore Rigidbody2D position explicitly, clear velocity and buffered input, then resume simulation and input at a physics boundary.
5. Validate overlap before resuming. An upright rectangular collider does not preserve its swept shape at intermediate angles; collisions are intentionally disabled during the turn. At a 180-degree endpoint a centered symmetric box should preserve clearance. If invalid overlap remains, report it as a level/rotation bug and reset safely; do not silently move the player through geometry.
6. Reset must cancel an active turn, warning and schedule, restore the original map pose, respawn the player, reset progress and UI, and clear velocity. Never let a stale coroutine finish after reset.

Use an explicit state or session-generation token to prevent stale events. Death takes priority over exit if both occur in the same simulation step. Winning stops scheduling and motion. R works during warning, turning, death and win. Pause countdown on application focus loss if playtesting shows browser focus causes unfair deaths.

## 8. Timing and level design

Design ascent and descent together. Use staggered solid platforms and alternating openings to interrupt a straight fall; put the exit away from a direct drop path. Avoid one-way platforms until their flipped behavior is deliberately designed.

Measure progress in the original map's local coordinates, using MapRoot.InverseTransformPoint(player.position). Track the maximum achieved progress so repeated crossings do not retrigger bands. Initially use fixed test sequences. Later use single-use bands, seeded delays and a minimum gap, and cancel pending requests on reset/win. Store the seed in debug logs. Random timing must never override the warning or create overlapping turns.

A two-second warning needs a reachable preparation option with the current hop timing. Test warnings during small hops, big jumps, edge departures and descent. Do not assume randomness is fair because there is a countdown. If fair options cannot be designed for a band, move or remove that band.

## 9. Git workflow

The starter has a local Git history on main. No remote has been created or pushed. Use the user's real Git identity for future commits. Initial automated commits use a neutral repository-local identity only when none is configured; replace it before human work.

Commit Assets and every corresponding .meta file, Packages, ProjectSettings, Docs, .gitignore and .gitattributes. Never commit Library, Temp, Obj, Logs, UserSettings, generated builds, IDE caches, credentials or Unity license files. Move assets inside Unity so GUIDs survive.

Before working:

```sh
git status
git switch main
git pull --ff-only
git switch -c feature/rotation
```

The pull command requires a configured remote/upstream. Save Unity assets before inspecting and committing:

```sh
git diff --check
git status --short
git add Assets Packages ProjectSettings Docs
git diff --cached --stat
git commit -m "Add warned deterministic room rotation"
git push -u origin feature/rotation
```

Use small commits with a purpose and validation note. Agree on scene ownership before simultaneous edits; use separate test scenes/prefabs for parallel features. Do not hand-resolve conflicting GUIDs casually. Keep main importable and playable once the first Unity gate passes. Merge reviewed branches, then delete completed branches. Do not force-push shared history.

To connect an empty remote after creating it under the correct account:

```sh
git remote add origin YOUR_REPOSITORY_URL
git push -u origin main
```

Do not run this against a nonempty repository without first fetching and reconciling its history. No public/private visibility choice has been made. GitHub Pages availability depends on the account/repository configuration; verify access before submission.

## 10. Milestones and delivery

| Target | Milestone | Exit condition |
| --- | --- | --- |
| Sep 25 | Movement | Stable small hops, steering and one buffered big jump. |
| Sep 26 | Deterministic rotation | Warning, 180-degree map/player transition and clean reset. |
| Sep 28 | Complete room and first Web build | Hazards, exit and both traversal directions work; hosted build loads. |
| Sep 30 | Fairness and constrained random timing | Reproducible seeds, playtest revisions and accurate contribution log. |
| Oct 1 | Submission package | Working links, final descriptive document and video under one minute. |

The supplied reference records an October 2, 2026, 12:59 PM deadline, Brightspace and Discord submission, and GitHub Pages hosting rather than Unity Play. Confirm against the course's current announcement before final delivery.

For Web delivery, add the playable scene to the build scene list and install matching Web Build Support. Build into an ignored Builds/Web folder. Start with compression disabled to simplify the initial hosting check; later use compression with decompression fallback if the host cannot provide required encoding headers. Test via HTTP/HTTPS, not by opening index.html as a local file. Publish build output separately from source with GitHub Pages; do not upload Unity caches. Add a .nojekyll file to the published output when using branch-based Pages. Verify the actual hosted URL, browser keyboard focus, restart and complete win loop. Record Editor version, commit and build date. Do not claim a successful deployment until the hosted build has been played.

## 11. Acceptance and regression checklist

### Movement gate

- [ ] Import and compile with no Console errors in the chosen Editor.
- [ ] On flat ground, 30 seconds of continuous small hops without input.
- [ ] A/D steers during hops; no separate ground-walking phase.
- [ ] One Space press just before landing causes one larger takeoff.
- [ ] Held Space never repeats big jumps; an early expired press does not trigger later.
- [ ] Walls and platform undersides do not launch a hop.
- [ ] Each test platform is reachable; edge landings do not produce repeated impulses.
- [ ] R/fall reset clears velocity and queued jump.

### Rotation and room gate

- [ ] Exactly one warning and turn per scheduled opportunity.
- [ ] Player, map, goal and hazards reach expected endpoints; UI stays upright.
- [ ] No launch spikes or clipping on physics resume.
- [ ] Restart halfway through warning/turn fully restores the initial state.
- [ ] Big jumps during warnings retain a fair survival option.
- [ ] Exit reachable in each orientation; an uncontrolled straight fall cannot win.
- [ ] Death/exit priority is deterministic; win stops further turns.
- [ ] Same random seed reproduces a reported schedule.

### Delivery gate

- [ ] Fresh clone imports using the recorded Editor; metadata and package lock are committed.
- [ ] Actual hosted browser build accepts input and completes/restarts.
- [ ] No editor-only debug controls or Console errors in release.
- [ ] Final document includes three researched games, actual course matrix columns, diagram, controls, repository/build/video links and actual contributions.
- [ ] AI assistance recorded. User reports current TA/grader guidance permits complete AI use.

## 12. Collaboration and AI working rules

Read this guide and PROGRESS.md before coding. Inspect existing scripts and scene setup before adding replacements. Preserve agreed mechanics and label proposed changes. Use the recorded Editor's APIs; consult official Unity docs when uncertain. Implement one milestone at a time and explain each script's attachment point and Inspector settings. Favor short components with explicit references over frameworks or global managers.

After each feature, state exactly what changed, what was actually tested, remaining risks and the next action. Code written is not code compiled; a compile is not a playtest. Update progress and decisions after tests. Never invent teammate contributions or mark an unchecked acceptance item complete. Keep personal discussion out of shared project files. Both teammates should own meaningful code and design tasks; the ownership split in the design reference remains a proposal.

## 13. Technical references

- Unity Rigidbody2D contacts: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D.GetContacts.html
- Unity Web deployment and compression: https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-deploying.html

Consult documentation matching the pinned Editor. These references support implementation; they are not the three-game research required for the assignment.

# Milestone 3 Rotation Test

Target Editor: Unity 6000.3.23f1, 2D Built-In Render Pipeline.
Status: source prepared; Unity compilation and Play Mode validation pending.

## Starting point

The user has imported the movement starter and pushed project foundation to MNSamarth/Hat-Hop. The user accepts movement as functionally working and wants finer jump tuning deferred until gameplay testing. Space currently queues one big jump until landing. This update preserves that motor and the existing MovementTest scene.

## Install and run

1. Save Unity work and stop Play Mode. In the project terminal, run `git status`. Commit any unfinished movement changes before changing branches.
2. Run `git switch -c feature/world-rotation` from the current branch containing the working movement revision. Do not switch to an older main branch first.
3. Close Unity. Extract Hat_Hop_Rotation_Milestone.zip and copy its Assets and Docs folders into the project root, merging folders. This patch contains three new scripts, their metadata, and this note. It does not contain a .git folder, existing scenes, ProjectSettings, packages or a replacement motor.
4. Reopen Unity and allow compilation. If any Console error appears, report its complete text before continuing.
5. Choose Hat Hop > Create Rotation Test Scene. If prompted, save the currently open scene. The tool saves Assets/HatHop/Scenes/RotationTest.unity and creates its own RotationSquare sprite. It asks before replacing an existing RotationTest scene.
6. Enter Play Mode and click the Game view. Use a landscape Game view, preferably 16:9. A/D steers, Space queues a big jump and R resets the entire test room.

## What should happen

Six seconds of traversal precede a two-second warning. Movement stays active during warning. Then MapRoot and the player's position rotate 180 degrees counterclockwise over roughly 0.6 seconds. The player stays upright; gravity resumes screen-down. The next traversal period starts only after physics resumes. Timing advances on fixed steps, so durations are quantized by the physics timestep.

A gold background marker starts near the upper-left corner and moves to the lower-right corner after one flip. The UI and camera stay upright. The room has solid safety floors and walls so failures in this milestone can be separated from hazard deaths. These are test fixtures, not final lethal boundaries. There is no exit or win logic yet.

## Architecture

- RotationSceneBuilder: creates only the separate test scene and its sprite.
- RotationController: owns the deterministic Traversal, Warning, Turning and Resuming states, input for full reset and endpoint checks. Runs before PlayerMotor2D.
- RotationTestHUD: temporary screen-space countdown and completed-turn count using Unity's built-in GUI. Replace with final UI later.
- PlayerMotor2D: existing movement component, unchanged by this patch.

No MovementTestSession is attached in this scene: otherwise two components would compete to reset the player.

The controller disables player simulation and interpolation during animation, transforms the player from its starting position around MapRoot's fixed pivot, and snaps to exact 0/180-degree endpoints. It restores simulation with the motor suspended for one physics step so collision contacts can refresh. The motor then resumes with a cleared jump queue and no inherited launch velocity. Small gravity velocity from that refresh step is expected.

Player endpoint clearance is checked against enabled solid map colliders. Penetration exceeding 0.03 units, or an invalid distance result, triggers a warning and full reset. Small solver contact penetration is tolerated. This does not prove collision safety in Unity; test the scenarios below. The design assumes a centered, symmetric player box and a unit-scale MapRoot; do not change those while validating rotation.

R requests reset at the next physics step. Reset restores original map pose and player position, clears turn count, warning time and queued input, then refreshes contacts. There are no asynchronous turn coroutines left running after reset. Traveling farther than 20 units from the map center also resets as a test fallback.

## Acceptance checks

- [ ] Scripts compile without red Console errors.
- [ ] Warning lasts approximately two seconds and permits steering/jumping.
- [ ] Exactly one 180-degree turn follows each warning.
- [ ] Background marker, all platforms and player position move with the map.
- [ ] Player remains upright, camera/UI do not rotate, gravity remains screen-down.
- [ ] Wait through at least four turns, including small hops and big jumps: no clipping, huge impulses or frozen player.
- [ ] Press R during traversal, warning and halfway through a turn. Each restores the same starting layout and a fresh six-second traversal timer.
- [ ] A queued big jump is cleared by reset and rotation.
- [ ] Existing MovementTest scene still opens and its controls work.

Do not mark the milestone complete based on source review alone. The full level's climb/descent fairness and hazard/exit logic belong to later milestones.

## Commit after the first successful import and test

Save the generated scene outside Play Mode, then run:

```sh
git add Assets/HatHop Docs/MILESTONE_3_ROTATION.md
git --no-pager diff --cached --stat
git commit -m "Add warned world rotation and reset test scene"
git push -u origin HEAD
```

Include the generated RotationTest scene, RotationSquare image and their metadata. Record test results and any errors here before committing. Movement tuning remains on the backlog.

## Verification and references

Authoring checks: whitespace and patch metadata checks; analytical half-turn geometry check for the test room. No Unity Editor or C# compiler is available in the authoring environment, so no compilation, Play Mode, frame-rate or browser claims are made.

Official API references consulted:
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ColliderDistance2D.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody2D.html

AI assistance: initial rotation controller, test HUD, scene generator and instructions prepared with Codex. User testing and later revisions should be recorded separately.

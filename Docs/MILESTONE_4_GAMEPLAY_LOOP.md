# Milestone 4 Gameplay Loop

Unity 6000.3.23f1, 2D Built-In Render Pipeline. Source prepared; import, compilation, Play Mode and browser checks pending.

## Current progress

The user reports Milestone 3 rotation, movement resume and reset working correctly. Movement comfort tuning remains deferred to gameplay playtests. This patch adds a separate GameplayTest scene generator. It preserves MovementTest and RotationTest scenes and the existing queued-jump motor.

## Install

1. Save the current scene and exit Play Mode. Confirm `git status` is clean or commit unfinished rotation work.
2. On the branch containing the working rotation milestone, run `git switch -c feature/gameplay-loop`. Do not switch to an older main branch first.
3. Close Unity. Extract Hat_Hop_Gameplay_Milestone.zip, then merge its Assets and Docs folders into the project root.
4. Allow replacement of **RotationController.cs only**. Keep its existing .meta file. All other files in the patch are new. There are no .git, package, settings, or scene files in the archive.
5. Reopen Unity. After compilation, select **Hat Hop > Create Gameplay Test Scene**. It generates and saves Assets/HatHop/Scenes/GameplayTest.unity with its own GameplaySquare sprite. Save the currently open scene if prompted.
6. Press Play and focus the Game view. Prefer a 16:9 or other landscape Game view.

## Controls and rules

- A/D steers; automatic small hops and the queued Space big jump are unchanged.
- Reach the green rectangle in the upper-right alcove to win. It rotates with the map.
- The red strips at the top and bottom are lethal. Grey side walls are solid.
- Falling into a red strip shows death feedback, freezes the player and rotation, and restarts the entire room after 0.6 seconds. Automatic respawn preserves the death counter.
- Winning freezes the player and all future rotation. Press R or click Restart to begin again.
- R works during traversal, warning, turning, death and win. Manual restart resets the map, player, rotation timer, queued jump, run timer, flip count and death count.
- Six seconds of traversal, two seconds warning and a 0.6-second turn remain deterministic.
- Hazard and exit contacts during the turn are ignored. Contacts after the turn resumes count, including contacts found during the physics-refresh step.
- If hazard and goal are reported in the same physics step, death wins. Only the configured player's Rigidbody2D can activate these triggers.
- Leaving the room by more than 25 units from its center counts as a death rather than leaving the game stuck.

This is a functional loop fixture. Its platform spacing, exit approach and warning fairness still need playtesting in both orientations; they are not approved final level design. Do not add difficulty to compensate for awkward controls.

## Components and lifecycle

| Component | Responsibility |
| --- | --- |
| GameplaySceneBuilder | New scene, primitives, hazards, exit, player and wired references. |
| LevelFlow | Playing/Dead/Won states, contact arbitration, full restart and auto-respawn. |
| LevelTrigger2D | Enter/Stay reports for hazards or goal, restricted to the configured player. |
| GameplayHUD | Instructions, countdown, death/win response, counters and restart button. |
| RotationController | Existing rotation behavior plus external lifecycle mode, halt and restart methods. |

RotationController's Managed By Level defaults to false, preserving its standalone rotation-test behavior. The gameplay builder enables it, so LevelFlow alone handles R and out-of-bounds deaths in this scene. Do not attach MovementTestSession or RotationTestHUD to this gameplay scene.

Order is LevelFlow (-200), RotationController (-100), PlayerMotor2D (default). Physics contacts are collected as flags and resolved at the next FixedUpdate before any new rotation or motor impulse. Contact callback order does not determine death versus win. No coroutine controls death or respawn, so R cannot leave a delayed stale respawn running.

The controller stays enabled when a run ends; it is halted through HaltForOutcome, because disabling the controller invokes its standalone cleanup. Externally requested reset skips the controller's next update once, refreshing physics contacts before allowing the motor to resume. Reset clears both outcome flags, preventing stale goal contacts from winning the restarted room.

## Play Mode checks

- [ ] Import compiles without errors; GameplayTest opens with red boundaries and green exit.
- [ ] Move off the start platform into the bottom red strip: one death, brief message, auto-respawn on the original platform and a fresh warning timer.
- [ ] Repeat several deaths: no extra spawns, no duplicate death counts, consistent original orientation.
- [ ] After a flip, contact the boundary that was originally at the top: it is still lethal.
- [ ] Reach green: win appears; player and timer freeze. Wait longer than a rotation interval to confirm no new flip.
- [ ] R and the Restart button both leave the win state and restore a playable run.
- [ ] R during warning, mid-turn and death leaves no delayed turn/respawn afterward.
- [ ] A queued big jump before reset is cleared.
- [ ] RotationTest still performs its original warning/flip/reset sequence after importing the changed controller.
- [ ] Hazard/goal priority: in a temporary duplicate scene, overlap a hazard with the exit, move the player there outside Play Mode and run. Death must win. Do not save that layout over GameplayTest.

If reaching the exit prevents testing the win response, temporarily increase **Traversal Seconds** on Gameplay Systems > Rotation Controller to 30 before Play Mode. This isolates the climb/win test. Restore 6 afterward and test with rotation. A successful slow-timer test does not prove the normal schedule is fair.

## Git after the checks

Save the scene outside Play Mode and record the observed results here:

```sh
git add Assets/HatHop Docs/MILESTONE_4_GAMEPLAY_LOOP.md
git --no-pager diff --cached --stat
git commit -m "Add hazards, respawn, exit and win gameplay loop"
git push -u origin HEAD
```

Include generated GameplayTest, GameplaySquare and their .meta files. No automatic merge or remote push was performed by the authoring environment.

## Early browser checkpoint after Play Mode passes

The plan calls for the first browser build after this milestone, before expanding the level. Use the installed matching Web support module. In File > Build Profiles, select a Web profile and put GameplayTest in its scene list. For the first release build, use disabled compression to avoid requiring compressed-file response headers on the initial host. Build output belongs in the ignored Builds/Web directory. Serve through HTTP/HTTPS rather than opening index.html directly. GitHub Pages is the course delivery target, but deployment and the actual hosted keyboard/death/win/restart checks remain pending. Record the source commit used for the build.

References consulted:
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/MonoBehaviour.OnTriggerStay2D.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-building.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/webgl-deploying.html

## Verification and contribution record

Source review covers state ordering, death priority, clearing pending outcomes, standalone controller compatibility, and the physics refresh after external reset. Static file/metadata and fixture geometry checks were run. No Unity Editor or C# compiler is available in the authoring environment, so this is not a claimed compilation or gameplay pass.

AI assistance: LevelFlow, trigger and HUD scripts, controller integration, scene generator and this guide prepared with Codex. Add actual human test results and contribution details as work occurs.

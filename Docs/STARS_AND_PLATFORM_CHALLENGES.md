# Stars and platform challenges

September 25, 2026. Combined source update for Unity 6000.3.23f1, 2D Built-In. This ZIP contains the main menu, three-level generator and this revision together. It builds on the working grounded movement/camera project; do not apply earlier menu/level ZIPs afterward.

## What changed

- Exactly five physical stars in each level, parented to the rotating map. Two are on the route, two are inside deep flip-dependent pockets, and the fifth rewards the final approach (Easy/Medium) or a raised-tip landing (Hard).
- Clear with any number of stars. The result screen displays five star icons and the collected count. Best completed score is shown in Level Select and the HUD.
- Collection is per attempt. Death/respawn, R and Retry restore all five stars. Best results change only on a successful clear and never decrease. Returning to the menu does not bank an unfinished attempt.
- A floating right-side EXIT arrow points toward the goal's actual position, including during rotations. It hides when the exit is within the central visible viewport or the run has ended.
- Two marked red undersides in Medium and three in Hard. Touching the red face is lethal in either orientation, including jumping into it. After a flip it becomes a lethal landing face. The tested static descent routes bypass these faces using the next safe landing and, where needed, bonus shelves.
- Two weight-operated seesaws in Hard. Stand at one end for roughly half a second, wait for the other tip to rise, cross and jump from that tip. A short latch keeps it raised while crossing. The next ledge is 2.0 world units above the neutral beam, beyond the normal jump rise. The upright jump requires the raised tip; a flipped descent can offer a different route, consistent with the world-flip mechanic.

Existing A/D movement, jump speed, coyote/buffer settings, 0.4 visual hop height and camera size 5 are retained. Seesaw weight/contact uses the physical body, not the bobbing visual.

## Why the pocket stars require a flip

Each golden pocket is 1.4 units wide and 3.3 units deep (previously 2.0 by 3.6), closed at the top and sides, with its mouth at the bottom in the original orientation. The star sits near the closed cap. Its pickup zone is higher than the player's maximum reach from the mouth, even allowing the full collision-body height. There is no orientation-lock flag: the geometry is what prevents collection before a flip.

Once inverted, the mouth faces upward and the player can descend into the pocket. The pocket may hold the player until the next scheduled flip. These are optional star detours; the timer remains visible. After flipping back, steer toward the center of the map on the way out, using the colored recovery shelf and return step below the mouth. Missing a star does not permanently remove it; later flips provide another chance, subject to surviving and navigating back.

## Install and generate

1. Stop Play Mode and save your scenes. Commit the current tested checkpoint before regenerating layouts. Use a feature branch such as `feature/stars-platforms` from that checkpoint.
2. Extract **Hat_Hop_Compact_Pockets_Faster_Flips.zip**. Merge **Assets**, **Docs**, **Tools** and **README.md** into the project root, replacing the included files. This includes the earlier menu/level source, so use only this ZIP. It does not replace .git, Packages or ProjectSettings directly.
3. Let Unity finish compiling. If red errors appear, copy the first full error before running setup.
4. Select **Hat Hop > Create Stars and Platform Challenge Levels**. The older Create Menu and Three Levels command is an alias for the same updated generator. Accept the replacement dialog only after saving the previous checkpoint: it regenerates MainMenu, Easy, Medium and Hard, resets their layout edits, refreshes their catalog mapping and generated icons. Older test scenes are preserved, though shared lifecycle/HUD scripts are updated.
5. Check the active Build Profile includes enabled MainMenu first, followed by Easy, Medium and Hard. A profile with Override Global Scene List needs its own matching entries. Press Play from MainMenu.

The new components and Inspector references are added automatically. Do not manually attach StarCollectible, LevelStars, ExitIndicator or SeesawPlatform, and do not rerun the movement setup on these generated scenes.

## Level contents

| Level | Flip-only stars | Red undersides | Seesaws | Traversal / warning / turn |
| --- | --- | --- | --- | --- |
| Easy | Two pockets | None | None | 5 s / 1 s / 0.3 s |
| Medium | Two pockets | Landings 03 and 15 | None | 4 s / 1 s / 0.3 s |
| Hard | Two pockets | Landings 03, 15 and 27 | Landings 08 and 20 | 3 s / 1 s / 0.3 s |

Traversal, warning and turn animation durations are all half the previous values. From the start of traversal, a flip starts after 6 / 5 / 4 seconds in Easy / Medium / Hard. A full cycle also includes the 0.3-second turn and a physics contact-refresh step. The shorter warning still permits movement. Old test scenes retain their existing timing.

Hard is now 44.34 units high because its two elevated transitions were raised. Easy and Medium remain 22.52 and 31.12 units high. Each level still has five stars total. Some older red edge blocks were removed around pocket approaches so they do not obstruct the new detours. See Three_Levels_Overview.png for the updated design diagram, not a Unity screenshot.

## Focused acceptance checks

### Stars and scoring

- Collect one star: it disappears and the counter increases exactly once, including if you remain in its trigger.
- Collect stars and die: the new attempt starts at 0/5 with all five visible.
- Press R or Retry: same reset; no partially collected state survives.
- Touch a star and the exit in one physics step: the star counts in the win result. Touch a hazard in that same step: death takes precedence and no best score is saved.
- Clear with 0, some, then more stars: display matches the attempt; best score only rises.
- Return to Level Select and relaunch: best completed score remains. A lower-score clear does not overwrite a higher one.
- Repeat in a fresh localhost Web build. Scores are local to this installation/browser origin, not an account or cloud save. Browser storage clearing or a different host may result in separate/reset progress.

### Exit arrow

- Begin far from the exit: the right-hand arrow is visible and points correctly.
- Trigger several flips: it follows the transformed exit instead of continuing to point upward.
- Approach the green exit: the arrow hides as the exit becomes visible; it reappears when offscreen.
- Die, retry, win and return to menu: no stale indicator from a previous scene.

### Pocket routes and red faces

- In the original orientation, try to reach each deep pocket star from below; normal jumping must not reach it.
- After a flip, enter from above and collect it. After the next flip, exit toward the center and use the recovery ledges to rejoin.
- Check the new 1-second warning and 0.3-second turn feel readable, particularly when preparing a Hard seesaw jump.
- Verify both smaller pocket detours remain doable during actual scheduled flips, not just when the world is stationary.
- Stand on a marked platform's safe top; then test its red underside and the flipped red top. Only the marked face and existing red hazards are lethal.
- Before flipping near a red platform, identify a reachable safe landing. Report any unavoidable death sequence for route/timing adjustment.

### Hard seesaws (highest priority physics test)

- On Landing 08, stay near an end until it tilts. Use the raised opposite tip to jump to Landing 09. Repeat Landing 20 to 21.
- The neutral platform must not allow the same upright jump. The maximum tilt should allow it without changing jump speed.
- Crossing should not immediately reverse the tilt. Holding at the opposite edge long enough may eventually tilt it the other way.
- Jumping from the settled raised tip should work consistently; note any loss of support, slipping or head collisions.
- Trigger a world flip while a seesaw is tilted and while standing on it. Check for clipping, unexpected launch velocity, drift or rotation endpoint reset warnings.
- R, death/respawn and retry should reset both seesaws to neutral. Winning freezes them with the rest of the gameplay.

After these pass, test Easy > Medium > Hard through Next Level and a fresh localhost browser build. Public deployment and automation remain on hold.

## Implementation and validation boundaries

LevelFlow resolves pending contacts once per physics step: death first, then stars, then goal/save. A HashSet deduplicates repeated star callbacks. RotationController emits RoomReset before transform synchronization so stars and seesaw poses reset on both ordinary and safety resets. These changes do not require LevelStars in old test scenes.

Seesaws use a kinematic Rigidbody2D with limited MoveRotation targets. Dwell is 0.55 seconds, angle limit 18 degrees, tilt speed 24 degrees/second, crossing hold 2 seconds, unloaded return 8 degrees/second. The dwell/hold is frozen through world turns; geometry is still parented under MapRoot. Real moving-platform contact and world-rotation behavior need Unity testing.

The geometry validator reads the same JSON as the builder. It checks fixed and tilted rectangles with an approximate 0.02-second motion model, ceiling stops, walking drops, red-face detours, pocket entry/escape and recovery paths. It confirms neutral jumps fail and sampled raised-tip jumps pass at the two elevated transitions. Those are geometry checks, not execution of the C# seesaw or a Unity/Box2D simulation. No Unity Editor or C# compiler is available in the authoring environment; compilation, UI, persistence and full gameplay tests remain pending.

```sh
python Tools/validate_level_layouts.py
```

Optional: Tools/render_level_overview.py regenerates the design diagram with matplotlib. Neither Python nor matplotlib is needed to run the Unity game.

Official API references checked for this revision:
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody2D.MoveRotation.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerPrefs.html

## Git after testing

Stop Play Mode and save, then:

```sh
git add Assets/HatHop Docs Tools README.md ProjectSettings/EditorBuildSettings.asset
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Shrink hollow pockets and halve level flip timings"
git push -u origin HEAD
git status
```

Include generated scenes, icons, materials and all metadata. Review custom Build Profile changes outside Assets/HatHop separately. Do not commit browser build output or Unity caches.

# Leap of Faith — controls, circuit and presentation update

Applies on top of the tested four-level project and minimal-presentation update. Keep your current local edits: this is a source patch, not a replacement Unity project.

## Install

1. Save scenes, leave Play Mode, and make a Git checkpoint. Merge the ZIP's `Assets`, `Docs`, and `Tools` folders plus its README and workflow into the project root. Preserve the included `.meta` files. No `.git`, Packages or complete ProjectSettings folder is included.
2. Wait for compilation, then choose **Leap of Faith > Apply Latest Update**. Confirm replacement of **MainMenu and Hard**. Beginner (`Prologue.unity`), Easy and Medium geometry is preserved; only their rotation countdown fields are changed. The command preflights all four scenes and requires one RotationController in each.
3. Play from MainMenu, test all four levels, and save/commit the generated scenes and product setting. If Unity reports an error, stop and inspect its first full Console message before building.

The script sets `PlayerSettings.productName` to Leap of Faith in your own settings, so this patch does not overwrite your teammate's complete settings asset. Menu title, editor menus, Web release metadata and publishing labels also use the new name. Internal HatHop paths/namespaces, the GitHub URL, score IDs, and cloud/application identifiers remain stable. Renaming the GitHub repository or Unity project folder is unnecessary for the displayed game title.

**Apply Timing and Menu Only** is available if you deliberately want to retain a hand-edited Hard scene. It does not install the circuit layout. Older full-level generation commands can replace Easy/Medium; use **Apply Latest Update** for this patch.

## Controls and countdowns

| Control | Action |
| --- | --- |
| A / D or Left / Right | Horizontal movement |
| Space or Up | Fresh-press jump, with existing buffer/coyote timing |
| R or Restart | Restart current run |
| Home | Open main menu |

Pressing D and Right together does not double speed. Opposite directions cancel. Both Input System and legacy input branches support arrows.

| Level | Traversal | Warning | Countdown before rotation |
| --- | ---: | ---: | ---: |
| Beginner / Easy / Medium | 5 s | 1 s | **6 s** |
| Hard | 4 s | 1 s | **5 s** |

The existing turn animation duration is separate from the countdown and is preserved for existing scenes; new Hard uses 0.3 seconds. The countdown restarts after the world resumes. It does not pause for idle players. Tutorial practice remains freely playable.

## Hard: outer circuit

The new room is **42 × 24 units**, with a central exit and distinct lower crossing, east tower, upper bridge and west return. It replaces the tall zigzag layout with a loop around a shared centre. Small ledges, drops and two opposite-direction seesaw launches change the movement rhythm. The camera remains close at orthographic size 5.

Two coloured outer pads must be touched in the same run. Matching lights at the exit show their state; the exit is grey while locked and green when both are active. Pads work in either orientation and stay active through flips, but reset on death or Restart. Stars stay optional: zero-star completion is still valid after both pads are active. The controls menu explains the pad rule; gameplay adds no instruction text.

There are five stars, including two deep golden pockets whose stars require a flip. Each pocket has a tested static entrance, post-flip escape, recovery ledge and return to the circuit. The red underside is dangerous on contact; an alternate landing bypasses it when inverted.

Hard data now live in `Assets/HatHop/Editor/LevelData/HardCircuit.json`, consumed by both the scene builder and geometry checker. The shared `ThreeLevels.json` is excluded from this patch to avoid overwriting Easy/Medium design work.

## Focused playtest

- Confirm Leap of Faith on the menu and in a newly built Web page; no story, hat or rabbit presentation.
- Confirm Home and Restart are text buttons during play and on completion. Next is a text button too.
- Test arrows alone, A/D + Space, mixed keys, opposing directions, and holding Up. Idle visual hop remains 0.4.
- Confirm initial countdowns 6 / 6 / 6 / 5 and their final one-second warning. Verify a new cycle after each turn.
- On Hard, touch the grey exit before collecting pads: it must not finish. Visit both pads in either order, then exit. Verify pad colours reset on death/R/Restart and stay lit through flips.
- Complete Hard in both orientations; test both seesaws, red-face bypass, pocket entry/recovery and all five stars. Static checks do not prove playability under the five-second live flip schedule.
- Open your teammate's Easy and Medium scenes: compare their layouts with your checkpoint. Only the timer fields should change from this command (Unity may also reserialize scene data).
- Build Web Release and test menu, all levels and buttons in a browser before publishing.

## Verification and Git

The authoring environment has no Unity Editor or C# compiler. Source consistency, metadata, patch scope and sampled geometry can be checked here; compilation, physics, UI appearance and hosted deployment remain pending local tests.

Use a feature branch from your current tested work, for example `git switch -c feature/leap-of-faith-release`. After import, generation and testing, review `git status` and `git diff`. Stage the intended source, generated scenes, docs and product setting, inspect `git --no-pager diff --cached --stat`, then commit and push that feature branch. Merge your teammate's changes through your agreed review process before merging to main. Do not replace the repository's `.git` folder.

The GitHub Pages tooling remains prepared but this patch does not publish anything. Rebuilds and deployment must complete successfully before a hosted game reflects the source update.

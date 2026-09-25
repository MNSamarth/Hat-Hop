# Hat Hop: menu and three levels

Combined milestone for Unity 6000.3.23f1, 2D Built-In. Apply this ZIP to the existing Hat Hop project with the tested grounded movement/camera update. It includes the entire main-menu patch, so the separate menu ZIP is not needed.

## What is included

| Level | Room size | Route landings* | Intended challenge | Traversal / warning |
| --- | --- | --- | --- | --- |
| Easy: The Foyer | 16 x 22.52 units | 15 | Broad platforms, three traversal sections, no interior hazards, longer preparation time. | 10 s / 2 s |
| Medium: False Bottom | 18 x 31.12 units | 21 | Five sections, longer horizontal sweeps, narrower landings and four edge hazards. | 8 s / 2 s |
| Hard: The Last Act | 20 x 42.96 units | 29 | Seven sections, precision platforms and eight edge hazards. | 6 s / 2 s |

*Includes the starting platform, excludes the exit alcove floor and roof. Easy's final section has an extended exit approach. Room dimensions are world units; the camera does not show the full room.

All scenes use the existing grounded movement, jump speed 8, horizontal speed 4.5, gravity scale 2, short buffer/grace, visual hop height **0.4**, and follow camera orthographic size **5** (2x the former overview). Warning is always two seconds; turn duration stays at the existing 0.6-second default. Timing is deterministic for reproducible playtests. The scene builder assumes default downward 2D gravity of -9.81 and the project's usual 0.02-second fixed timestep; it does not modify those global settings.

MainMenu contains Play, Level Select and Controls. Play starts Easy; all three levels are selectable immediately. Winning Easy or Medium shows Retry, Next Level and Main Menu. Hard shows Final Level Complete, Retry and Main Menu. Directly selecting Hard does not claim that the other two levels were completed. No progress save or unlock system is introduced.

The green exit is inside a side alcove bounded by a solid floor, roof and back wall. A straight vertical fall is blocked in both orientations. On an inverted approach, use the outside face, move around the open side and steer onto the inside shelf. Two-sided platforms remain solid in either orientation. Color bands and accented landing platforms mark sections. Red geometry is lethal; colored non-red platforms are safe.

## Integrate once

1. Stop Play Mode and save your existing scene. Work from the tested movement/camera checkpoint. Use your existing feature/main-menu branch if already created, or create `feature/menu-levels` from that checkpoint. Review `git status` before importing; save any unrelated work first.
2. Extract **Hat_Hop_Menu_And_Three_Levels.zip** outside the project. Copy **Assets**, **Docs**, **Tools** and **README.md** into the Unity project root. Merge folders and replace the supplied files. You do not need the earlier menu ZIP. Existing gameplay scripts outside the menu/HUD changes and the current test scenes are not replaced. No .git, Packages, ProjectSettings or generated .unity files are shipped.
3. Return to Unity and wait for compilation. The menu uses the Unity UI package (com.unity.ugui) and the existing input backend. If there are red Console errors, copy the first complete error before running the generator.
4. Choose **Hat Hop > Create Menu and Three Levels**. This generates and saves MainMenu, Easy, Medium and Hard; creates the level art/material/catalog; assigns all references; and opens MainMenu. Do not run the older individual test-scene generators or Apply Movement and Camera Update on these newly generated levels; the combined tool already wires the player and camera.
5. Open **File > Build Profiles > Scene List**. The global list will begin MainMenu, Easy, Medium, Hard, with GameplayTest included if present. Check that those first four scenes are enabled. If your active profile has **Override Global Scene List**, update that profile's list too. Keep MainMenu first.
6. Press Play from MainMenu and test the flows below. Save any later tuning outside Play Mode.

If you already installed the separate menu patch, this package updates it in place with the same script GUIDs. The generator asks before replacing any existing MainMenu/Easy/Medium/Hard scenes. Rebuilding resets manual edits in those four scenes and overwrites their catalog mappings; commit scene edits before rebuilding. GameplayTest, MovementTest and RotationTest are preserved. Editing JSON does not change a saved scene until regeneration.

The source JSON is `Assets/HatHop/Editor/LevelData/ThreeLevels.json`. The single level mapping asset is generated at `Assets/HatHop/Settings/LevelCatalog.asset`; its Inspector uses SceneAsset pickers. Next Level reads the same catalog as the menu. Refresh Menu Build Scenes after changing scene mappings, and reassign paths after scene moves/renames.

## Test as one package

| Test | Expected result |
| --- | --- |
| Home > Controls > Back | Controls readable; no red Console errors. |
| Play | Easy opens with a blue hopping player, close camera and Easy HUD title. |
| Level Select | Each button loads the matching difficulty, not the prototype. |
| A/D + Space on every level | Same movement settings; idle visual hops do not move the collision body or camera. |
| Reach platforms upright | Adjacent jumps have workable launch positions; no unintended ceiling traps. |
| Let warning/turn happen while standing, jumping and descending | One warned half-turn; stable resume and readable landings. |
| Miss a landing or touch a red edge hazard | Death, one respawn, fresh room orientation; R resets counters. |
| Enter exit upright and inverted | Exit can be approached from the open side; falling straight down onto its outside roof/floor does not win. |
| Win Easy > Next Level | Medium opens with fresh timer, deaths and orientation. |
| Win Medium > Next Level | Hard opens fresh. |
| Win Hard | Final Level Complete, Retry and Main Menu; no fourth-level button. |
| Retry and R after winning | Same level resets fully. |
| Main Menu during warning, turn and win | Menu works; subsequent Play starts a fresh Easy run. |
| Repeat scene transitions | No duplicate camera, EventSystem or carried-over gameplay state. |

After Editor testing, make a fresh localhost Web Build And Run and repeat menu selection, at least one full level and return navigation. **Public deployment remains on hold** until the package is tested. This milestone does not configure GitHub Pages or CI. We will publish the combined game and automate builds/deployment afterward.

These are first playable greybox layouts, not a claim that final difficulty has been balanced. Report the difficulty, landing name/section if known, and whether the map was upright or flipped when something felt unfair. A screenshot and full Console error, if any, will make fixes precise.

## Validation performed here

- The layout script reads the same JSON as the scene generator.
- It found sampled collision-free routes for every adjacent landing in upright and inverted geometry, using full-footprint landings and all solid/hazard rectangles.
- It checks the upright exit approach, inverted outer landing and a controlled side-entry maneuver.
- It checks boundary clearance, rise margin and horizontal shielding of the entire green trigger by the alcove faces.
- It models the 0.6 x 0.8 body, 4.5 horizontal speed, 8 jump speed and 19.62 gravity with a 0.02-second semi-implicit step. Walking off ledges is considered for descent. It does not model Box2D contact resolution, human timing, live rotation interruptions, rendering or camera comfort.
- Source/metadata and combined archive checks are separate from Unity validation. No Unity Editor or C# compiler is available in the authoring environment. **Unity compilation, actual full runs and browser tests remain pending.**

Optional local check (Python 3, standard library only):

```sh
python Tools/validate_level_layouts.py
```

See `Three_Levels_Overview.png` for a shared-scale diagram of the layouts. It is a design overview, not a screenshot from Unity.

## Commit after testing

Stop Play Mode and save all scene/asset edits, then:

```sh
git add Assets/HatHop Docs Tools README.md ProjectSettings/EditorBuildSettings.asset
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Add three playable levels with menu and level progression"
git push -u origin HEAD
git status
```

Include the generated scenes, catalog, art/material assets and all corresponding .meta files. If you changed a custom Build Profile outside Assets/HatHop, review and stage it separately. Review other ProjectSettings changes individually. Build output remains ignored. Merge only after your tests; do not force-push or replace repository history.

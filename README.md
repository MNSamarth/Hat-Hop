# Hat Hop

A Unity 2D platformer where warned 180-degree world rotations turn upward climbs into dangerous descents. Developed for USC CSCI 526.

## Development setup

Use Unity **6000.3.23f1**, the **2D Built-In Render Pipeline**, and matching Web Build Support. Open the Unity project root containing Assets, Packages and ProjectSettings.

Read [the development guide](Docs/Hat_Hop_Development_Guide.md) and [progress](Docs/PROGRESS.md) before changing mechanics.

## Current controls

- **A/D:** ground movement and air steering.
- **Space:** a real jump when grounded, with a short landing buffer and edge grace.
- **R:** restart the run.

The visible rabbit placeholder makes small cosmetic hops; the collision body stays grounded until a real jump or fall. The camera follows at approximately 2x the former overview magnification.

## Current revision

The user has tested the revised movement and camera and chosen a scene visual hop height of **0.4**.

The **combined menu and three-level milestone** is authored and awaiting Unity validation. Follow [combined setup and checks](Docs/THREE_LEVELS_AND_MENU.md): import Hat_Hop_Menu_And_Three_Levels.zip, then run **Hat Hop > Create Menu and Three Levels**. No separate menu ZIP is needed.

- Play starts Easy; Level Select offers Easy, Medium and Hard.
- Three progressively larger rooms with 15, 21 and 29 route landings, safe section platforms and side-alcove exits.
- Easy/Medium completion offers Next Level; Hard has a final completion screen. All levels support Retry/Restart and Main Menu.
- Static geometry checks passed for sampled traversal in both orientations. Unity compilation, live flips, full runs and browser tests are pending.

See [layout overview](Docs/Three_Levels_Overview.png). Public hosting remains on hold until testing is complete. Automated rebuild/deployment from main is planned after the combined release, not configured. Builds belong in ignored Builds/Web.

Assets and their .meta files, Packages, ProjectSettings and Docs belong in Git. Use feature branches, test before merging, and record actual contributions and AI assistance.

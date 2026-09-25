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

The **stars and platform challenges revision** is authored and awaiting Unity validation. Follow [setup and playtests](Docs/STARS_AND_PLATFORM_CHALLENGES.md): import Hat_Hop_Stars_And_Platform_Challenges.zip, then run **Hat Hop > Create Stars and Platform Challenge Levels**. This includes menu and all three levels; no earlier ZIP is needed.

- Five stars per level, including two deep pockets that require a world flip to enter.
- A 0-5 star completion result and best completed score in the HUD and Level Select. Death/restart resets the current attempt's collectibles.
- Floating exit direction marker on the right while the goal is offscreen.
- Medium and Hard introduce red undersides; Hard adds two delayed seesaws and elevated jumps that require a raised tip when upright.
- Play/Level Select, Next Level, Retry and Main Menu remain connected across all three difficulties.

See the [updated design overview](Docs/Three_Levels_Overview.png). Sampled geometry checks passed for routes, red-face detours, flip pockets and raised-tip reach. Unity compilation, moving-platform physics, saved-score behavior and full gameplay/browser tests are pending. Public hosting and automated deployment remain on hold until testing is complete.

Assets and their .meta files, Packages, ProjectSettings and Docs belong in Git. Use feature branches, test before merging, and record actual contributions and AI assistance.

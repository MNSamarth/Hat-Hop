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

The movement and camera revision is authored and awaiting Unity compilation and playtesting. Open GameplayTest and use **Hat Hop > Apply Movement and Camera Update**, then save the scene. See [setup and checks](Docs/MOVEMENT_CAMERA_UPDATE.md).

Prior user-reported checkpoints passed: rotation, hazards, death/respawn, exit/win, restart and a localhost Web build. These do not establish that the revised movement passes yet.

Next: validate movement and camera, build larger Easy/Medium/Hard levels, add menus/progression, tune difficulty, and publish. Public hosting is deferred. Generated builds belong in ignored Builds/Web; pushing source does not rebuild the hosted game.

Assets and their .meta files, Packages, ProjectSettings and Docs belong in Git. Use feature branches, test before merging, and record actual contributions and AI assistance.

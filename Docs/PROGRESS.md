# Hat Hop Progress

Updated September 24, 2026. Gameplay results below are user reports from the Windows Unity installation.

## Tested and saved checkpoints

- Unity 6000.3.23f1, 2D Built-In, Web support installed.
- Rotation, hazards, death/respawn, exit/win and restart reported working.
- Previous Web Build And Run worked on localhost; no public deployment.
- Revised grounded jumping and follow camera reported working well.
- User chose **0.4** for Player Hop Visual > Hop Height in the gameplay scene.
- Movement/camera checkpoint pushed: user terminal showed feature/movement-camera up to date with its remote. A separate automatic preload-settings change was discussed; final clean status has not been shown.

## Current milestone: combined menu and three levels

User requested all three levels and menu in one ZIP for integration/testing. Source is authored locally on feature/three-levels, based on the earlier main-menu source checkpoint.

- Complete generator: Hat Hop > Create Menu and Three Levels.
- MainMenu, Easy, Medium and Hard scenes generated and wired together.
- Easy: 22.52 units high, 15 route landings, three sections, no interior hazards.
- Medium: 31.12 units high, 21 landings, five sections, four interior edge hazards.
- Hard: 42.96 units high, 29 landings, seven sections, eight edge hazards.
- Existing grounded movement, 0.4 cosmetic hop and camera size 5 configured in all new levels.
- Shared catalog for Play, Level Select and Next Level. Final completion on Hard; Retry/Restart/Main Menu supported.
- Side-alcove exits protected from direct vertical falls in either orientation.
- JSON-backed static trajectory checks passed for sampled upright/flipped traversal and exit approaches after headroom/spacing revisions.
- Design overview PNG included; it is not a Unity screenshot.

Pending: import, Unity compilation, generated-scene inspection, full runs with live flips, difficulty tuning and localhost build checks. Follow THREE_LEVELS_AND_MENU.md. No Unity or browser pass is claimed from the static checks.

## Next milestones

1. User integrates the combined ZIP and tests the menu plus all three levels.
2. Fix reported geometry/rotation/navigation issues and tune difficulty.
3. Deploy the tested package to GitHub Pages.
4. Configure automatic builds/deployment for changes merged into main.

Public deployment remains on hold. No remote push or hosted release was performed by the authoring environment.

## Source and contribution record

The authoring checkout contains source scaffolding; the user's generated Unity scenes, settings and current GitHub history are not automatically synchronized here. Unity Editor and a C# compiler are unavailable here.

AI assistance: source, Editor tools, design/docs and review. User: local setup, Git, Unity imports, tuning and reported gameplay/browser tests. Record teammate contributions when completed. User reports current TA/grader guidance permits full AI use.

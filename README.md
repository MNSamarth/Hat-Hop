# Leap of Faith

A Unity 2D platformer built around timed world flips, precision jumps, collectible stars and finding the exit. Built for USC CSCI 526 with Unity **6000.3.23f1**, 2D Built-In.

- **A/D or Left/Right:** move and steer in the air.
- **Space or Up:** jump. Holding a jump key does not repeat jumps.
- **R / Restart:** restart the current run. **Home:** return to the menu.
- **Beginner, Easy, Medium:** 6-second countdown, including a 1-second warning.
- **Hard:** 5-second countdown, including a 1-second warning.

Beginner offers safe practice. Easy and Medium build on the movement and flip mechanics. Hard is a wide circuit: cross the lower deck, climb the east tower, traverse the upper bridge, and return through the west wing to the central exit. Light both outer pads to unlock that exit. Raised seesaw tips, red undersides and optional flip-star pockets create extra decisions. Five stars per level provide a saved best completion rating.

The menu uses abstract platforms and a square, with no story or character theme. Gameplay keeps the flip countdown, EXIT indicator and text navigation buttons. Completion shows stars, Restart and Next/Home.

## Apply this update

Import the patch into your existing tested four-level project, then run **Leap of Faith > Apply Latest Update** outside Play Mode. This rebuilds **MainMenu and Hard only**, updates the other levels' timers in place, and sets the product name. Easy and Medium geometry stays intact. Save or commit manual edits first.

See [integration and checks](Docs/LEAP_OF_FAITH_UPDATE.md), [Hard layout](Docs/Hard_Circuit_Overview.png) and [progress](Docs/PROGRESS.md).

## Build and publish

**Leap of Faith > Build Web Release** produces `Builds/WebGL`. Test it over HTTP. Existing Web publishing scripts and opt-in CI workflows remain available; see the publishing section of [the earlier release guide](Docs/THE_LAST_TRICK_RELEASE.md). Generate, test and commit scenes before building; source changes alone do not update a hosted game.

This patch has source and sampled geometry checks. Unity compilation, live gameplay and a rebuilt Web release still require local verification. No new hosted deployment is claimed.

## Repository compatibility

The existing [Hat-Hop repository](https://github.com/MNSamarth/Hat-Hop), `Assets/HatHop` paths, C# namespace, Unity asset GUIDs, application identifiers and score keys are retained for compatibility. The visible game/product title is **Leap of Faith**. Historical guide filenames and archive names refer to earlier revisions. Do not rename folders in Explorer inside Assets; coordinate scene ownership with your teammate.

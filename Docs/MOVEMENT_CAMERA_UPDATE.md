# Movement and Camera Revision

Target: Unity 6000.3.23f1, 2D Built-In. Source prepared; Unity compilation and playtesting pending.

## User-approved design change

The player moves on the ground like a conventional platformer while its visible sprite makes small hops. Space launches a real jump immediately when grounded. This replaces automatic physical hops and the indefinite next-landing queue. A/D still controls horizontal movement in the air. The camera follows the collision body at approximately 2x the previous overview scale.

This guide and the updated Development Guide supersede the old hopping rules in the design reference and earlier milestone notes. The historical milestone notes describe the earlier prototype, not current input behavior.

## Install

1. Save work and exit Play Mode. Use the existing feature/movement-camera branch based on the tested gameplay/Web checkpoint. If already on that branch, do not create it again.
2. Close Unity. Extract Hat_Hop_Movement_Camera_Update.zip and merge Assets and Docs into the project root.
3. Allow replacement of PlayerMotor2D.cs, GameplayHUD.cs, RotationTestHUD.cs, Hat_Hop_Development_Guide.md, PROGRESS.md and README.md. Keep the existing .meta files for replaced scripts. If you edited these documents locally, preserve those notes before replacing it. Other files in the patch are new. No scene files, .git directory, packages or ProjectSettings are included.
4. Reopen Unity and open the existing GameplayTest scene. Wait for compilation to finish.
5. Choose **Hat Hop > Apply Movement and Camera Update**, then Apply in the dialog. Run it outside Play Mode. It changes the open scene, preserving platform layout and gameplay references.
6. Save with Ctrl+S. Press Play, then click the Game view.

Do not regenerate GameplayTest from the original generator: that would replace your configured scene. For any newly generated scene, run Apply Movement and Camera Update again. Applying it repeatedly to the same configured scene reuses components and the original camera size; it does not repeatedly halve the view.

## What changes in the scene

- Player keeps its Rigidbody2D, BoxCollider2D and PlayerMotor2D on the root object.
- A child called Visual receives a copy of the root sprite's appearance. The root SpriteRenderer is disabled, not deleted. The child has no collider or rigidbody.
- PlayerHopVisual animates only Visual; the root collision body stays grounded. Cosmetic motion stops in the air, during turns and after death/win.
- A generated zero-friction, zero-bounce PhysicsMaterial2D is assigned to the player's collider to reduce sticking against walls. It is saved at Assets/HatHop/Physics/GroundedPlayer.physicsMaterial2D.
- Main Camera receives PlayerFollowCamera and remains outside MapRoot. Its size changes from 10 to 5 in the existing GameplayTest scene, giving 2x linear magnification at the same aspect ratio.
- The camera follows the body, not the bouncing child. It adds modest direction-based look-ahead during normal movement, directly follows the player during rotation, remains upright and snaps to the player on reset.

Undo can revert the scene configuration. The generated physics material remains as an asset even if scene changes are undone. The new PlayerMotor2D source applies to every scene using that script; old scene files are preserved, but they no longer use the old automatic physics hop.

## Starting values

| Setting | Value | Purpose |
| --- | --- | --- |
| Horizontal Speed | 4.5 | Preserves current movement speed. |
| Jump Speed | 8 | Migrates from the old Big Hop Speed field. |
| Jump Buffer Seconds | 0.12 | Allows a slightly early press just before landing. |
| Coyote Seconds | 0.08 | Forgives a press just after stepping off an edge. |
| Visual Hop Height | 0.12 world units | A small cosmetic hop; it does not change reachability. |
| Visual Hop Period | 0.32 seconds | Hops continuously while grounded, including while idle. |
| Camera Zoom | 2 | Halves the reference orthographic size. |
| Camera Follow Smooth Time | 0.15 seconds | Starting follow smoothing for normal movement. |

Jump strength and horizontal speed already saved in a scene are preserved. The setup explicitly sets the new short input buffer and edge grace to avoid retaining an old 0.5-second buffer from earlier experiments. Holding Space does not repeat jumps. A second midair press cannot launch another jump; it expires unless a landing occurs within the short buffer. Reset, suspension and application focus loss clear pending input. Ground support requires upward contact normals, so walls and platform undersides do not grant jumps.

The visual hop is intentionally small: the displayed sprite can be slightly above the true collision box. In the Scene view, select Player to inspect the stationary box when evaluating edge/hazard contact. Artwork and animation polish remain future work.

## Focused acceptance checks

- [ ] Scripts compile, and applying the menu command creates exactly one Visual child and one follow component.
- [ ] Idle: sprite hops gently, while the root Player Y stays steady after settling on a platform.
- [ ] Hold D or A and press Space at different phases of the cosmetic hop: the real jump starts without waiting for the sprite to finish its animation.
- [ ] Hold Space through landing: only the original jump occurs, with no automatic repeat.
- [ ] Press Space twice in midair away from a landing: no double jump.
- [ ] A press slightly before landing is accepted; one far earlier in the fall expires.
- [ ] Walking off an edge gives only the brief grace period, not a prolonged midair jump.
- [ ] Pushing against a wall while falling does not suspend the player; wall/ceiling contacts do not grant jumps.
- [ ] Camera is closer; it does not bounce with the idle visual and keeps the player visible through a flip.
- [ ] R during traversal/warning/turn snaps the view to the restored player and clears pending jump input.
- [ ] Death/respawn and win/restart still work; the character does not animate while the run is frozen.

The closer view alone cannot guarantee the exit stays hidden in every location. The larger Easy/Medium/Hard levels will place the exit and prepare landings with the new viewport in mind. Fairness and camera comfort must be tested before expanding levels.

## Git after testing

Save GameplayTest outside Play Mode. From the project root:

```sh
git add Assets/HatHop Docs README.md
git --no-pager diff --cached --stat
git commit -m "Replace automatic hops with grounded jumps and add follow camera"
git push -u origin HEAD
```

The commit should include the updated scene and new Physics folder/material metadata in addition to scripts and docs. Do not commit Builds/Web. The old browser build still contains the old behavior until rebuilt; public deployment is deferred by the user's request.

## Validation and sources

Authoring environment: no Unity Editor or C# compiler available. Source review and metadata/patch checks do not establish a compile or gameplay pass. Unity tests listed above remain pending.

- Orthographic size: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Camera-orthographicSize.html
- Serialized field migration: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Serialization.FormerlySerializedAsAttribute.html
- Physics material: https://docs.unity3d.com/6000.3/Documentation/Manual/2d-physics/physics-material-2d-reference.html

AI assistance: revised motor, cosmetic hop, follow camera, scene setup command and documentation prepared with Codex. Record actual human playtest results separately.

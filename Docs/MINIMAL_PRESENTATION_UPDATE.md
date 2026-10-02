> Historical milestone. Current setup, timing, title and presentation: [Leap of Faith update](LEAP_OF_FAITH_UPDATE.md).

# Minimal presentation update

This patch applies on top of The_Last_Trick_Story_Release.zip. It removes story presentation, reduces gameplay text to the flip countdown and EXIT marker, and adds one second before Hard flips. The game title stays Leap of Faith; the menu uses abstract platforms and a square, with no hat, rabbit, tagline or story.

## Apply

1. Stop Play Mode and save your work. Merge this patch into your project root, replacing its included files.
2. Wait for Unity to compile, then choose **Leap of Faith > Apply Minimal UI and Hard Timing**.
3. This rebuilds MainMenu and edits only the traversal timer in your existing Hard scene. It does not regenerate any gameplay layout. Easy and Medium scenes, level JSON, project settings and the level catalog are not included in the ZIP.
4. Play from MainMenu. The existing Prologue scene now appears as BEGINNER in Level Select; the internal scene path stays the same.

Do not run the older full level generators for this update. The targeted command updates Hard in place, preserving manual geometry edits. It sets traversal to exactly 4 seconds, so running it twice does not add a second twice. Future Hard generation also uses the same 4-second setting; the JSON layout file is left alone to avoid overwriting your teammate's edits.

## Result

- During play: only `FLIP IN …s` and the offscreen `EXIT` direction marker appear as text. The timer includes both traversal and warning, so it counts down to the actual turn rather than to the start of the warning.
- No level heading, control instructions, story lines, death/flip counters, numeric star counters, seesaw hints or tutorial prompts appear during gameplay.
- Bottom-left circular arrow: restart. Bottom-right house: main menu. Completion shows five star icons and icon buttons for retry and next level; the final level offers the home icon. No result prose is shown.
- Star collection, saved best ratings and death/respawn still work. Best-rating text remains in Level Select, alongside essential menu labels and Controls instructions; gameplay itself has no such text.
- Beginner is a freely playable safe room. Its timer starts normally, and its exit has no hidden movement/jump/star/flip requirement now that prompts are gone.
- Hard changes from 3 to 4 seconds of traversal. With the existing 1-second warning, a flip starts after 5 seconds instead of 4. The 0.3-second animation is unchanged. Easy/Medium configured timers are untouched.

## Check in Unity

Confirm the neutral menu, all four level buttons and Controls/Back. In Beginner, do nothing and confirm the countdown advances; confirm the exit works without completing an invisible checklist. Test restart, home and next-level icons, star results and death/respawn. Verify Hard starts at approximately 5 seconds while Easy/Medium retain their timers. Check the exit arrow before/after a flip.

Source checks verified that gameplay labels are limited to the timer and EXIT, the old tutorial gate/story is removed, and Easy/Medium scenes and JSON are unchanged. Unity compilation, UI appearance and live behavior remain untested here.

This patch changes shared scripts. If your teammate also edited one of those scripts, merge both sets of changes in Git rather than blindly choosing one whole file. Commit the menu and Hard scene changes generated locally together with the included source and metadata. Deployment remains a separate step after testing and integrating your teammate's work.

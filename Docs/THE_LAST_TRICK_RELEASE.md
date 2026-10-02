> Historical milestone. Current setup, timing, title and presentation: [Leap of Faith update](LEAP_OF_FAITH_UPDATE.md).

# Leap of Faith release

This update is based on the Hat-Hop-main.zip supplied on October 1, 2026. It adds a guided Prologue, replaces Hard's repeated staircase with an asymmetric route, connects four levels, and prepares Web publishing. The menu, opening lines, ending and Unity product name now use Leap of Faith. The GitHub repository, asset paths, script namespace and existing asset GUIDs remain compatible with the shared project.

## Apply and play

1. Save your current project and start a feature branch from your team's updated main. Merge the patch ZIP contents into the project root, replacing included files. Keep your existing Git history.
2. Wait for Unity to compile. Run **Leap of Faith > Prepare Four-Level Release**. This replaces MainMenu and Hard and creates/replaces Prologue. Existing Easy and Medium scenes are preserved. Commit any manual edits to the replaced scenes before running it.
3. Open MainMenu and press Begin the Story. Play through Prologue > Easy > Medium > Hard. Level Select also opens any of the four levels independently.
4. Save the generated scenes and catalog. The global scene list starts MainMenu, Prologue, Easy, Medium, Hard. The dedicated Web build command uses those exact five scenes even if a Build Profile has a different override.

The older Leap of Faith generator commands remain available for compatibility; those regenerate all four levels. For this release use the targeted command above. No components need to be attached by hand. Movement speed 4.5, real jump speed 8, visual hop height 0.4 and close camera size 5 remain as before.

## Story and level intent

The magician's performance is over, but the rabbit is still inside the hat. The player learns how the trick works, crosses its hidden chambers and escapes before the next applause. Story appears in short, non-blocking lines; controls stay active.

| Stage | Purpose | Timing before each turn |
| --- | --- | --- |
| Prologue — Behind the Curtain | Move, jump, collect a star, experience a turn, follow the goal arrow | Initially paused; then 8 s traversal + 2 s warning + 0.6 s turn |
| Easy — The Foyer | Apply the core controls and orientation changes | Existing scene retained; supplied scene tuning remains |
| Medium — False Bottom | Combine pocket detours and dangerous undersides | Existing scene retained; supplied scene tuning remains |
| Hard — Break the Illusion | Plan across irregular chambers, drops, red faces and opposite-direction seesaw launches | 3 s traversal + 1 s warning + 0.3 s turn |

Prologue has safe enclosing surfaces and broad platforms. The first timer starts only after movement, an upward jump and a collected star have been observed. Touching the goal early cannot finish the lesson; after one completed turn it becomes usable. Restart resets the lesson, stars and schedule. There are five stars, but only the first is needed for the introductory lesson. Later levels retain optional 0–5-star clears.

Score IDs remain Easy=0, Medium=1, Hard=2, Prologue=3, so adding the introduction does not shift the existing score keys. The renamed desktop product may use a different PlayerPrefs storage location; browser saves are local to the hosting origin. This is not cloud synchronization.

## Hard route design

Hard is 24 by 43 world units, with 29 primary landings. Horizontal spacing, platform widths and rises vary. Three hanging/vertical partitions break up open cross-room shortcuts. Large safe surfaces punctuate tighter sequences; no unseen hazard is added merely to surprise the player.

| Section | Primary landings | Decision or execution challenge |
| --- | --- | --- |
| Opening crossing | 00–04 | Cross from the lower left to the far right; choose whether to detour into the first flip-star pocket. |
| Reverse the rigging | 05–10 | Turn back toward the left and use the first seesaw's raised left tip to reach Landing 09. |
| False route | 11–16 | Avoid the red underside at 11, then deliberately drop from 12 to 13 before climbing out of the side chamber. |
| Left pocket | 17–19 | Choose a safe side of the red face at 17 and decide whether to enter the second star pocket after a flip. |
| Second launch | 20–22 | The second seesaw launches right, reversing the technique learned at the first. |
| Final crossing | 23–28 | Avoid the red face at 23, negotiate the hanging screen, then cross back to the exit alcove. |

The two pocket stars still require a flip. The fifth star rewards the second raised-tip landing; two others mark a crossing and the side chamber. Pocket escapes lead to recovery shelves and return steps, not directly to the highest nearby platform. Some inverted routes use these secondary ledges or a walk-off-and-steer-back maneuver. Seesaws are 4.6 units wide with an 18-degree limit; a neutral jump cannot span their 2-unit rises in the sampled model.

The overview is a design diagram, not a Unity screenshot: Story_Levels_Overview.png.

## Required local validation

- No red compilation errors after import.
- Prologue: remain still for 15 seconds; no turn. Move, jump, collect a star; the countdown starts. Survive the turn, then finish. Press R during each lesson stage and confirm a clean reset.
- Prologue: deliberately fall from several platforms in both orientations; recover using the safe floor/ceiling and platforms. Confirm the exit stays blocked before the lesson finishes.
- Menu: all four buttons, Back, Controls, Retry and every Next Level transition work. Completing Hard shows the ending and no nonexistent next level.
- Hard: test both seesaws (left launch at 08, right launch at 20), the drop at 12–13, all three marked red faces, both pocket entries and return routes, and flips while standing on tilted beams.
- Stars reset after death/R/retry, results match collection, and saved best scores never decrease. Verify Prologue has its own score.
- Confirm the arrow points at the rotated exit, including when the goal is covered by story/tutorial UI. Confirm the 0.4 visual hop and close camera are unchanged.
- Make a Web build and test from an HTTP server, including browser input, all menu transitions and completion. A source check is not a browser playtest.

Final sampled geometry checks passed for Prologue, Easy, Medium and Hard, including both pocket return routes and both raised-tip launches. YAML parsing, C# delimiter checks, asset GUID uniqueness and patch integrity were also checked; these do not establish compilation.

The geometry validator samples static rectangles, tilted rectangles, jumps, ceiling stops, walking drops and recovery routes. It does not simulate actual Box2D contacts, moving-platform transport, real input timing, the full timed flip cycle or human difficulty. Unity Editor, a C# compiler and PowerShell are unavailable in the authoring environment. C# compilation, tutorial behavior, the Windows publisher, CI and hosted play remain unverified until run locally/on GitHub.

## Commit the source release

From your existing project, before importing if possible:

```sh
git switch main
git pull --ff-only origin main
git switch -c feature/the-last-trick-release
```

After importing, generating and testing:

```sh
git add Assets/HatHop Docs Tools README.md .gitignore .github ProjectSettings/ProjectSettings.asset ProjectSettings/EditorBuildSettings.asset
git --no-pager diff --cached --stat
git diff --cached --check
git commit -m "Rename to Leap of Faith and add story release with prologue and redesigned Hard"
git push -u origin feature/the-last-trick-release
```

Merge the reviewed release through a pull request, or locally after coordinating with your teammate:

```sh
git switch main
git pull --ff-only origin main
git merge feature/the-last-trick-release
git push origin main
```

Stop if Git reports a conflict. Include all generated scenes and their metadata. Do not add Builds, Library, credentials or license files.

## First GitHub Pages deployment

This route uses the licensed Unity installation already on your Windows computer. It does not need Unity credentials in GitHub.

1. In the repository, set **Settings > Pages > Build and deployment > Source = GitHub Actions**. If the github-pages environment restricts deployment branches, allow `main` and `pages-build` under **Settings > Environments > github-pages**.
2. In Unity, run **Leap of Faith > Build Web Release**. It writes Builds/WebGL using gzip with decompression fallback and no Web threads, so it does not depend on custom server headers. It fails if Prologue or a catalog mapping is missing. It never regenerates scenes during a build.
3. Test over HTTP. With Python installed, run `py -m http.server 8000 --directory Builds/WebGL`, then open http://localhost:8000. Stop the server with Ctrl+C after testing.
4. Save and commit any intentional settings changes from Unity, push main, then close Unity Editor.
5. Open PowerShell in the project root and run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Tools\Publish-WebBuild.ps1
```

The publisher checks for a clean working tree, builds again from the committed source, checks the build output, records the source revision and pushes an isolated `pages-build` branch. It does not switch your project branch or force-push main. If Unity modifies tracked settings, it stops: review/commit those changes and rerun. If your Unity installation path differs, pass `-UnityPath "C:\path\to\Unity.exe"`.

The **Publish tested Web build** Action then uploads and deploys the static output. A push is not proof that deployment succeeded: check the Action's green result and open the reported URL. With the existing repository name, the expected address is https://mnsamarth.github.io/Hat-Hop/ — this document does not claim it is live.

Files of 100 MiB or larger are rejected by the local publisher; use the artifact-based CI route below if a build exceeds that Git file limit. The temporary publishing checkout is retained and its path printed so a failed push can be inspected. Do not run the manual publisher concurrently with automatic Unity deployments.

## Automatic rebuilds after main changes

The **Build Unity and deploy Pages** workflow is included but remains skipped until configured. This prevents an unconfigured cloud license from breaking every push.

1. Follow GameCI's activation guide for your license. For the supplied Personal-license workflow, create repository Actions secrets `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD`. Enter them only in GitHub Secrets; do not put them in chat or the repository. License activation and availability of the exact Unity editor image must succeed before automatic builds can run.
2. Create repository Actions variable `UNITY_CI_ENABLED` with value `true`.
3. Run **Actions > Build Unity and deploy Pages > Run workflow**, selecting main. Confirm a successful build and deployment before relying on automation.

Thereafter changes to Assets, Packages, ProjectSettings or that workflow on main trigger a new Unity Web build and Pages deployment. Feature branch pushes do not deploy. CI builds the committed scenes; layout JSON or generator edits require regenerating and committing the affected scenes first. Python-only docs/tool edits do not rebuild the game. If cloud activation is unavailable, leave the variable unset and use the local publisher.

No GitHub push or Pages deployment was performed in the authoring environment: network access to the repository was denied, and no Unity installation or account secrets are available there.

Official references consulted October 1, 2026:
- Unity build API: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
- Unity Web compression settings: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl/compressionformat
- GitHub Pages workflows: https://docs.github.com/en/pages/getting-started-with-github-pages/using-custom-workflows-with-github-pages
- GameCI builder: https://game.ci/docs/github/builder/
- GameCI activation: https://game.ci/docs/github/activation/

# Unity validation

Use the pinned Unity **2022.3.37f1**. Do not upgrade the project to satisfy a Hub warning.
All commands operate on `Desktop/Zombtoy`, not the older `Desktop/Zombtoy-Project` copy.

## Batch checks

Close this checkout's editor first. From the repository root:

```sh
DevTools/shell_scripts/validate-unity.sh assets
DevTools/shell_scripts/validate-unity.sh build
```

Override `UNITY_EDITOR_PATH` or `ZOMBTOY_VALIDATION_DIR` if necessary. Default artifacts go to
`/private/tmp/zombtoy-validation/`: asset report, Unity logs and a development macOS player.
The runner requires the command's positive `[Validation] PASS:` marker, not only process exit 0.
Licensing and editor caches may require running outside a restricted agent sandbox.

`ZombtoyValidation.ValidateProject` checks seven enabled build scenes and first-party prefabs at the
Assets root or in Assets/Prefabs and Assets/Guns. It includes inactive objects, missing scripts and
serialized object references with unresolved instance IDs. It explicitly validates the Titan's attack
references and dedicated noncolliding GroundTarget. Null fields assigned later by Start (e.g. Player in
the boss prefab) are not falsely treated as missing required assignments. Unresolved Renderer.m_LightmapParameters references are retained as lighting warnings (57 in the baseline),
not gameplay errors. No lighting asset is repaired by this command. This is a targeted inspector
check, not proof that every possible null field or unresolved GUID is detectable.

Scenes and prefab contents are opened through Unity APIs, never saved; the original scene setup is
restored. Dirty scenes prevent the scan. Build validation checks Unity's BuildReport as well as logs.

## Observed Play-mode smoke session

Launch with `ZOMBTOY_VALIDATION_DIR=/private/tmp/zombtoy-validation ./DevTools/shell_scripts/open-unity.sh`.
In Unity's **Tools > Zombtoy** menu:

1. **Begin Smoke Session (preserve prefs)** before Play. This snapshots the score/audio PlayerPrefs keys
   used by this project, including whether each key originally existed. A previous unrestored backup blocks
   another session, so a crashed session cannot silently overwrite the original values.
2. Enter Play through the editor. Use ordinary keyboard/mouse gameplay. The editor observer writes
   `play.jsonl` and `latest-play.json` every half second; **Capture Play State / F8** also captures on demand.
3. Exit Play, then **End Smoke Session (restore prefs)**. Backups remain recoverable after a crash and the
   restore command works after reopening Unity. Do not delete an unrestored backup.

Snapshots include player position/rotation/health, ammo/reload state, scores, enemies, projectiles,
boss range/marker state and Floor collider positions. The observer never moves objects, fires weapons,
sets health or synthesizes game input. Computer Use performs the visible interactions and collects
screenshots. The owner remains the judge of balance and feel.

Use a temporary .NET backend content root/database for the results/leaderboard flow. Keep the real
`zombtoy.db` untouched. Never reuse an unknown listener on port 3000 for a write test.

Record pass/fail and evidence in [EXECUTION_LOG](EXECUTION_LOG.md). A compile-only success does not
complete the GUI checklist. On a runtime failure, preserve logs/state and pause unrelated changes.

## Computer-use result on this Mac (2026-10-30)

Editor menus, scene loading and Play-mode toggling work. Synthetic clicks on runtime uGUI buttons
did not navigate, while the owner's physical PLAY click did. Until that limitation is resolved, use
owner input for the gameplay checklist and the observer/logs for evidence; do not claim unattended
input coverage. The utility intentionally does not bypass input by invoking weapon/menu methods.

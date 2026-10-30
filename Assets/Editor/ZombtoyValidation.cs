// Editor-only inspection commands. No scene saves, runtime hooks or asset repairs.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ZombtoyValidation
{
    const string SessionKey = "ZombtoyValidation.SmokeSession";
    static double nextSample;
    static string OutputDir => Environment.GetEnvironmentVariable("ZOMBTOY_VALIDATION_DIR")
        ?? Path.Combine(Path.GetTempPath(), "zombtoy-validation");
    static readonly string[] IntKeys = { "HighScore", "Score", "LastRunScore", "LastRunHighScore",
        "LastRunKills", "HasFinalizedRun", "AudioMuted" };
    static readonly string[] FloatKeys = { "MasterVolume", "MusicVolume", "SFXVolume" };

    [Serializable] class CheckReport
    {
        public string utc;
        public string project;
        public List<string> inspected = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
    }
    [Serializable] class PrefEntry
    {
        public string key;
        public bool existed;
        public bool isFloat;
        public int intValue;
        public float floatValue;
    }
    [Serializable] class PrefBackup { public List<PrefEntry> entries = new List<PrefEntry>(); }
    [Serializable] class AmmoState
    {
        public string path;
        public bool active;
        public int magazine, reserve;
        public bool reloading;
    }
    [Serializable] class BossState
    {
        public string name;
        public Vector3 position, markerPosition;
        public bool inRange, markerActive;
    }
    [Serializable] class PlaySnapshot
    {
        public string utc, scene;
        public float time, timeScale;
        public bool hasPlayer, dead;
        public int health, score, kills, enemies, playerRockets, enemyProjectiles;
        public Vector3 playerPosition, playerEuler;
        public AmmoState[] ammo;
        public BossState[] bosses;
        public string[] floors;
    }

    static ZombtoyValidation()
    {
        EditorApplication.update += ObserveSession;
    }

    static void Write(string name, object value)
    {
        Directory.CreateDirectory(OutputDir);
        File.WriteAllText(Path.Combine(OutputDir, name), JsonUtility.ToJson(value, true));
    }

    static T[] SceneObjects<T>() where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.gameObject.scene.IsValid() && !EditorUtility.IsPersistent(x)).ToArray();

    static string ObjectPath(Transform t)
    {
        return t.parent == null ? t.name : ObjectPath(t.parent) + "/" + t.name;
    }

    static void Require(bool condition, string context, string message, CheckReport report)
    {
        if (!condition) report.errors.Add(context + ": " + message);
    }

    static void Inspect(GameObject root, string asset, CheckReport report)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string context = asset + ":" + ObjectPath(t);
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            Require(missing == 0, context, missing + " missing script(s)", report);
            foreach (Component component in t.GetComponents<Component>())
            {
                if (component == null) continue;
                var serialized = new SerializedObject(component);
                var property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                    {
                        string message = context + ":" + component.GetType().Name + "." +
                            property.propertyPath + " has an unresolved object reference";
                        // Legacy environment lighting configuration is optional to gameplay.
                        // Preserve every finding in the report rather than silently ignoring it.
                        if (component is Renderer && property.propertyPath == "m_LightmapParameters")
                            report.warnings.Add(message);
                        else report.errors.Add(message);
                    }
                }
            }
        }
        foreach (EnemyTargetShooting boss in root.GetComponentsInChildren<EnemyTargetShooting>(true))
        {
            string context = asset + ":" + ObjectPath(boss.transform);
            Require(boss.GetComponent<EnemyHealth>() != null, context, "missing EnemyHealth", report);
            Require(boss.Projectile != null, context, "missing Projectile", report);
            Require(boss.shootPoint != null, context, "missing shootPoint", report);
            Require(boss.Range != null, context, "missing Range", report);
            Require(boss.cooldown > 0, context, "cooldown must be positive", report);
            Require(boss.groundTarget != null, context, "missing groundTarget", report);
            if (boss.groundTarget == null) continue;
            var target = boss.groundTarget;
            Require(target != boss.gameObject && target.transform.IsChildOf(boss.transform),
                context, "groundTarget must be the dedicated prefab child", report);
            Require(target.layer != LayerMask.NameToLayer("Floor"), context,
                "groundTarget must not be on Floor layer", report);
            Require(target.GetComponentsInChildren<Collider>(true).Length == 0, context,
                "groundTarget must not intercept physics raycasts", report);
            Require(target.GetComponentInChildren<Renderer>(true) != null, context,
                "groundTarget has no renderer", report);
            // Player may be null in a prefab: Start() deliberately resolves it at runtime.
        }
    }

    [MenuItem("Tools/Zombtoy/Validate Project")]
    public static void ValidateProject()
    {
        RunCommand(() => {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before asset validation");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => SceneManager.GetSceneByPath(s.path).isDirty))
                throw new InvalidOperationException("Unsaved scene edits: save or discard them before validation");
            var report = new CheckReport { utc = DateTime.UtcNow.ToString("o"), project = Directory.GetCurrentDirectory() };
            try
            {
                Require(LayerMask.NameToLayer("Floor") == 8, "TagManager", "Floor layer must be index 8", report);
                var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                Require(scenes.Length == 7, "Build Settings", "Expected the seven current build scenes", report);
                foreach (string path in scenes)
                {
                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects()) Inspect(root, path, report);
                    report.inspected.Add(path);
                }
                var prefabs = AssetDatabase.FindAssets("t:Prefab").Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) &&
                        (p.Count(c => c == '/') == 1 || p.StartsWith("Assets/Prefabs/") || p.StartsWith("Assets/Guns/")))
                    .OrderBy(p => p, StringComparer.Ordinal);
                foreach (string path in prefabs)
                {
                    GameObject contents = PrefabUtility.LoadPrefabContents(path);
                    try { Inspect(contents, path, report); }
                    finally { PrefabUtility.UnloadPrefabContents(contents); }
                    report.inspected.Add(path);
                }
            }
            finally
            {
                try
                {
                    // A fresh batchmode process may have no loaded/active scene to restore.
                    if (setup.Any(s => s.isLoaded) && setup.Count(s => s.isActive) == 1)
                        EditorSceneManager.RestoreSceneManagerSetup(setup);
                    else
                        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }
                finally { Write("assets.json", report); }
            }
            foreach (string error in report.errors) Debug.LogError("[Validation] " + error);
            if (report.warnings.Count > 0)
                Debug.LogWarning("[Validation] " + report.warnings.Count + " unresolved legacy lighting references; see assets.json");
            if (report.errors.Count > 0) throw new InvalidOperationException(report.errors.Count + " asset validation failures; see assets.json");
            Debug.Log("[Validation] PASS: inspected " + report.inspected.Count + " scenes/prefabs; " + OutputDir);
        });
    }

    [MenuItem("Tools/Zombtoy/Capture Play State _F8")]
    public static void CapturePlayState()
    {
        if (!EditorApplication.isPlaying) { Debug.Log("[Validation] Capture requires Play mode"); return; }
        var player = SceneObjects<PlayerHealth>().FirstOrDefault(x => x.gameObject.activeInHierarchy);
        var scores = SceneObjects<ScoreManager>().FirstOrDefault();
        var snapshot = new PlaySnapshot {
            utc = DateTime.UtcNow.ToString("o"), scene = SceneManager.GetActiveScene().name,
            time = Time.time, timeScale = Time.timeScale, hasPlayer = player != null,
            health = player != null ? player.currentHealth : -1, dead = player != null && player.isDead,
            playerPosition = player != null ? player.transform.position : Vector3.zero,
            playerEuler = player != null ? player.transform.eulerAngles : Vector3.zero,
            score = scores != null ? scores.CurrentScore : -1, kills = scores != null ? scores.MonsterKills : -1,
            enemies = SceneObjects<EnemyHealth>().Count(x => x.gameObject.activeInHierarchy && x.currentHealth > 0),
            playerRockets = SceneObjects<Rocket>().Length, enemyProjectiles = SceneObjects<EnemyProjectile>().Length,
            ammo = SceneObjects<Ammo>().OrderBy(x => ObjectPath(x.transform)).Select(x => new AmmoState {
                path = ObjectPath(x.transform), active = x.gameObject.activeInHierarchy,
                magazine = x.ammo, reserve = x.Capacity, reloading = x.ReloadCheck != null && x.ReloadCheck.reload }).ToArray(),
            bosses = SceneObjects<EnemyTargetShooting>().Select(x => new BossState {
                name = x.name, position = x.transform.position, inRange = x.Range != null && x.Range.inRange,
                markerActive = x.groundTarget != null && x.groundTarget.activeInHierarchy,
                markerPosition = x.groundTarget != null ? x.groundTarget.transform.position : Vector3.zero }).ToArray(),
            floors = SceneObjects<Collider>().Where(x => x.gameObject.layer == LayerMask.NameToLayer("Floor"))
                .Select(x => ObjectPath(x.transform) + " active=" + x.gameObject.activeInHierarchy + " position=" + x.transform.position).ToArray()
        };
        Directory.CreateDirectory(OutputDir);
        File.AppendAllText(Path.Combine(OutputDir, "play.jsonl"), JsonUtility.ToJson(snapshot) + Environment.NewLine);
        Write("latest-play.json", snapshot);
    }

    [MenuItem("Tools/Zombtoy/Begin Smoke Session (preserve prefs)")]
    public static void BeginSmokeSession()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Begin before entering Play mode");
        string path = Path.Combine(OutputDir, "prefs-backup.json");
        if (SessionState.GetBool(SessionKey, false) || File.Exists(path))
            throw new InvalidOperationException("An existing prefs backup must be restored with End Smoke Session first");
        var backup = new PrefBackup();
        foreach (string key in IntKeys) backup.entries.Add(new PrefEntry {
            key = key, existed = PlayerPrefs.HasKey(key), intValue = PlayerPrefs.GetInt(key) });
        foreach (string key in FloatKeys) backup.entries.Add(new PrefEntry {
            key = key, existed = PlayerPrefs.HasKey(key), isFloat = true, floatValue = PlayerPrefs.GetFloat(key) });
        Write("prefs-backup.json", backup);
        SessionState.SetBool(SessionKey, true);
        Debug.Log("[Validation] Smoke session started; prefs backup at " + path);
    }

    [MenuItem("Tools/Zombtoy/End Smoke Session (restore prefs)")]
    public static void EndSmokeSession()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before restoring prefs");
        string path = Path.Combine(OutputDir, "prefs-backup.json");
        if (!File.Exists(path)) throw new InvalidOperationException("No prefs backup exists at " + path);
        var backup = JsonUtility.FromJson<PrefBackup>(File.ReadAllText(path));
        foreach (var entry in backup.entries)
        {
            if (!entry.existed) PlayerPrefs.DeleteKey(entry.key);
            else if (entry.isFloat) PlayerPrefs.SetFloat(entry.key, entry.floatValue);
            else PlayerPrefs.SetInt(entry.key, entry.intValue);
        }
        PlayerPrefs.Save();
        SessionState.SetBool(SessionKey, false);
        File.Move(path, Path.Combine(OutputDir, "prefs-restored-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + ".json"));
        Debug.Log("[Validation] Smoke session ended; original prefs restored");
    }

    static void ObserveSession()
    {
        if (!SessionState.GetBool(SessionKey, false) || !EditorApplication.isPlaying ||
            EditorApplication.isCompiling || EditorApplication.timeSinceStartup < nextSample) return;
        nextSample = EditorApplication.timeSinceStartup + 0.5;
        CapturePlayState();
    }

    [MenuItem("Tools/Zombtoy/Build Mac Smoke Player")]
    public static void BuildMacSmokePlayer()
    {
        RunCommand(() => {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before building");
            string path = Path.Combine(OutputDir, "Build", "Zombtoy.app");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path, target = BuildTarget.StandaloneOSX, options = BuildOptions.Development });
            File.WriteAllText(Path.Combine(OutputDir, "build.txt"), report.summary.result +
                " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + "\n" + path);
            if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors != 0)
                throw new InvalidOperationException("Player build failed: " + report.summary.result);
            Debug.Log("[Validation] PASS: standalone build " + path);
        });
    }

    static void RunCommand(Action action)
    {
        int result = 0;
        try { action(); }
        catch (Exception error) { Debug.LogException(error); result = 2; }
        if (Application.isBatchMode) EditorApplication.Exit(result);
    }
}

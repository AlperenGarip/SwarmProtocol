using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using SwarmProtocol.Combat;
using SwarmProtocol.Combat.Strategies;
using SwarmProtocol.Enemies;
using SwarmProtocol.Player;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Tools
{
    /// <summary>
    /// Editor/runtime benchmarking helper for the CMPE 485 profiler experiments.
    ///
    /// Hotkeys (Editor + builds):
    ///   F1  spawn 50 enemies via EnemySpawner
    ///   F2  clear all active enemies (despawn via Die())
    ///   F3  toggle aura detection method (Registry ↔ Physics)
    ///   F4  reset frame-time samples
    ///   F5  toggle FULL BENCHMARK MODE (invincible + weapons OFF + aura auto-fires)
    ///   F6  toggle projectile spawn method (Pooled ↔ Instantiate)
    ///   F7  toggle player nearest-enemy method (Registry ↔ FindGameObjectsWithTag)
    ///   F8  toggle INVINCIBILITY-ONLY (player can't die, weapons still fire)
    ///   F9  toggle RAPID BURST (fire RapidFireStrategy N times/frame — drives pooling benchmark)
    ///   F12 dump last N seconds of frame samples to CSV (Application.persistentDataPath)
    ///
    /// Shows an on-screen overlay with current settings + rolling frame stats.
    /// </summary>
    public class BenchmarkRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemySpawner       spawner;
        [SerializeField] private EnemyDataSO        benchmarkEnemy;       // which enemy type to spawn (e.g. swarmer)
        [SerializeField] private AuraStrategySO     auraToToggle;         // optional — Aura strategy to flip method on
        [SerializeField] private RapidFireStrategySO rapidFireToToggle;   // optional — flip projectile pool/instantiate
        [SerializeField] private PlayerController    playerToToggle;      // optional — flip nearest-enemy lookup method

        [Header("Spawn config")]
        [SerializeField] private int   spawnBatchCount     = 50;
        [SerializeField] private float spawnDifficultyMult = 1f;

        [Header("Sampling")]
        [SerializeField] private int   sampleBufferSize    = 600;   // ~10s at 60fps
        [SerializeField] private bool  showOverlay         = true;
        [SerializeField] private bool  hotkeysEnabled      = true;

        [Header("Benchmark mode")]
        [Tooltip("When ON: player can't die, player weapons don't fire, BenchmarkRunner directly fires the Aura strategy at the player every auraFireInterval seconds.")]
        [SerializeField] private bool  benchmarkMode       = false;
        [SerializeField] private float auraFireInterval    = 0.1f;   // 10 fires/sec — same as a typical aura
        [SerializeField] private float auraBenchmarkRange  = 8f;     // detection radius used for the synthetic aura tick

        [Tooltip("When ON: player can't die, but weapons still fire normally. Use for projectile-spawn benchmarks where you NEED the weapon to be firing.")]
        [SerializeField] private bool  invincibilityOnly   = false;

        [Tooltip("When ON: BenchmarkRunner directly fires the RapidFireStrategy N times every frame, bypassing weapons. Drives the pooling benchmark to high spawn rates so the difference is visible.")]
        [SerializeField] private bool  rapidBurst          = false;
        [Tooltip("Shots per frame in burst mode. Keep low (3-8) so render cost of in-flight projectiles doesn't dominate the frame and mask the spawn-cost difference.")]
        [SerializeField] private int   rapidBurstShotsPerFrame = 5;

        // Circular buffer of recent frame times (ms)
        private float[] _frameMs;
        private int     _writeIdx;
        private int     _validCount;

        // Benchmark-mode state
        private float          _nextAuraTick;
        private PlayerHealth   _cachedPlayerHealth;
        private WeaponManager  _cachedWeaponManager;
        private Transform      _cachedPlayerTransform;
        private WeaponDataSO   _benchmarkAuraWeaponData;
        private WeaponDataSO   _cachedRapidFireWeaponData; // a real WeaponDataSO whose fireStrategy == rapidFireToToggle

        private void Awake()
        {
            _frameMs = new float[Mathf.Max(60, sampleBufferSize)];
            AutoBindIfMissing();
        }

        private void AutoBindIfMissing()
        {
            if (spawner == null)
                spawner = Object.FindFirstObjectByType<EnemySpawner>(FindObjectsInactive.Include);

            if (auraToToggle == null)
            {
                foreach (var so in Resources.FindObjectsOfTypeAll<AuraStrategySO>())
                { auraToToggle = so; break; }
            }

            if (benchmarkEnemy == null)
            {
                foreach (var so in Resources.FindObjectsOfTypeAll<EnemyDataSO>())
                {
                    if (so.name.IndexOf("Swarmer", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    { benchmarkEnemy = so; break; }
                    if (benchmarkEnemy == null) benchmarkEnemy = so;
                }
            }

            if (auraToToggle != null && auraToToggle.physicsEnemyMask.value == ~0)
                auraToToggle.physicsEnemyMask = 1 << 8; // Enemy layer

            if (rapidFireToToggle == null)
            {
                foreach (var so in Resources.FindObjectsOfTypeAll<RapidFireStrategySO>())
                { rapidFireToToggle = so; break; }
            }

            if (playerToToggle == null)
                playerToToggle = Object.FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);

            Debug.Log($"[Benchmark] Auto-bound: spawner={(spawner != null ? spawner.name : "NULL")} " +
                      $"enemy={(benchmarkEnemy != null ? benchmarkEnemy.name : "NULL")} " +
                      $"aura={(auraToToggle != null ? auraToToggle.name : "NULL")} " +
                      $"rapidFire={(rapidFireToToggle != null ? rapidFireToToggle.name : "NULL")} " +
                      $"player={(playerToToggle != null ? playerToToggle.name : "NULL")}");
        }

        private void Update()
        {
            // Record sample
            float ms = Time.unscaledDeltaTime * 1000f;
            _frameMs[_writeIdx] = ms;
            _writeIdx = (_writeIdx + 1) % _frameMs.Length;
            _validCount = Mathf.Min(_validCount + 1, _frameMs.Length);

            // Hotkeys (Input System — no legacy Input)
            if (hotkeysEnabled && Keyboard.current != null)
            {
                if (Keyboard.current.f1Key.wasPressedThisFrame)  SpawnBatch();
                if (Keyboard.current.f2Key.wasPressedThisFrame)  ClearEnemies();
                if (Keyboard.current.f3Key.wasPressedThisFrame)  ToggleAuraMethod();
                if (Keyboard.current.f4Key.wasPressedThisFrame)  ResetSamples();
                if (Keyboard.current.f5Key.wasPressedThisFrame)  ToggleBenchmarkMode();
                if (Keyboard.current.f6Key.wasPressedThisFrame)  ToggleProjectileSpawnMethod();
                if (Keyboard.current.f7Key.wasPressedThisFrame)  ToggleFindMethod();
                if (Keyboard.current.f8Key.wasPressedThisFrame)  ToggleInvincibilityOnly();
                if (Keyboard.current.f9Key.wasPressedThisFrame)  ToggleRapidBurst();
                if (Keyboard.current.f12Key.wasPressedThisFrame) DumpCsv();
            }

            if (benchmarkMode)         TickBenchmarkMode();
            else if (invincibilityOnly) TickInvincibilityOnly();

            if (rapidBurst) TickRapidBurst();
        }

        // ─── Benchmark mode (player frozen, aura fires from BenchmarkRunner) ─────

        public void ToggleBenchmarkMode()
        {
            benchmarkMode = !benchmarkMode;
            if (benchmarkMode)
            {
                invincibilityOnly = false; // mutually exclusive
                EnterBenchmarkMode();
            }
            else ExitBenchmarkMode();
        }

        private void EnterBenchmarkMode()
        {
            CachePlayerRefs();
            // Player invincible: very long i-frames so TakeDamage early-exits
            _cachedPlayerHealth?.GrantIFrames(99999f);
            // Player weapons frozen so they don't kill the swarm we're trying to count
            if (_cachedWeaponManager != null) _cachedWeaponManager.IsOverrideActive = true;
            _nextAuraTick = 0f;
            Debug.Log("[Benchmark] BENCHMARK MODE ON — player invincible, weapons disabled, aura auto-firing.");
        }

        private void ExitBenchmarkMode()
        {
            if (_cachedWeaponManager != null) _cachedWeaponManager.IsOverrideActive = false;
            Debug.Log("[Benchmark] Benchmark mode OFF — player resumes normally (i-frames will lapse on next damage tick).");
        }

        private void CachePlayerRefs()
        {
            if (_cachedPlayerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null)
                {
                    _cachedPlayerTransform = p.transform;
                    _cachedPlayerHealth    = p.GetComponent<PlayerHealth>();
                    _cachedWeaponManager   = p.GetComponentInChildren<WeaponManager>();
                }
            }

            // Build a one-off WeaponDataSO so AuraStrategySO.Execute has a valid ctx
            if (_benchmarkAuraWeaponData == null)
            {
                _benchmarkAuraWeaponData = ScriptableObject.CreateInstance<WeaponDataSO>();
                _benchmarkAuraWeaponData.weaponName       = "BenchmarkAura";
                _benchmarkAuraWeaponData.baseDamage       = 0f; // never kills enemies — preserves count
                _benchmarkAuraWeaponData.range            = auraBenchmarkRange;
                _benchmarkAuraWeaponData.knockbackForce   = 0f;
                _benchmarkAuraWeaponData.fireRate         = 1f / auraFireInterval;
            }
        }

        private void TickBenchmarkMode()
        {
            CachePlayerRefs();
            if (auraToToggle == null || _cachedPlayerTransform == null) return;

            // Re-grant invincibility periodically so it never lapses
            if (_cachedPlayerHealth != null) _cachedPlayerHealth.GrantIFrames(99999f);

            _nextAuraTick -= Time.unscaledDeltaTime;
            if (_nextAuraTick > 0f) return;
            _nextAuraTick = auraFireInterval;

            // Manually fire the aura at the player position — runs the SAME code path the real game uses,
            // but doesn't depend on a weapon prefab being equipped.
            var ctx = new FireContext
            {
                FirePoint                  = _cachedPlayerTransform,
                AimDirection               = _cachedPlayerTransform.forward,
                WeaponData                 = _benchmarkAuraWeaponData,
                Level                      = 1,
                LevelDamageBonus           = 0f,
                OverchargeDamageMultiplier = 1f,
                DamageMultiplier           = 1f,
                CritChance                 = 0f,
                AreaMultiplier             = 1f,
            };
            auraToToggle.Execute(ctx);
        }

        // ─── Actions ────────────────────────────────────────────────

        public void SpawnBatch()
        {
            if (spawner == null || benchmarkEnemy == null)
            {
                Debug.LogWarning("[Benchmark] spawner or benchmarkEnemy not assigned");
                return;
            }
            for (int i = 0; i < spawnBatchCount; i++)
                spawner.Spawn(benchmarkEnemy, spawnDifficultyMult);
            Debug.Log($"[Benchmark] Spawned {spawnBatchCount} enemies. Total now ~{ActiveCount()}");
        }

        public void ClearEnemies()
        {
            int killed = 0;
            foreach (var enemy in Object.FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(float.MaxValue);
                    killed++;
                }
            }
            Debug.Log($"[Benchmark] Killed {killed} enemies");
        }

        public void ToggleAuraMethod()
        {
            if (auraToToggle == null) { Debug.LogWarning("[Benchmark] auraToToggle not assigned"); return; }
            auraToToggle.detection = auraToToggle.detection == AuraStrategySO.DetectionMethod.ActiveEnemyRegistry
                ? AuraStrategySO.DetectionMethod.PhysicsOverlapSphereNonAlloc
                : AuraStrategySO.DetectionMethod.ActiveEnemyRegistry;
            Debug.Log($"[Benchmark] Aura detection → {auraToToggle.detection}");
        }

        public void ToggleProjectileSpawnMethod()
        {
            if (rapidFireToToggle == null) { Debug.LogWarning("[Benchmark] rapidFireToToggle not assigned"); return; }
            rapidFireToToggle.spawnMethod = rapidFireToToggle.spawnMethod == RapidFireStrategySO.SpawnMethod.Pooled
                ? RapidFireStrategySO.SpawnMethod.Instantiate
                : RapidFireStrategySO.SpawnMethod.Pooled;
            Debug.Log($"[Benchmark] Projectile spawn → {rapidFireToToggle.spawnMethod}");
        }

        public void ToggleFindMethod()
        {
            if (playerToToggle == null) { Debug.LogWarning("[Benchmark] playerToToggle not assigned"); return; }
            playerToToggle.useFindWithTagBenchmark = !playerToToggle.useFindWithTagBenchmark;
            Debug.Log($"[Benchmark] Player nearest-enemy method → {(playerToToggle.useFindWithTagBenchmark ? "FindGameObjectsWithTag" : "ActiveEnemyRegistry")}");
        }

        public void ToggleInvincibilityOnly()
        {
            invincibilityOnly = !invincibilityOnly;
            if (invincibilityOnly && benchmarkMode) ExitBenchmarkMode();
            CachePlayerRefs();
            Debug.Log($"[Benchmark] Invincibility-only mode → {invincibilityOnly}");
        }

        private void TickInvincibilityOnly()
        {
            CachePlayerRefs();
            if (_cachedPlayerHealth != null) _cachedPlayerHealth.GrantIFrames(99999f);
        }

        // ─── Rapid burst (drives projectile pooling benchmark) ──────────────────

        public void ToggleRapidBurst()
        {
            rapidBurst = !rapidBurst;
            CachePlayerRefs();
            CacheRapidFireWeaponData();
            if (rapidBurst && _cachedRapidFireWeaponData == null)
            {
                Debug.LogWarning("[Benchmark] Rapid burst ON but no WeaponDataSO uses the active RapidFireStrategy — fix that or assign rapidFireToToggle.");
            }
            Debug.Log($"[Benchmark] Rapid burst → {rapidBurst} ({rapidBurstShotsPerFrame} shots/frame)");
        }

        private void CacheRapidFireWeaponData()
        {
            if (_cachedRapidFireWeaponData != null || rapidFireToToggle == null) return;
            // Find any WeaponDataSO whose fireStrategy is the same RapidFireStrategySO we're toggling.
            // That way Execute() has a real projectilePrefab + range/speed config to work with.
            foreach (var wd in Resources.FindObjectsOfTypeAll<WeaponDataSO>())
            {
                if (wd != null && wd.fireStrategy == rapidFireToToggle && wd.projectilePrefab != null)
                { _cachedRapidFireWeaponData = wd; break; }
            }
            if (_cachedRapidFireWeaponData == null)
            {
                // Fallback — first WeaponDataSO that has any projectile prefab assigned
                foreach (var wd in Resources.FindObjectsOfTypeAll<WeaponDataSO>())
                {
                    if (wd != null && wd.projectilePrefab != null)
                    { _cachedRapidFireWeaponData = wd; break; }
                }
            }
        }

        private void TickRapidBurst()
        {
            if (rapidFireToToggle == null || _cachedPlayerTransform == null || _cachedRapidFireWeaponData == null) return;

            var ctx = new FireContext
            {
                FirePoint                  = _cachedPlayerTransform,
                AimDirection               = _cachedPlayerTransform.forward,
                WeaponData                 = _cachedRapidFireWeaponData,
                Level                      = 1,
                LevelDamageBonus           = 0f,
                OverchargeDamageMultiplier = 1f,
                DamageMultiplier           = 1f,
                CritChance                 = 0f,
                AreaMultiplier             = 1f,
            };
            for (int i = 0; i < rapidBurstShotsPerFrame; i++)
                rapidFireToToggle.Execute(ctx);
        }

        public void ResetSamples()
        {
            _writeIdx   = 0;
            _validCount = 0;
            Debug.Log("[Benchmark] Frame samples cleared");
        }

        public void DumpCsv()
        {
            if (_validCount == 0) { Debug.LogWarning("[Benchmark] No samples to dump"); return; }

            // Filename encodes ALL three toggles so you can tell from the name which run produced it
            string aura = auraToToggle != null
                ? (auraToToggle.detection == AuraStrategySO.DetectionMethod.ActiveEnemyRegistry ? "auraReg" : "auraPhys")
                : "auraNA";
            string proj = rapidFireToToggle != null
                ? (rapidFireToToggle.spawnMethod == RapidFireStrategySO.SpawnMethod.Pooled ? "projPool" : "projInst")
                : "projNA";
            string find = playerToToggle != null
                ? (playerToToggle.useFindWithTagBenchmark ? "findTag" : "findReg")
                : "findNA";

            int    enemyCount = ActiveCount();
            string ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = Path.Combine(Application.persistentDataPath,
                $"benchmark_{aura}_{proj}_{find}_n{enemyCount}_{ts}.csv");

            var sb = new StringBuilder();
            sb.AppendLine("frame_index,frame_ms,fps");
            // Walk samples oldest → newest
            int start = _validCount < _frameMs.Length ? 0 : _writeIdx;
            for (int i = 0; i < _validCount; i++)
            {
                int idx = (start + i) % _frameMs.Length;
                float frameMs = _frameMs[idx];
                float fps = frameMs > 0f ? 1000f / frameMs : 0f;
                sb.Append(i).Append(',').Append(frameMs.ToString("F3")).Append(',').AppendLine(fps.ToString("F1"));
            }

            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[Benchmark] Wrote {_validCount} samples to {path}\n  Aura={aura}  Projectile={proj}  Find={find}  Enemies={enemyCount}  AvgMs={Average():F2}  FPS={(1000f/Average()):F0}");
        }

        // ─── Helpers ────────────────────────────────────────────────

        private int ActiveCount()
        {
            int count = 0;
            foreach (var e in Object.FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (e != null && !e.IsDead) count++;
            return count;
        }

        private float Average()
        {
            if (_validCount == 0) return 0f;
            float sum = 0f;
            for (int i = 0; i < _validCount; i++) sum += _frameMs[i];
            return sum / _validCount;
        }

        private void OnGUI()
        {
            if (!showOverlay) return;
            string method = auraToToggle != null ? auraToToggle.detection.ToString() : "n/a";
            float avg = Average();
            string modeTag = benchmarkMode      ? "<color=#7CFC7C>FULL MODE (F5): ON</color>"
                            : invincibilityOnly ? "<color=#7CFC7C>INVINCIBLE-ONLY (F8): ON</color>"
                            :                     "<color=#FFD27A>NORMAL PLAY</color>";
            string projMethod = rapidFireToToggle != null ? rapidFireToToggle.spawnMethod.ToString() : "n/a";
            string findMethod = playerToToggle    != null ? (playerToToggle.useFindWithTagBenchmark ? "FindWithTag" : "Registry") : "n/a";
            string burstTag = rapidBurst ? $"  <color=#FF8888>BURST {rapidBurstShotsPerFrame}/frame</color>" : "";
            string text = $"<b>BENCHMARK</b>\n" +
                          $"{modeTag}{burstTag}\n" +
                          $"Enemies: {ActiveCount()}\n" +
                          $"Aura: {method}\n" +
                          $"Projectile: {projMethod}\n" +
                          $"Nearest-enemy: {findMethod}\n" +
                          $"Avg frame: {avg:F2} ms ({(avg > 0f ? 1000f/avg : 0f):F0} FPS)\n" +
                          $"Samples: {_validCount}/{_frameMs.Length}\n" +
                          $"\n" +
                          $"<size=11>F1 spawn  F2 clear  F3 aura  F4 reset\n" +
                          $"F5 full mode  F6 projectile  F7 nearest\n" +
                          $"F8 invincible-only  F9 rapid burst  F12 dump CSV</size>";

            var style = new GUIStyle(GUI.skin.box)
            {
                alignment   = TextAnchor.UpperLeft,
                richText    = true,
                fontSize    = 13,
                padding     = new RectOffset(10, 10, 8, 8),
                normal      = { textColor = Color.white }
            };
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.Box(new Rect(8, 8, 400, 250), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(8, 8, 400, 250), text, style);
        }
    }
}

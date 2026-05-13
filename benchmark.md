# SwarmProtocol — Benchmark Guide

Complete protocols for the three Profiler experiments backing slides 5a (Metrics & Method) and 5b (Interaction Figure) of the CMPE 485 presentation.

All three benchmarks use the in-scene `BenchmarkRunner` GameObject. Press Play, click **Start**, then use the hotkeys below.

---

## Hotkey reference

| Key | Action |
|---|---|
| **F1** | Spawn 50 enemies via `EnemySpawner` |
| **F2** | Kill all active enemies |
| **F3** | Toggle aura detection method (Registry ↔ Physics) |
| **F4** | Reset frame-time samples (clear the rolling buffer) |
| **F5** | Toggle FULL BENCHMARK MODE (invincible + weapons OFF + auto-fires aura) — for aura/find tests |
| **F6** | Toggle projectile spawn method (Pooled ↔ Instantiate) |
| **F7** | Toggle player nearest-enemy method (Registry ↔ FindGameObjectsWithTag) |
| **F8** | Toggle INVINCIBILITY-ONLY (player can't die, weapons still fire) — for projectile tests |
| **F9** | Toggle RAPID BURST (BenchmarkRunner directly fires RapidFireStrategy 30×/frame) — drives pooling test |
| **F12** | Dump last ~10s of frame samples to CSV |

> **F5 vs F8** — F5 is the *full* benchmark setup (invincible + weapons off + aura auto-fires). F8 only grants invincibility; weapons keep firing normally. Use F8 when the thing you're measuring **needs the player's weapons to be active** (the projectile pooling benchmark).

CSVs land at: `~/Library/Application Support/DefaultCompany/SwarmProtocol/`

The on-screen overlay shows live state for all three toggles + current frame stats.

---

## Opening the Profiler

1. **Window → Analysis → Profiler** (or press **Ctrl/Cmd + 7**)
2. In the toolbar, enable **Record** (●) before pressing Play
3. Useful tabs:
   - **CPU Usage** — main timeline, shows ms per frame + named markers
   - **Memory** — for GC alloc tracking
4. **Hierarchy** view (bottom of CPU Usage tab) — search box filters by marker name

Disable **Deep Profile** for these benchmarks. It changes timings significantly and is meant for finding hotspots, not measuring them.

---

# Benchmark #1 — Object Pooling vs Instantiate / Destroy

**Story:** *We compared pooled projectile spawning against the textbook `Instantiate`/`Destroy` baseline at the same fire rate. The pooled path eliminates GC allocation per shot.*

**What's being measured:** CPU cost + GC alloc per projectile spawn.

## Setup

- **F8 ON** — invincibility-only. Stage spawners will keep dropping enemies on you; F8 keeps you alive without disabling weapons.
- **F9 ON** — rapid burst. BenchmarkRunner directly fires the RapidFireStrategy **30 times per frame** (~1800 spawns/sec) so the difference between Pooled and Instantiate is dramatic, not buried in noise.
- Pile-up of projectiles + enemies is fine — projectile spawn cost is what's measured.

> **Why F9?** PulseRifle's natural fire rate (~5 shots/sec) only produces ~50 spawns in a 10s window. That's invisible against the rest of the frame. F9 drives 1800 shots/sec — guarantees a measurable Instantiate cost.

## Protocol

```
1. Press Play → click Start
2. F8  (overlay shows "INVINCIBLE-ONLY (F8): ON")
3. F9  (overlay shows "BURST 30/frame")
4. Confirm overlay reads:
     Projectile: Pooled
     INVINCIBLE-ONLY (F8): ON  BURST 30/frame
5. Wait 5s for warm-up (pools to populate, JIT to settle)
6. F4   (reset frame samples)
7. Wait 10s
8. F12  → file: benchmark_..._projPool_..._*.csv

9. F6   → overlay now reads "Projectile: Instantiate"
10. F4  (reset)
11. Wait 10s
12. F12 → file: benchmark_..._projInst_..._*.csv
```

The CSV filename now encodes all three method states (`auraReg_projPool_findReg` etc.) so you can tell at a glance which run produced each file.

## What to check in Profiler

While step 9 is running, switch to the Profiler window:

### CPU Usage tab — Timeline
- Look for **frame spikes**. With Instantiate/Destroy, you'll see jagged peaks every few frames (Destroy triggers GC).
- Pooled path is a flat line.
- Screenshot a 5-second timeline window of each method side-by-side.

### CPU Usage tab — Hierarchy view
- Type `RapidFire.Spawn_` into the search box
- You'll see one of:
  - `RapidFire.Spawn_Pooled` — should read tiny (microseconds)
  - `RapidFire.Spawn_Instantiate` — much higher, plus a `GC Alloc` column showing bytes per call
- Note the **Time ms** and **GC Alloc** columns for each method

### Memory tab — GC Allocated In Frame
- This is the **smoking gun** for pooling.
- Pooled: ~0 KB / frame
- Instantiate: kilobytes / frame, plus periodic GC.Collect spikes

## What to put on the slide

- **Bar chart**: avg ms/frame, Pooled vs Instantiate
- **Bonus stat**: "GC alloc dropped from X KB/frame to Y KB/frame"
- **Profiler screenshot**: side-by-side CPU timeline showing flat (pooled) vs spikes (instantiate)

---

# Benchmark #2 — Nearest-Enemy: Registry vs FindGameObjectsWithTag

**Story:** *Player auto-aim picks the nearest enemy 10×/sec. We compared a centralized registry (cached list, sqrMagnitude) against `FindGameObjectsWithTag` (allocates a new array every call).*

**What's being measured:** per-call cost of the nearest-enemy lookup + GC pressure from array allocation.

## Setup

- **F5 ON** — player invincible + weapons disabled so enemies don't die during the run
- **F1 ×4** — spawn 200 enemies so the lookup has real work to do

## Protocol

```
1. Press Play → Start
2. F1 F1 F1 F1   (≈200 enemies; check overlay)
3. F5            ("BENCHMARK MODE: ON" — enemies stop dying, player can't die)
4. Confirm overlay reads:
     Enemies: ~200
     Nearest-enemy: Registry
     BENCHMARK MODE: ON
5. Wait 10s for warm-up
6. F4 (reset samples)
7. Wait 10s
8. F12 → rename: registry_n200.csv

9. F7  → overlay now reads "Nearest-enemy: FindWithTag"
10. F4 (reset)
11. Wait 10s
12. F12 → rename: findwithtag_n200.csv
```

## What to check in Profiler

### CPU Usage tab — Hierarchy view
Search for `PlayerController.FindNearest_`. You'll see ONE of these markers depending on which method is active:
- `PlayerController.FindNearest_Registry` — typically **~5–20 µs/call** (microseconds), **0 B GC alloc**
- `PlayerController.FindNearest_FindWithTag` — typically **50–200 µs/call**, plus several **hundred bytes of GC alloc per call** (the new GameObject array)

### Hierarchy → Calls + Total Time
- Each method runs 10×/sec (throttled in `PlayerController.Update`)
- So 600 sample frames ≈ 100 calls — the marker's "Total" column accumulates that
- Note **Avg per call** and **GC Alloc per call** columns for both methods

### Memory tab
- FindWithTag run: GC Alloc graph shows steady allocation (~5–10 KB/sec at 200 enemies)
- Registry run: flat at 0

## What to put on the slide

- **Bar chart or bullet list** of per-call cost: Registry **X µs** vs FindWithTag **Y µs** → `Y/X` × speedup
- **GC stat**: "FindWithTag allocates ~Z bytes per call; Registry allocates 0"
- Justifies the architectural decision (CLAUDE.md note: "ActiveEnemyRegistry replaces FindGameObjectsWithTag")

---

# Benchmark #3 — Aura Detection: Registry vs Physics

*(Already run — keeping here for reference / re-runs.)*

**Story:** *Aura weapons damage all enemies in a radius every fire tick. We compared an enemy-registry sqrMagnitude scan against `Physics.OverlapSphereNonAlloc`.*

**What's being measured:** per-fire detection cost, scaled across enemy counts.

## Setup

- **F5 ON** — player invincible, weapons off, BenchmarkRunner directly fires the aura strategy 10×/sec
- **F1** repeatedly to ramp enemy count

## Protocol

```
1. Press Play → Start
2. F5 (BENCHMARK MODE ON)
3. Wait 5s for warm-up

# Registry sweep:
4. F1 (50)   → wait 10s → F4 → wait 10s → F12 → registry_n50.csv
5. F1 (100)  → wait 10s → F4 → wait 10s → F12 → registry_n100.csv
6. F1 F1 (200) → wait 10s → F4 → wait 10s → F12 → registry_n200.csv
7. F1 ×6 (500) → wait 10s → F4 → wait 10s → F12 → registry_n500.csv

# Switch method:
8. F2 (clear all enemies)
9. F3 (overlay now reads "Aura: PhysicsOverlapSphereNonAlloc")

# Physics sweep — repeat the spawn ladder:
10. F1 (50)   → wait 10s → F4 → wait 10s → F12 → physics_n50.csv
11. ... repeat at 100, 200, 500
```

## What to check in Profiler

### Hierarchy view
Search for `Aura.`. Three relevant markers:
- `Aura.Execute` — total cost per fire tick (wraps both detection and damage application)
- `Aura.RegistryQuery` — only active when method = Registry
- `Aura.PhysicsOverlap` — only active when method = Physics
- `Aura.ApplyDamage` — same in both modes (damage application)

Compare `Aura.RegistryQuery` vs `Aura.PhysicsOverlap` directly — that's the apples-to-apples comparison of the two algorithms in isolation.

### What you'll see in our existing data
- Frame-time difference is small (5–11% at typical N, crossover at ~500)
- Per-marker comparison (Profiler) is much sharper

## What to put on the slide
- The PNG chart at `benchmark_chart.png` — frame-time interaction figure
- Honest framing: "Both methods scale linearly. Registry wins at typical play counts; both maintain 60 FPS up to ~250 enemies."

---

# Aggregating results across all three benchmarks

After all CSVs are dumped, run:

```bash
python3 - <<'PY'
import csv, glob, os, re
from collections import defaultdict

# Adapt this glob if your CSVs are in a different folder
files = sorted(glob.glob("/Users/alperen/Downloads/SwarmProtocol/benchmark_*.csv"))
for f in files:
    name = os.path.basename(f)
    with open(f) as fp:
        next(fp)
        ms = [float(line.split(",")[1]) for line in fp if line.strip()]
    avg = sum(ms) / len(ms)
    print(f"{name:<70} avg={avg:6.2f} ms ({1000/avg:5.1f} FPS, {len(ms)} samples)")
PY
```

This prints a one-line summary for every CSV file you've dumped — useful for a quick sanity check before building charts.

---

# Common pitfalls & answers

| Issue | Fix |
|---|---|
| "Benchmark mode auto-fires aura, polluting #2" | The Profiler markers `FindNearest_Registry`/`_FindWithTag` measure *only* the lookup. Aura cost shows under separate markers and doesn't contaminate the comparison. |
| "Player dies before I capture" | F5 (full benchmark mode) for aura/find tests; F8 (invincibility-only) for projectile tests where you need weapons firing. |
| "Enemy count drifts during the run" | Either F5 is off (your weapons are killing them) or contact damage is happening. Use F5 + don't move. |
| "Frame ms is dominated by NavMesh / rendering, not the thing I'm measuring" | Use the Profiler **Hierarchy** view + named markers instead of the CSV's frame ms. Microsecond-level marker times isolate the algorithm. |
| "GC Alloc column in Profiler is empty" | Click the column header `GC Alloc` to enable it; toggle **Allocation Callstacks** in the Profiler toolbar to see what's allocating |
| "Numbers are very different in Editor vs Build" | They will be — Editor adds 30–50% overhead. Mention this in the slide ("Editor measurements; build perf is better"). |

---

# Recommended slide assignments

| Slide | Source |
|---|---|
| **5a — Metrics & Method** | Profiler Hierarchy screenshot showing `Aura.RegistryQuery` + `PlayerController.FindNearest_Registry` markers with their µs / GC numbers visible |
| **5b — Interaction Figure** | One of: `benchmark_chart.png` (aura sweep) **or** a new pooling bar chart (#1) **or** a nearest-enemy bar chart (#2). The pooling chart is the most dramatic story. |

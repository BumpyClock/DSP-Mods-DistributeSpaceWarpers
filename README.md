Simple mod for Dyson Sphere Program to distribute Space Warpers from a local ILS or PLS without using up a slot. 

To use simply have one ILS/PLS set to local supply Warpers, all other ILS on the same planet will get Wapers automatically. 

To install use ThunderStore Mod manager https://thunderstore.io/c/dyson-sphere-program/p/BumpyClock/DistributeSpaceWarpers/

I'm pretty new to building mods so expect bugs. If you find an incompatability or a bug create an issue. 

I'm not responsible for borking your saved game. Use at your own risk. 

**Engine Overview**
- Fair, low-overhead distribution with a dedicated engine:
  - Snapshot → Plan → Execute → Refresh each tick.
  - Most‑needy stations first; optional proportional fair‑share across all receivers.
  - Local-first transfers to minimize costs; remote when enabled.
  - Bounded per‑run work via per‑receiver caps and supplier rotation.

**Key Config** (BepInEx config)
- `General.WarperTickCount` – ticks between runs (default 60).
- `General.WarperRemoteMode` – allow cross‑planet supply.
- `General.WarperTransportCost` / `WarperLocalTransportCost` / `WarperRemoteTransportCost` – cost model.
- `Advanced.WarperTarget` – target warpers per receiver (default 50).
- `Advanced.FairShareDistribution` – proportional fair share (default true).
- `Advanced.MaxPerTickPerReceiver` – per‑run cap (default 10, 0 = unlimited).
- `Advanced.SupplierReserve` – minimum stock to keep at suppliers (default 0).

**Scheduling**
- Runs on true game ticks inside `GalacticTransport.GameTick` (not frame rate).
- Cadence is `WarperTickCount` (min 1). Adaptive cadence may be added later.

**Files of Interest**
- `DistributeSpaceWarper/DistributionEngine.*.cs` – engine (partial class split):
  - `Core` (orchestration), `Snapshot` (scanning), `Plan` (ordering/targets),
    `Execute` (transfers), `Refresh` (traffic batching).
- `DistributeSpaceWarper/Patch.cs` – schedules the engine each tick.
- `DistributeSpaceWarper/Config.cs` – configuration bindings.

**Build**
- Build with Visual Studio or `dotnet build DSP-Mods.sln -c Release`.
- Output is `Release/DistributeSpaceWarper.dll` for packing.

**Contributing**
- Keep hot paths allocation‑free:
  - No LINQ in per‑tick logic. Prefer indexed `for` loops and reusable buffers.
  - Avoid closures and `ToList()` in the engine; profile before adding abstractions.
- Maintain engine structure:
  - Distribution logic lives in `DistributionEngine` partials (Core/Snapshot/Plan/Execute/Refresh).
  - `Patch.cs` remains a thin scheduler only.
- Traffic updates:
  - Batch `UpdateNeeds` and `Refresh*Traffic` calls; dedupe by planet/gid.
- Config changes:
  - Add new keys under `Config.Advanced` or `Config.General`, document defaults/ranges, and update README.
  - Keep sensible defaults; avoid breaking behavior for existing users.
- Logging:
  - Use `ModDebug.Log/Error/Trace`; default to quiet logs unless troubleshooting.
- Style:
  - Prefer explicit names, minimal visibility, and small, focused methods.

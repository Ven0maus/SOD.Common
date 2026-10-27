# AGENTS.md — SOD.Common Agent Instructions

AI coding agents working on this repository must follow these rules. They prevent regressions, preserve downstream mod compatibility, and keep the library aligned with the Shadows of Doubt modding ecosystem.

---

## Project Overview

**SOD.Common** is a shared BepInEx Harmony library for *Shadows of Doubt* (IL2CPP). It provides:

- Configuration bindings with `.BindingAttribute` (interface-driven, TOML-based).
- A `Lib` singleton exposing helper namespaces: `GameMessage`, `Time`, `SaveGame`, `InputDetection`, `Interaction`, `SyncDisks`, `DdsStrings`, `Dialogs`, `PluginDetection`, `PlayerStatus`, `RestoredEngineFunctions`, `Gameplay`, `CaseObjectives`.
- Harmony patches that intercept game logic (murder events, save/load, input, dialogs, upgrades, etc.).
- IL2CPP interop helpers (Il2Cpp list wrappers, JSON converters, restored Unity functions).
- A build tool (`Builder/`) that packages mods into r2modman zip files.

**Consumers:** Downstream mods in `Mods/` that reference `SOD.Common.csproj` and depend on `Venomaus.SOD.Common` via `[BepInDependency]`.

---

## Repository Structure

```
SOD.Common.sln          — Library + Builder solution
SODMods.sln             — Full solution: library + all 8 example/production mods
SOD.Common/             — Core library source
  Plugin.cs              — BepInPlugin entry point, initializes UniverseLib + Harmony.PatchAll()
  Lib.cs                 — Static singleton aggregator for all helper classes
  IPluginBindings.cs     — Base bindings interface (DebugMode)
  BepInEx/               — PluginController<T>, config proxy system, BindingAttribute
  Extensions/            — GameExtensions, EnumerableExtensions, LogExtensions, etc.
  Helpers/               — Public-facing helper classes (GameMessage, Time, SaveGame, etc.)
  Patches/               — Harmony patches targeting game types
  Custom/                — MersenneTwister, JsonConverters, InteractableInstanceData
  RestoredUnityEngine/   — JsonUtility reimplementation (stripped from game)
  ModFolderContent/      — Thunderstore manifest + icon for library distribution
  UniverseLib.IL2CPP.Interop.dll — Bundled interop DLL
Builder/                 — CLI tool for building r2modman packages (reads manifest.json)
Mods/                    — Example/production mods consuming SOD.Common
  SOD.QoL/
  SOD.StockMarket/       — Only mod with x64 platform + Bogus dependency + AssetBundles
  SOD.LifeAndLiving/
  SOD.LethalActionReborn/
  SOD.RelationsPlus/
  SOD.ThreatOverhaul/
  SOD.MailCourier/
libs/il2cpp/             — IL2CPP game assemblies + bundled third-party DLLs (NOT committed to git)
  .gitignore             — Ignores everything except .gitignore and *.md
```

---

## Build and Validation Commands

**Verified commands (executed on this machine):**

```bash
# Build the library + Builder only
dotnet build SOD.Common.sln --verbosity minimal

# Build the library + all downstream mods
dotnet build SODMods.sln --verbosity minimal
```

Both succeeded with only `NETSDK1138` warnings (net6.0 EOL). No errors.

**Build notes:**

- Target framework: `net6.0` (all projects). Do not change this without explicit justification.
- `libs/il2cpp/` assemblies must exist locally (from r2modman interop folder). They are not in git.
- The `Builder/` tool is a console app. Run it from its directory: `dotnet run --project Builder\Builder.csproj`.
- `SOD.StockMarket` uses `x64` platform; other mods use `AnyCPU`.
- No test projects exist. No test runner is configured.

**Unverified:** Runtime behavior in-game. Compilation does not validate IL2CPP runtime compatibility.

---

## Coding Conventions

**Inferred from source inspection. Enforced by .csproj/config where noted.**

| Rule | Source |
|------|--------|
| `Nullable` is **disabled** (`<Nullable>disable</Nullable>`) | All `.csproj` files |
| `ImplicitUsings` is **disabled** | All `.csproj` files |
| `LangVersion` is `latest` | SOD.Common.csproj |
| No `.editorconfig` formatting rules (only `CA1826 = silent`) | `.editorconfig` |
| Namespace: `SOD.Common` for library, `SOD.<ModName>` for mods | All source files |
| Helper classes are `public sealed` with `internal` constructors | `Lib.cs`, all Helpers/ files |
| Extensions are `public static` classes | Extensions/ directory |
| Patches are `internal` classes with nested patch classes | Patches/ directory |
| XML documentation on all public members | Throughout source |
| Config identifiers: `Section.PropertyName` format | `BindingAttribute`, `ConfigHelper.cs` |
| Config interface inheritance for grouping settings | `IPluginBindings.cs` in Mods/ |
| `Harmony.PatchAll(Assembly.GetExecutingAssembly())` in `Load()` | Plugin.cs pattern |
| Use `Plugin.Log.LogInfo()` / `Plugin.Log.LogWarning()` / `Plugin.Log.LogError()` | Throughout |
| Debug logging gated by `Plugin.InDebugMode` | Helpers, Extensions |
| `CopyLocalLockFileAssemblies = true` on all projects | All `.csproj` files |

**Brace style:** Opening brace on same line (K&R). 4-space indentation (inferred from source).

---

## Game and Modding Integration

**Runtime constraints agents must respect:**

1. **BepInEx initialization:** Plugins load via BepInEx `BasePlugin` lifecycle. `PluginController<T>` extends `BasePlugin`. Override `Load()` for setup. Override `Unload()` for cleanup.

2. **Harmony patches:** All patches registered via `Harmony.PatchAll()` in `Plugin.Load()`. Patches target game types (e.g., `MurderController`, `SaveStateController`, `DialogController`). Use `[HarmonyPrefix]`, `[HarmonyPostfix]`, `[HarmonyTranspiler]` as needed.

3. **IL2CPP interop:** Game types are accessed via il2cpp-generated wrappers. Use `UniverseLib` for reflection helpers. Never assume .NET types map directly to game types.

4. **Unity main thread:** Unity object operations must run on the main thread. Use `UniverseLib.RuntimeHelper.StartCoroutine()` for async operations (see `GameMessage.ShowPlayerSpeech()`).

5. **Object lifetime:** Destroyed Unity objects must be checked with `IsNullOrDestroyed()`. IL2CPP objects can become invalid between frames.

6. **Patch compatibility:** Patches must not break other mods. Use `__state` for prefix→postfix data passing. Guard against null instances. Check `PluginDetection` for optional dependency presence before using types from other mods.

7. **Save/load lifecycle:** `SaveGame` helper fires events: `OnBeforeSave`, `OnAfterSave`, `OnBeforeLoad`, `OnAfterLoad`, `OnBeforeDelete`, `OnAfterDelete`, `OnBeforeNewGame`, `OnAfterNewGame`. Use these for mod data persistence, not raw file I/O.

8. **Save data paths:** Use `Lib.SaveGame.GetSaveGameDataPath()` for save-specific data. Use `Lib.SaveGame.GetPluginDataPath()` for plugin-wide data. Never write to arbitrary paths.

9. **Time system:** `Lib.Time` is not available until `OnTimeInitialized` fires. Check `Lib.Time.IsInitialized` before accessing time data.

10. **Input detection:** Rewired-based. `InputDetection` raises `OnButtonStateChanged`. Patches in `InputControllerPatches.cs` feed events.

---

## Public API and Compatibility

**SOD.Common is a NuGet-published library consumed by downstream mods. Breaking changes have real consequences.**

Agents must:

- **Preserve existing public types, methods, and signatures** unless a breaking change is explicitly required.
- **Never rename public classes, namespaces, or method signatures** without updating all callers and documenting the change.
- **Inspect all callers** before modifying shared functionality. Check `Mods/` projects and consider Thunderstore dependencies.
- **Avoid adding new dependencies** to the library project unless they are genuinely shared across multiple mods.
- **Maintain binary compatibility** where possible. The library is distributed as a DLL + NuGet package.
- **Update `ModFolderContent/manifest.json` version** only when releasing a new library version.

**Public API surface (key types agents should not break):**

- `SOD.Common.Lib` (static singleton)
- `SOD.Common.Helpers.*` (all helper classes)
- `SOD.Common.Extensions.*` (all extension classes)
- `SOD.Common.BepInEx.PluginController<T, TBindings>` (base class for mod plugins)
- `SOD.Common.BepInEx.Configuration.BindingAttribute` (config binding attribute)
- `SOD.Common.Patches.*` (internal, but changing these can affect game behavior)

---

## Dependencies and Bundled Assemblies

**NuGet packages (from `.csproj`):**

| Package | Version | Scope |
|---------|---------|-------|
| `BepInEx.Unity.IL2CPP` | `6.0.0-be.*` | compile only |
| `Castle.Core` | `5.1.1` | compile (Moq dependency for config proxy) |
| `Newtonsoft.Json` | `13.0.3` | Builder project only |
| `Bogus` | `35.2.0` | SOD.StockMarket only |

**Game assemblies (project references, not NuGet):**

- `Assembly-CSharp.dll` — Main game assembly (IL2CPP)
- `Il2Cppmscorlib.dll`, `Il2CppSystem.Core.dll` — IL2CPP runtime
- `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.UI.dll`, `UnityEngine.PhysicsModule.dll`, `UnityEngine.ImageConversionModule.dll`, `UnityEngine.InputLegacyModule.dll` — Unity modules
- `Unity.TextMeshPro.dll` — TextMeshPro
- `Rewired_Core.dll` — Input framework
- `FMODUnity.dll` — Audio (used by StockMarket mod)
- `AssetBundleLoader.dll` — piepie's mod (used by StockMarket)
- `DialogUIRework.dll` — piepie's mod (not referenced by SOD.Common directly)

**Bundled DLLs:**

- `UniverseLib.IL2CPP.Interop.dll` — Copied to output, referenced by both library and mods. Provides IL2CPP interop helpers.

**Critical rules:**

- **Never commit generated binaries** (`bin/`, `obj/`). The `.gitignore` excludes them.
- **`libs/il2cpp/` is git-ignored** (everything except `.gitignore` and `*.md`). Do not add game DLLs to git. Agents downloading this repo must populate `libs/il2cpp/` from their r2modman interop folder.
- **Do not upgrade `BepInEx.Unity.IL2CPP`** without verifying compatibility with the target game version.
- **Do not remove or replace game assembly references** — they define the types the library patches and extends.

---

## Testing and Verification

**No automated tests exist. Verification is manual and compile-time only.**

Agents must follow this workflow:

1. **Inspect the relevant implementation and its callers.** Check `Patches/`, `Helpers/`, `Extensions/`, and dependent `Mods/` projects.
2. **Build the narrowest affected project first.** E.g., `dotnet build SOD.Common\SOD.Common.csproj`.
3. **Build the relevant solution.** E.g., `dotnet build SODMods.sln` to verify downstream mod compatibility.
4. **Review the final diff.** Check for:
   - Unrelated formatting changes
   - Generated files (`bin/`, `obj/`)
   - Accidental dependency changes
   - Public API surface changes
   - Breaking changes to downstream mods
5. **Report any checks that could not run.** If a build fails due to missing `libs/il2cpp/` assemblies, document this.
6. **Runtime validation requires the game.** A successful compile does not guarantee in-game correctness. Note any unverified runtime assumptions.

---

## Change-Management Rules

1. **Understand before editing.** Read the existing implementation, patches, and call sites. Know what a change affects.
2. **No unrelated refactoring.** Do not rename, restructure, or "improve" code that is not directly tied to the task.
3. **No speculative abstractions.** Do not add interfaces, base classes, or patterns that are not needed by the current task.
4. **Follow established patterns.** Use `PluginController<T, TBindings>` for new mod plugins. Use `BindingAttribute` for config. Use `HarmonyPatch` for patches.
5. **Preserve user changes.** Do not overwrite unsaved user work.
6. **No destructive Git operations** without explicit authorization.
7. **No version/metadata changes** unless required. The library version lives in `SOD.Common.csproj` and `ModFolderContent/manifest.json`.
8. **Do not claim a feature works because it compiles.** IL2CPP runtime behavior is unverified at compile time.

---

## Documentation and Examples

- **XML documentation:** All public members have `<summary>` tags. Add or update them when changing public APIs.
- **Wiki:** The GitHub wiki at `https://github.com/Ven0maus/SOD.Common/wiki` documents public features. Update it when adding new public functionality.
- **Examples in code:** Use real, existing APIs. Reference actual types (`Lib.GameMessage`, `Lib.Time`, etc.) and valid signatures.
- **README:** `README.md` describes Thunderstore dependency setup and NuGet package. Update if the dependency string or versioning changes.

---

## Completion Criteria

When finishing a task, agents must report:

1. **Summary of changes** — what was modified and why.
2. **Important implementation decisions** — architectural choices made during the task.
3. **Projects built** — which `.csproj` or `.sln` was built and the result.
4. **Tests/checks executed** — which checks ran, which could not run, and why.
5. **Results and failures** — build output, warnings, errors.
6. **Unverified runtime assumptions** — anything that compiles but was not tested in-game.
7. **Public API / compatibility impact** — whether downstream mods are affected and how.

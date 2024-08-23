# CHANGELOG
**2.0.4**
- Added extra helpers to the plugin detection lib
- Added extra extension to TryCast for il2cpp types.

**2.0.3**
- Added Gameplay Lib helper which provides functionality related to the detective gameplay
- Added CaseObjectives Lib helper which provides functionality to create and hook new objectives to cases
- Added extra extensions for il2cpp to enumerable support

**2.0.2**
- Bugfix option ids were not passed through properly for Upgrade Options on sync disk builder
- Code cleanup, logging and performance improvements to sync disk helper
- Added GameExtensions class, which will contain helper extensions for the game, such as "Actor.Sees(actor);"
- Added GetUniqueNumber variant to SaveGame helper (similar to GetUniqueString but uint version)
- Added GetPluginDirectoryPath to SaveGame helper
- Optimized InputController_Update patch input detection handling

**2.0.1**
- Adjusted GetSavestoreDirectoryPath method overrides. (No more missing method exception when not providing filename parameter)
- Added extension class "GameExtensions" in SOD.Common.Extensions
- Added CanSee extension in "GameExtensions" for Actor object

**2.0.0**
- Fix missing method exceptions.

**1.1.9**
- Added extra operators (<, >, <=, >=) to TimeData struct in Lib.Time
- Added extra AddHours method to TimeData struct in Lib.Time
- Fixed parsing issue with GetFnvHashCode extension
- Fixed Lib.Time not initializing properly in some specific scenarios
- Removed 'GetPluginGuidFromPartialGuid' method from PluginDetection lib
- Minor bugfixes and code cleanup/optimization
- Added traits, occupations, and world spawning to sync disk builder
- Added JsonUtility as part of RestoredUnityEngine functions helper (thx piepieonline)

**1.1.8**
- Added non-generic bind extension on BepInEx ConfigFile object
- Added support for enum values in interface bindings
- Added method "UpdateConfigFileLayout" to PluginController for IPluginBindings clean up support
- Added Dialog Helper to create dialogs between player and npcs. (Lib.Dialog)
- Added PlayerStatus helper, it provides helpers to set status on player such as illegal status. (Lib.PlayerStatus)
- Fixed DDS entries (names, descriptions, text) not being loaded for custom sync disks when loading/creating a game from an already ongoing game.
- Sync disk DDS records are now unique and cannot be overriden anymore by other mods by accident
- Added ChainLoader util to detect the presence of another mod (Lib.PluginDetection)

**1.1.7**
- Made sync disk re-raise events optional (extra parameter in builder, default true)

**1.1.6**
- Lib.SyncDisks.Builder method signature changed, (added extra parameter)
- Custom installed syncdisks will now re-raise their install and upgrade events on loading a savegame
- Inner exception message is now shown when an error occurs in plugin-bindings proxy.

**1.1.5**
- Added DdsStrings helper to add DDS strings directly in the game.
- Added SyncDisk helper class to very easily create new sync disks and hook events

**1.1.4**
- Fixed null reference errors regarding interaction helpers
- Fixed time not re-initializing when triggering a new game from an ongoing game

**1.1.3**
- Added Interaction helpers (prerelease v1.2.0)

**1.1.2**
- Fixed MonthEnum and DayEnum being wrong
- Added TimeData.AddMinutes method

**1.1.1**
- Fix ToString() formatting for Time.TimeData
- Fix for AddDays and - operator when month/day are 0 in Time.TimeData

**1.1.0**
Bugfixes:
- Bugfix: loading a save game triggers the load process twice

Adjustments:
- Renamed Common class to Lib to make it easier to access the helper classes.
- PluginController Instance now returns the actual class itself by using PluginController<TImpl, TBindings> where TImpl is your Plugin class.

New features:
- Added MersenneTwister random number generator implementation (which allows the full state to be exported and reimported)
- Added IEnumerable extensions for Il2Cpp list objects (Select, Where, ToList, ToListIl2Cpp), including Il2Cpp IList objects
- Added SaveGame helper class (events that trigger such as: NewGame, LoadGame, Create SaveGame, Delete SaveGame)
- Added Time helper class (events and properties to access the in-game time)
- Added InputDetection helper class (events that trigger when a button is pressed)

**1.0.0**
- Initial release

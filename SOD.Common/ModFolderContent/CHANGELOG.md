# CHANGELOG
**1.0.4**
- Added InputDetection helper class (Rewired button state detection)
- Renamed Common class to Lib to make it easier to access the helper classes.
- PluginController Instance now returns the actual class itself by using PluginController<TImpl, TBindings> where TImpl is your Plugin class.
- Added Newtonsoft.Json dependency (for serialization)
- Addes SaveGame helper class (save/load/newgame events)
- Added Time helper class (in game time events)
- Added patch to fix double loading from within a game scene
- Added MersenneTwisterRandom implementation
- Added IEnumerable extensions for Il2Cpp list objects (Select, Where, ToList, ToListIL2Cpp), including Il2Cpp IList objects

**1.0.3**
- Added validation messages for each binding when it is initialized incase something went wrong.

**1.0.1-2**
- Minor fixes

**1.0.0**
[BepInEx Additions (SOD.Common.BepInEx namespace)]
- Added PluginController base class and its generic variant PluginController<T>
- Added model-based configuration within PluginController<T> class

[Extension Additions (SOD.Common.Extensions namespace)]
- Added several extentions (SOD.Common.Extensions namespace)

[Common Additions (SOD.Common.Shadows namespace)]
- Added Common class which contains several helper implementations
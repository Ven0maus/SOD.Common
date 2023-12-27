# CHANGELOG
**1.1.0**
Bugfixes:
- Bugfix: loading a new game triggers twice

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
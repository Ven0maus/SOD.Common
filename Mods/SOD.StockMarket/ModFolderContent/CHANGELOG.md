# CHANGELOG
**2.0.4**
- Some minor fixes in dds records
- Some minor fixes in date printing
- Adjusted some hashcode usage to fnv hashcode from sod.common

**2.0.3**
- Remove ddsloader dependency, use SOD.Common DdsStrings functionality instead
- Fixed issue with Run Simulation parameter freezing game when loading a save

**2.0.2**
- Several fixes regarding purchasing stocks through trade orders

**2.0.1**
- Fixed stockmarket data not initializing properly when starting a new game from an ongoing game
- Added fallback for incompatible savegames when loading an old savegame from pre v2.0.0

**2.0.0**
Bugfixes:
- Fixed bug where loading a savegame would not take over trade orders
- Fixed bug where funds and portfolio history were carrying over to other savegames / newgames
- Fixed bug where outdated portfolio history entries would not be deleted automatically
- Fixed bug where trade orders would not execute properly

Additions:
- Added support for savegames that exist before the mod was installed. (new economy will be generated now)
- Added introduction screen
- Added trade history screen
- Added news screen with period (vague) news articles about trends
- Added in-game notification message toggle option when a buy/sell limit order is completed
- Added button to change the view on the stocks list to show more stocks
- Added possibility to buy stock amounts under whole numbers eg. 0.02 stock amount for total of 100 while the stock price is 5000 for 1 amount
(this is currently limited to if you cannot afford a whole stock amount, no precise self selection yet)

- Improved economy

**1.0.0**
-- Initial version
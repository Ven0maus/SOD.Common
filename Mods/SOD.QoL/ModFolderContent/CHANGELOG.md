# CHANGELOG
**1.1.5**
- Side jobs and LostAndFound items will now automatically expire after some hours to prevent stale evidence from accumulating. (accepted side jobs are excluded)
(hours is by default a random value between 24 and 48, which can be configured)

**1.1.4**
- Fixed null reference exception after consuming certain items

**1.1.3**
- Fixed some companies still not having unlocked doors sometimes on a new game start when they are suppose to be open.

**1.1.2**
- Fixed issue regarding consuming food breaking after using heatpacks

**1.1.1**
- Fixed player never getting tired and adjusted energy restore from caffeine drinks to be more balanced

**1.1.0**
- Fix mouse not responding to input when you have joystick connected but not using joystick to play the game.

**1.0.9**
- Businesses that are supposed to be open according to business hours now no longer have their doors locked on a new game.

**1.0.8**
- Skips the "press any key" screen at start game, immediately loading the mainmenu

**1.0.7**
- Fixed using menu key on crunchers making the cruncher no longer interactable until turned off/on again

**1.0.6**
- Bugfix: overwriting a savegame and pressing ESC would get the interface stuck

**1.0.5**
- The current plotted route is now overriden when plotting a new route instead of canceling it. (to cancel it, plot the same route again)

**1.0.3-4**
- Map is now by default zoomed out
- Center on player button now properly zooms in to the player after zooming and moving camera
- PlayerMarker is now slightly larger and green (customizable)

**1.0.1-2**
- Minor fixes

**1.0.0**
- Added automatic unpause after closing main menu with menu key
- Added ability to end conversations with menu key
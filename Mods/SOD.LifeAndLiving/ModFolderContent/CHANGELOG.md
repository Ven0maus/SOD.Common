# CHANGELOG
**1.0.5**
- Bugfix: Life and Living applies item cost increases to vendors multiple times (thx to PatchyDragon)

**1.0.4**
- Modified the reward calculation logic for side jobs to allow non-multiples of 50
- Modified the reward calculation logic for murder resolution questions to allow non-multiples of 50
- Added configuration option to define minimum value for a murder resolve question reward price
- Removed MinSideJobResolveQuestion configuration, replaced by MinSideJobReward
- Bugfix: calculate penalty properly for sidejobs and murder cases
- Reworked configuration file (added categories)

**1.0.3**
- Bugfix: Side jobs occassionaly paid nothing, it will now atleast pay 50 crows.
- Bugfix: Solved murder cases could potentially pay nothing, it will now atleast pay 50 crows.
- Bugfix: Diamond and SyncDiskModuleUpgrade prices now respect their configuration values
- Reworked item price calculation to be more robust and uniform (not too much price changes)
- Changed item price percentage from 60 to 40 as default value
- Changed min item price from 32 to 20 as default value

**1.0.2**
- Change thunderstore icon to correct one
- Balance side job price to be a little higher

**1.0.1**
- Fix bindings

**1.0.0**
- Reduce payout of side jobs
- Reduce payout of murder cases
- Reduce lockpick amount received from buying a lockpick kit
- Reduce spawn rate of lockpicks
- Reduce the spawn rate of diamonds
- Reduce the value of loose change
- Reduce the spawn rate of loose change
- Reduce the spawn rate of sync disks upgrade modules
- Reduce the spawn rate of sync disks
- Increase cost of apartment
- Increase cost of hotel suites
- Set adjustable max sell price for items to general/blackmarket (diamonds excluded)
- Adjust item value and store prices
- Adjust cost price of furniture
- Adjust cost price of dialog (guest pass, codes, etc..)
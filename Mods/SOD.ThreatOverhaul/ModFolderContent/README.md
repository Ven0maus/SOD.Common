# SOD.ThreatOverhaul

This mod overhauls the threat calculation logic that is executed when a citizen is attacked or see the player do something illegal.
What you can expect is a much more logical pattern to citizens reacting to your actions.

**Note**: All the functionality overhaul options can be fully configured in the configuration file of the mod.

## Overhauled functionalities

**Threat calculation**

When attacking a citizen or doing something illegal, other citizens will only become aggressive in following cases.

- They are a neighbor of the target
- They are a friend / partner of the target
- They are a work colleague of the target
- They are a police officer

Incase when a citizen sees the player doing something illegal the logic will check against the owner of the building / apartment / company.
And its matching employees / partners.

**AND** only if the citizen saw the illegal attack happening.
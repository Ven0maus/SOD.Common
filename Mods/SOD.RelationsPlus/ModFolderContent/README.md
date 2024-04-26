# SOD.RelationsPlus
RelationsPlus is a standalone common library that provides functionality to retrieve and edit relational information between citizens and the player.
The relational information is automatically saved by the library seperately for each savegame.
This mod serves as a stepping stone for other mods, and does not directly influence the game in anyway by itself.

The relational information is tracked in two ways:
- Citizens seeing the player in various locations and statuses
- Citizens directly interacting with the player

**Features**
- Tracks how often the citizens see the player in various locations
- Tracks how much the citizens know the player
- Tracks how much the citizens like the player
- A natural decay so that the citizens forget about the player overtime if not interacted or seen.
- Well documented methods

**Usage**
The library uses a singleton object that controls the relations for each citizen.
You can access this by the following property: "RelationManager.Instance"

To retrieve the relation of a given citizen you can do it in two ways:

// This will create a new relational object if none exists
var relation = RelationManager.Instance[citizenId]; 

// Will do a check if it exists, and outs the relation object (null if none exists)
var exists = RelationManager.Instance.TryGetValue(citizenId, out var relation));

A relation object is automatically created and stored the first time a citizen sees the player or comes in contact with them.

Both the RelationManager and the Relation object have events you can hook to execute your custom code.
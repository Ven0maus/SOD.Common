# SOD.Common
A common library for Shadows of Doubt.
Contains various extensions and methods to manipulate commonly needed gamedata.

# Features
**Modelbased configuration**
```csharp
public interface IConfigBindings
{
    // Binds in the config file as: Prices.SyncDiskPrice
    [Binding(500, "The price for the sync disk.", "Prices.SyncDiskPrice")]
    int SyncDiskPrice { get; set; }

    // Binds in the config file as: General.SomeTextConfig
    [Binding("Hello", "Yep this is some text config!")]
    string SomeTextConfig { get; set; }
}

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("Venomaus.SOD.Common")]
public class Plugin : PluginController<IConfigBindings>
{
    public override void Load()
    {
        Log.LogInfo("SyncDiskPrice: " + Config.SyncDiskPrice);
        Log.LogInfo("SomeTextConfig: " + Config.SomeTextConfig);
    }
}
```
You can also combine interfaces:
```csharp
public interface IConfigBindings : ISomeOtherBindings, ISomeMoreBindings
{ }
```
# Base functionality
The PluginController provides some basic helper and setup functionalities.
Following methods can be overriden and run in the given sequence:

- Load (EntryPoint)
- OnConfigureBindings (Runs at constructor level of PluginController, initializing the model based configuration bindings if any)
- Unload (Runs when the plugin is unloaded, unpatches self)

# UniverseLib
Comes standard with UniverseLib for assetbundle loading and other IL2CPP goodies.
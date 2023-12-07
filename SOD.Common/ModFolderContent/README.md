# SOD.Common
A common library for Shadows of Doubt.
Contains various extensions and methods to manipulate commonly needed gamedata.

# Features
**Modelbased configuration**
```csharp
public interface IConfigBindings
{
    [Binding("Prices.SyncDiskPrice", 500, "The price for the sync disk.")]
    int SyncDiskPrice { get; set; }

    [Binding("General.SomeTextConfig", "Hello", "Yep this is some text config!")]
    string SomeTextConfig { get; set; }
}

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
[BepInDependency("Venomaus.SOD.Common")]
public class Plugin : PluginController<IConfigBindings>
{
    public override void OnBeforePatching()
    {
        Log.LogInfo("SyncDiskPrice: " + Config.SyncDiskPrice);
        Log.LogInfo("SomeTextConfig: " + Config.SomeTextConfig);
    }
}
```
You can also combine these interfaces:
```csharp
public interface IConfigBindings : ISomeOtherBindings, ISomeMoreBindings
{ }
```
# Base functionality
The PluginController provides some base setup methods for your config, hooks, etc..
Following methods can be overriden and run in the given sequence:

- Load (Plugin entry point)
- OnConfigureBindings (runs at the start of load, initializes the modelbased configuration)
- OnBeforePatching (runs before OnPatching, can be used to setup custom logic)
- OnPatching (runs after OnConfigureBindings, uses harmony to patch the assembly)
- Unload (runs when the plugin is unloaded, unpatches self)

# UniverseLib
Comes standard with UniverseLib for assetbundle loading and other IL2CPP goodies.
# SOD.Common
A common library for Shadows of Doubt mods.
Contains various extensions and methods to manipulate commonly needed gamedata.

# Features
**PluginController**
The PluginController provides some basic helper and setup functionalities for your plugin.
Following methods can be overriden and run in the given sequence:

- Load (EntryPoint)
- OnConfigureBindings (Runs at constructor level of PluginController, initializing the model based configuration bindings if any exist)
- Unload (Runs when the plugin is unloaded, unpatches self)

**Extensions**
The extensions namespace contains several extensions for things such as logging, enumerables, general quality of life, etc..

**Modelbased configuration**
It is now possible to model your configuration within interfaces,
to easily access or set configuration in your bepinex config file.
Here is a quick example:
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
You can also combine multiple interfaces:
```csharp
public interface IConfigBindings : ISomeOtherBindings, ISomeMoreBindings
{ }
```
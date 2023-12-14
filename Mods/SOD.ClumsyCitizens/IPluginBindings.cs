using SOD.Common.BepInEx.Configuration;

namespace SOD.ClumsyCitizens
{
    public interface IPluginBindings
    {
        [Binding(defaultValue: 5, description: "The 0-100 % chance a citizen forgets to lock a door that is suppose to be locked.", name: "Actions.Chances.ForgetDoorLock")]
        int ChanceToForgetDoorLock { get; set; }
    }
}

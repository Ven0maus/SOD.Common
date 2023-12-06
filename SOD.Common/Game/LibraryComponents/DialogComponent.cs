namespace SOD.Common.Game.LibraryComponents
{
    public sealed class DialogComponent
    {
        internal DialogComponent() { }

        /// <summary>
        /// All dialog presets, add your preset here in ToolBox.LoadAll hook
        /// </summary>
        public Il2CppSystem.Collections.Generic.List<DialogPreset> AllDialogPresets => Toolbox.Instance.allDialog;

        public void AddDialogOption(Human human, DialogPreset preset, bool allowPresetDuplicates) 
        {
            human.evidenceEntry.AddDialogOption(Evidence.DataKey.voice, preset, allowPresetDuplicates: allowPresetDuplicates);
        }
    }
}

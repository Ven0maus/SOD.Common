using HarmonyLib;
using SOD.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Patches
{
    internal class InteriorCreatorPatches
    {
        private static bool _isGeneratingChunk = false;
        private static List<SyncDiskPreset> _removedPresets;

        [HarmonyPatch(typeof(InteriorCreator), nameof(InteriorCreator.StartLoading))]
        internal class InteriorCreator_StartLoading
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                _isGeneratingChunk = true;

                var noWorldGenSpawnedDisks = Lib.SyncDisks.RegisteredSyncDisks.Where(a => !a.EnableWorldSpawn);
                foreach (var disk in noWorldGenSpawnedDisks)
                {
                    _removedPresets ??= new List<SyncDiskPreset>();
                    _removedPresets.Add(disk.Preset);
                    Toolbox.Instance.allSyncDisks.Remove(disk.Preset);
                }
            }
        }

        [HarmonyPatch(typeof(Creator), nameof(Creator.SetComplete))]
        internal class Creator_SetComplete
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                if (_isGeneratingChunk)
                {
                    _isGeneratingChunk = false;
                    if (_removedPresets != null)
                    {
                        foreach (var preset in _removedPresets)
                            Toolbox.Instance.allSyncDisks.Add(preset);
                        _removedPresets = null;
                    }
                }
            }
        }
    }
}

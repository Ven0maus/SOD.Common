using HarmonyLib;
using Il2CppSystem.Reflection;
using SOD.Common.Helpers;
using SOD.Common.Helpers.DialogObjects;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Patches
{
    internal class DialogControllerPatches
    {
        private static readonly Dictionary<string, DialogObject> _customDialogInterceptors = new();

        [HarmonyPatch(typeof(DialogController), nameof(DialogController.Start))]
        internal static class DialogController_Start
        {
            [HarmonyPostfix]
            internal static void Postfix(DialogController __instance)
            {
                foreach (var customDialog in Lib.Dialog.RegisteredDialogs)
                {
                    Toolbox.Instance.allDialog.Add(customDialog.Preset);
                    if (customDialog.Preset.defaultOption)
                        Toolbox.Instance.defaultDialogOptions.Add(customDialog.Preset);

                    MethodInfo warnNotewriterMI = null;
                    foreach (var dialogPreset in Toolbox.Instance.allDialog)
                    {
                        if (dialogPreset.name == "WarnNotewriter")
                        {
                            warnNotewriterMI = __instance.dialogRef[dialogPreset];
                            break;
                        }
                    }

                    if (warnNotewriterMI == null)
                    {
                        Plugin.Log.LogError("Warn note writer is no longer present in this version of the game. Dialog mods will no longer work at the moment.");
                        break;
                    }

                    __instance.dialogRef.Add(customDialog.Preset, warnNotewriterMI);

                    _customDialogInterceptors[customDialog.Preset.name] = customDialog;
                }

                if (Lib.Dialog.RegisteredDialogs.Count > 0)
                    Plugin.Log.LogInfo($"Loaded {Lib.Dialog.RegisteredDialogs.Count} custom dialogs.");
            }
        }

        // Patch to allow running code when the dialog is selected
        // We can't add extension methods, and the called methods are instance members of DialogController. So always use this one, and just catch the cases of it being called.
        [HarmonyPatch(typeof(DialogController), nameof(DialogController.WarnNotewriter))]
        public class DialogController_WarnNotewriter
        {
            [HarmonyPrefix]
            internal static bool Prefix(DialogController __instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
            {
                if (_customDialogInterceptors.TryGetValue(__instance.preset.name, out var interceptor) && interceptor.DialogLogic != null)
                {
                    interceptor.DialogLogic.OnDialogExecute(__instance, saysTo, saysToInteractable, where, saidBy, success, roomRef, jobRef);
                    return false;
                }

                return true;
            }
        }

        // Patch to only show the dialog if it should be visible
        [HarmonyPatch(typeof(DialogController), nameof(DialogController.TestSpecialCaseAvailability))]
        public class DialogController_TestSpecialCaseAvailability
        {
            [HarmonyPrefix]
            internal static bool Prefix(ref bool __result, DialogPreset preset, Citizen saysTo, SideJob jobRef)
            {
                if (_customDialogInterceptors.TryGetValue(preset.name, out var interceptor) && interceptor.DialogLogic != null)
                {
                    __result = interceptor.DialogLogic.IsDialogShown(preset, saysTo, jobRef);
                    if (__result)
                    {
                        // Check if we have dynamic text, if so update it now before it is shown.
                        interceptor.UpdateDynamicText();
                    }
                    return false;
                }

                return true;
            }
        }

        // Patch to allow for custom success/failure overrides
        [HarmonyPatch(typeof(DialogController), nameof(DialogController.ExecuteDialog))]
        public class DialogController_ExecuteDialog
        {
            [HarmonyPrefix]
            internal static void Prefix(DialogController __instance, EvidenceWitness.DialogOption dialog, Interactable saysTo, NewNode where, Actor saidBy, ref DialogController.ForceSuccess forceSuccess)
            {
                if (forceSuccess == DialogController.ForceSuccess.none && _customDialogInterceptors.TryGetValue(dialog.preset.name, out var interceptor) && interceptor.DialogLogic != null)
                {
                    Citizen saysToCit = ((dynamic)saysTo.isActor).Cast<Citizen>();
                    forceSuccess = interceptor.DialogLogic.ShouldDialogSucceedOverride(__instance, dialog, saysToCit, where, saidBy);
                }
            }
        }
    }
}

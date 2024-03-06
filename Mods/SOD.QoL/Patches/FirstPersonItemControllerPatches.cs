using HarmonyLib;
using SOD.Common.Custom;
using System.Collections;
using UnityEngine;

namespace SOD.QoL.Patches
{
    internal class FirstPersonItemControllerPatches
    {
        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.Update))]
        internal class FirstPersonItemController_Update
        {
            internal readonly struct PlayerState
            {
                public float Alertness { get; }
                public float Energy { get; }
                public float Cs { get; }

                public PlayerState(float alertness, float energy, float cs)
                {
                    Alertness = alertness;
                    Energy = energy;
                    Cs = cs;
                }
            }

            [HarmonyPrefix]
            internal static void Prefix(FirstPersonItemController __instance, ref PlayerState __state)
            {
                if (__instance.isConsuming && !__instance.takeOneActive && BioScreenController.Instance.selectedSlot != null && 
                    BioScreenController.Instance.selectedSlot.interactableID > -1)
                {
                    Interactable interactable = BioScreenController.Instance.selectedSlot.GetInteractable();
                    if (interactable != null && interactable.cs > 0f)
                        __state = new PlayerState(Player.Instance.alertness, Player.Instance.energy, interactable.cs);
                }
                else
                {
                    __state = new PlayerState(Player.Instance.alertness, Player.Instance.energy, 0f);
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(ref PlayerState __state)
            {
                // Reset alertness to original values before this frame's modification
                Player.Instance.alertness = __state.Alertness;
                Player.Instance.energy = __state.Energy;

                // Re-calculate the alertness value modification
                if (__state.Cs > 0f)
                {
                    Interactable interactable = BioScreenController.Instance.selectedSlot.GetInteractable();
                    if (interactable.preset.retailItem != null)
                    {
                        // Set alertness properly
                        Player.Instance.alertness += interactable.preset.retailItem.alertness / interactable.preset.consumableAmount * Time.deltaTime;
                        Player.Instance.alertness = Mathf.Clamp01(Player.Instance.alertness);
                        Player.Instance.StatusCheckEndOfFrame();

                        // Set energy properly
                        Player.Instance.energy += interactable.preset.retailItem.energy / interactable.preset.consumableAmount * Time.deltaTime;
                        Player.Instance.energy = Mathf.Clamp01(Player.Instance.energy);
                        Player.Instance.StatusCheckEndOfFrame();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.TakeOneExecute))]
        internal class FirstPersonItemController_TakeOneExecute
        {
            private static float _alertness = 0f;
            private static float _energy = 0f;
            private static Interactable _consumable;

            [HarmonyPostfix]
            internal static void Postfix(ref IEnumerator __result)
            {
                __result = EnumeratorWrapper.Wrap(__result, PrefixAction, PostfixAction, PreYieldReturnAction);
            }

            private static void PrefixAction()
            {
                _alertness = Player.Instance.alertness;
                _energy = Player.Instance.energy;

                // set consumable
                if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                {
                    _consumable = BioScreenController.Instance.selectedSlot.GetInteractable();
                }
            }

            private static void PreYieldReturnAction(object returnValue)
            {
                Player.Instance.alertness = _alertness;
                Player.Instance.energy = _energy;

                // Set alertness properly
                if (_consumable != null && _consumable.preset.retailItem != null)
                {
                    float num = Time.deltaTime / 1.8f;
                    Player.Instance.alertness += _consumable.preset.retailItem.alertness * num;
                    Player.Instance.alertness = Mathf.Clamp01(Player.Instance.alertness);
                    Player.Instance.StatusCheckEndOfFrame();

                    // Set energy properly
                    Player.Instance.energy += _consumable.preset.retailItem.energy * num;
                    Player.Instance.energy = Mathf.Clamp01(Player.Instance.energy);
                    Player.Instance.StatusCheckEndOfFrame();
                }

                _alertness = Player.Instance.alertness;
                _energy = Player.Instance.energy;
            }

            private static void PostfixAction()
            {
                _consumable = null;
            }
        }
    }
}

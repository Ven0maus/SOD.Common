using HarmonyLib;
using System;
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
                if (!Plugin.Instance.Config.FixTiredness) return;
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
                if (!Plugin.Instance.Config.FixTiredness) return;
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

                        if (interactable.preset.retailItem.desireCategory == CompanyPreset.CompanyCategory.caffeine &&
                            interactable.preset.retailItem.energy <= 0f)
                        {
                            // Adjust retailItem energy if it is not yet set properly for caffeine item. (support for existing saves)
                            interactable.preset.retailItem.energy = (float)Math.Round(interactable.preset.retailItem.alertness / 100 * Plugin.Instance.Config.PercentageEnergyRestore, 2);
                        }

                        // Set energy properly
                        Player.Instance.energy += interactable.preset.retailItem.energy / interactable.preset.consumableAmount * Time.deltaTime;
                        Player.Instance.energy = Mathf.Clamp01(Player.Instance.energy);
                        Player.Instance.StatusCheckEndOfFrame();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.TakeOne))]
        internal class FirstPersonItemController_TakeOne
        {
            [HarmonyPrefix]
            internal static bool Prefix(FirstPersonItemController __instance)
            {
                if (!Plugin.Instance.Config.FixTiredness) return true;
                if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                {
                    Interactable interactable = BioScreenController.Instance.selectedSlot.GetInteractable();
                    if (interactable.preset.takeOneEvent != null)
                    {
                        AudioController.Instance.Play2DSound(interactable.preset.takeOneEvent, null, 1f);
                    }
                }
                if (!__instance.takeOneActive)
                    UniverseLib.RuntimeHelper.StartCoroutine(TakeOneExecute(__instance));
                return false;
            }

            private static IEnumerator TakeOneExecute(FirstPersonItemController __instance)
            {
                float progress = 0f;
                __instance.takeOneActive = true;
                __instance.SetConsuming(true);
                Interactable consumable = null;
                if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                {
                    consumable = BioScreenController.Instance.selectedSlot.GetInteractable();
                }

                while (progress < 1f && consumable != null && consumable.cs > 0f)
                {
                    float num = Time.deltaTime / 1.8f;
                    if (consumable.preset.retailItem != null)
                    {
                        Player.Instance.AddNourishment(consumable.preset.retailItem.nourishment * num);
                        Player.Instance.AddHydration(consumable.preset.retailItem.hydration * num);

                        // Handle energy and alertness seperately
                        HandleEnergyAndAlertness(consumable, num);

                        Player.Instance.AddExcitement(consumable.preset.retailItem.excitement * num);
                        Player.Instance.AddChores(consumable.preset.retailItem.chores * num);
                        Player.Instance.AddHygiene(consumable.preset.retailItem.hygiene * num);
                        Player.Instance.AddBladder(consumable.preset.retailItem.bladder * num);
                        Player.Instance.AddHeat(consumable.preset.retailItem.heat * num);
                        Player.Instance.AddDrunk(consumable.preset.retailItem.drunk * num);
                        Player.Instance.AddSick(consumable.preset.retailItem.sick * num);
                        Player.Instance.AddHeadache(consumable.preset.retailItem.headache * num);
                        Player.Instance.AddWet(consumable.preset.retailItem.wet * num);
                        Player.Instance.AddBrokenLeg(consumable.preset.retailItem.brokenLeg * num);
                        Player.Instance.AddBruised(consumable.preset.retailItem.bruised * num);
                        Player.Instance.AddBlackEye(consumable.preset.retailItem.blackEye * num);
                        Player.Instance.AddBlackedOut(consumable.preset.retailItem.blackedOut * num);
                        Player.Instance.AddNumb(consumable.preset.retailItem.numb * num);
                        Player.Instance.AddBleeding(consumable.preset.retailItem.bleeding * num);
                        Player.Instance.AddBreath(consumable.preset.retailItem.breath * num);
                        Player.Instance.AddStarchAddiction(consumable.preset.retailItem.starchAddiction * num);
                        Player.Instance.AddPoisoned(consumable.preset.retailItem.poisoned * num, null);
                        Player.Instance.AddHealth(consumable.preset.retailItem.health * num, true, false);
                    }
                    progress += num;
                    yield return null;
                }
                if (consumable != null)
                {
                    consumable.cs -= 1f;
                    if (consumable.cs <= 0f)
                    {
                        __instance.OnConsumableFinished(consumable);
                        if (consumable.preset.destroyWhenAllConsumed)
                        {
                            __instance.EmptySlot(BioScreenController.Instance.selectedSlot, false, true, true, false);
                            foreach (object obj in __instance.rightHandObjectParent)
                            {
                                UnityEngine.Object.Destroy(((Transform)obj).gameObject);
                            }
                        }
                    }
                }
                __instance.takeOneActive = false;
                __instance.SetConsuming(false);
                if (consumable.cs <= 0f)
                {
                    __instance.RefreshHeldObjects();
                }
                yield break;
            }

            private static void HandleEnergyAndAlertness(Interactable consumable, float num)
            {
                // Set alertness properly
                Player.Instance.alertness += consumable.preset.retailItem.alertness * num;
                Player.Instance.alertness = Mathf.Clamp01(Player.Instance.alertness);
                Player.Instance.StatusCheckEndOfFrame();

                if (consumable.preset.retailItem.desireCategory == CompanyPreset.CompanyCategory.caffeine &&
                    consumable.preset.retailItem.energy <= 0f)
                {
                    // Adjust retailItem energy if it is not yet set properly for caffeine item. (support for existing saves)
                    consumable.preset.retailItem.energy = (float)Math.Round(consumable.preset.retailItem.alertness / 100 * Plugin.Instance.Config.PercentageEnergyRestore, 2);
                }

                // Set energy properly
                Player.Instance.energy += consumable.preset.retailItem.energy * num;
                Player.Instance.energy = Mathf.Clamp01(Player.Instance.energy);
                Player.Instance.StatusCheckEndOfFrame();
            }
        }
    }
}

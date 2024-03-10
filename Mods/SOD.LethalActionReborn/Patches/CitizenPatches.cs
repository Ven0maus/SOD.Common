using HarmonyLib;
using System;
using UnityEngine;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            private static Lazy<MurderWeaponPreset> _loadedPreset = new Lazy<MurderWeaponPreset>(() => Resources.Load<MurderWeaponPreset>("data/weapons/bluntobjects/Dumbell"));

            [HarmonyPrefix]
            internal static void Prefix(Citizen __instance, float amount, Actor fromWho, ref bool enableKill)
            {
                if (!__instance.isDead && __instance.currentHealth - amount <= 0 && fromWho == Player.Instance)
                {
                    __instance.isStunned = false;

                    // Select weapon
                    var weapon = BioScreenController.Instance.selectedSlot.GetInteractable();
                    if (weapon == null)
                    {
                        weapon = new Interactable(new InteractablePreset());
                        weapon.preset.weapon = InteriorControls._instance.fistsWeapon;
                    }

                    var murder = new MurderController.Murder(Player.Instance.animationController.cit, 
                        __instance.animationController.cit, MurderController.Instance.murderPreset, new MurderMO())
                    {
                        weapon = weapon
                    };

                    murder.weaponStr = murder.weapon.name;
                    murder.weaponID = murder.weapon.id;
                    murder.weaponPreset = murder.weapon.preset;
                    if (murder.weaponPreset.weapon == null)
                        murder.weaponPreset.weapon = _loadedPreset.Value;
                    murder.victim = __instance.animationController.cit;
                    murder.victimID = __instance.animationController.cit.humanID;
                    murder.murderer = Player.Instance;
                    murder.murdererID = Player.Instance.humanID;
                    murder.location = __instance.animationController.cit.currentGameLocation.thisAsAddress;

                    MurderController.Instance.activeMurders.Remove(murder);
                    __instance.animationController.cit.Murder(Player.Instance, true, murder, weapon);

                    enableKill = false;
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(Citizen __instance, Actor fromWho)
            {
                if (__instance.isDead && __instance.currentHealth <= 0 && fromWho == Player.Instance)
                {
                    if (MurderController.Instance.currentMurderer == __instance)
                    {
                        MurderController.Instance.PickNewMurderer();
                        MurderController.Instance.PickNewVictim();
                    }
                    else if (MurderController.Instance.currentVictim == __instance)
                    {
                        MurderController.Instance.PickNewVictim();
                    }
                }
            }
        }
    }
}

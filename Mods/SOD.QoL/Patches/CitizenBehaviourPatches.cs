using HarmonyLib;
using UnityEngine;

namespace SOD.QoL.Patches
{
    internal class CitizenBehaviourPatches
    {
        [HarmonyPatch(typeof(CitizenBehaviour), nameof(CitizenBehaviour.GameWorldCheck))]
        internal class CitizenBehaviour_GameWorldCheck
        {
            internal readonly struct PlayerState
            {
                public float Energy { get; }
                public float WellRested { get; }
                public float Alertness { get; }
                public float TickChange { get; }

                public PlayerState(float energy, float wellRested, float alertness, float tickChange)
                {
                    Energy = energy;
                    WellRested = wellRested;
                    Alertness = alertness;
                    TickChange = tickChange;
                }
            }

            [HarmonyPrefix]
            internal static void Prefix(CitizenBehaviour __instance, ref PlayerState __state)
            {
                if (!Plugin.Instance.Config.FixTiredness) return;
                var tickRate = SessionData.Instance.gameTime - __instance.timeOnLastGameWorldUpdate;
                __state = new PlayerState(Player.Instance.energy, Player.Instance.wellRested, Player.Instance.alertness, tickRate);
            }

            [HarmonyPostfix]
            internal static void Postfix(ref PlayerState __state)
            {
                if (!Plugin.Instance.Config.FixTiredness) return;
                // Reset alertness, energy and wellRested back to original values before this frame's modification
                Player.Instance.energy = __state.Energy;
                Player.Instance.alertness = __state.Alertness;
                Player.Instance.wellRested = __state.WellRested;

                // Re-calculate the alertness, energy and wellRested value modifications
                if (Player.Instance.spendingTimeMode && InteractionController.Instance.lockedInInteraction != null && 
                    InteractionController.Instance.lockedInInteraction.preset.specialCaseFlag == InteractablePreset.SpecialCase.sleepPosition)
                {
                    if (Player.Instance.energy >= 0.8f)
                        Player.Instance.AddWellRested(__state.TickChange / 2f);
                    Player.Instance.AddEnergy(__state.TickChange / 4f);
                }
                else
                {
                    if (!Game.Instance.disableSurvivalStatusesInStory || !Toolbox.Instance.IsStoryMissionActive(out _, out _))
                    {
                        // Set alertness properly
                        Player.Instance.alertness += GameplayControls.Instance.playerTirednessRate * -__state.TickChange;
                        Player.Instance.alertness = Mathf.Clamp01(Player.Instance.alertness);
                        Player.Instance.StatusCheckEndOfFrame();

                        // Reduce energy of player
                        Player.Instance.AddEnergy(GameplayControls.Instance.playerTirednessRate * -__state.TickChange);
                    }

                    Player.Instance.AddWellRested(__state.TickChange * -0.5f);
                }
            }
        }
    }
}

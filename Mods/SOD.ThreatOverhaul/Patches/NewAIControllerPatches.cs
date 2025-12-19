using HarmonyLib;
using SOD.Common.Extensions;

namespace SOD.ThreatOverhaul.Patches
{
    internal class NewAIControllerPatches
    {
        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.SetPersue))]
        internal static class NewAIController_SetPersue
        {
            private static bool _wasPublicFauxPas = false;

            [HarmonyPrefix]
            internal static bool Prefix(ref bool publicFauxPas, Actor newTarget)
            {
                if (!newTarget.isPlayer || !Plugin.Instance.Config.EnableThreatCalculationOverhaul) return true;

                _wasPublicFauxPas = publicFauxPas;
                publicFauxPas = false;
                return true;
            }

            [HarmonyPostfix]
            internal static void Postfix(NewAIController __instance, Actor newTarget, ref bool publicFauxPas, int escalation, bool setHighUrgency, float responseRange)
            {
                if (_wasPublicFauxPas)
                {
                    _wasPublicFauxPas = false;

                    foreach (Actor actor in __instance.human.currentRoom.currentOccupants)
                    {
                        if (actor == __instance.human || actor.isStunned || actor.isDead || actor.isAsleep || actor.ai == null) 
                            continue;

                        if (Plugin.Instance.Config.IncludeSeeingThreat && !actor.Sees(newTarget))
                            continue;

                        var distance = (actor.transform.position - __instance.human.transform.position).sqrMagnitude;
                        if (distance <= (responseRange * responseRange) && IsTargetOfInterest(newTarget, actor))
                        {
                            if (Plugin.Instance.Config.EnableDebugMode)
                                Plugin.Log.LogInfo("Threat seen by: " + actor.name + " and engaged persuit!");

                            actor.ai.SetPersuit(true);
                            actor.ai.SetPersueTarget(newTarget);
                            actor.AddToSeesIllegal(newTarget, 1f * SessionData.Instance.currentTimeMultiplier);
                            actor.ai.persuitChaseLogicUses = 0.5f;
                            actor.ai.Investigate(newTarget.currentNode, newTarget.transform.position, newTarget, NewAIController.ReactionState.persuing, CitizenControls.Instance.persuitMinInvestigationTimeMP, escalation, setHighUrgency, 1f, null);
                        }
                    }

                    publicFauxPas = true;
                }
            }
        }


        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.PersuitUpdate))]
        internal static class NewAIController_PersuitUpdate
        {
            private static (Actor, float)? _remove = null;

            [HarmonyPrefix]
            internal static void Prefix(NewAIController __instance)
            {
                if (!Plugin.Instance.Config.EnableThreatCalculationOverhaul) return;
                if (__instance.human.seesIllegal != null && __instance.human.seesIllegal.Count > 0 && !__instance.persuit)
                {
                    foreach (var actor in __instance.human.seesIllegal)
                    {
                        var player = actor.Key.TryCast<Human>();
                        if (player == null || !player.isPlayer) continue;

                        // Check if the player is in a place that this citizen cares about
                        bool isTargetOfInterest = IsTargetOfInterest(player, __instance.human);
                        if (!isTargetOfInterest)
                        {
                            _remove = (actor.Key, actor.value);
                        }

                        break;
                    }

                    if (_remove != null)
                    {
                        __instance.human.seesIllegal.Remove(_remove.Value.Item1);
                        _remove = null;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.InstantPersuitCheck))]
        internal static class NewAIController_InstantPersuitCheck
        {
            [HarmonyPrefix]
            internal static bool Prefix(NewAIController __instance, Actor target)
            {
                if (!Plugin.Instance.Config.EnableThreatCalculationOverhaul || !target.isPlayer) return true;
                
                var toi = IsTargetOfInterest(target, __instance.human);
                if (toi && Plugin.Instance.Config.EnableDebugMode)
                    Plugin.Log.LogInfo($"Instant check: " + __instance.human.GetCitizenName());
                return toi;
            }
        }

        private static bool IsTargetOfInterest(Actor player, Actor thirdParty)
        {
            // Check if the player is in a place that this citizen cares about
            bool match = false;
            if (player.currentRoom != null && player.currentRoom.belongsTo != null)
            {
                match = player.currentRoom.belongsTo.Any(a =>
                {
                    if (DoesActorMatchModCriteria(a, thirdParty))
                        return true;
                    if (a.partner != null)
                        return DoesActorMatchModCriteria(a.partner, thirdParty);
                    return false;
                });
            }
            if (!match && player.currentGameLocation != null && player.currentGameLocation.thisAsAddress != null)
            {
                var company = player.currentGameLocation.thisAsAddress.company;
                if (company != null)
                {
                    // Check if janitor
                    if (company.janitor != null)
                        match = DoesActorMatchModCriteria(company.janitor, thirdParty);

                    // Check if owner
                    if (!match && company.director != null)
                        match = DoesActorMatchModCriteria(company.director, thirdParty);

                    if (!match && company.receptionist != null)
                        match = DoesActorMatchModCriteria(company.receptionist, thirdParty);

                    if (!match && company.security != null)
                        match = DoesActorMatchModCriteria(company.security, thirdParty);

                    // Check employees
                    if (!match && company.currentStaff != null)
                        match = company.currentStaff.Any(a => a.employee != null && DoesActorMatchModCriteria(a.employee, thirdParty));
                }
            }
            return match;
        }

        private static bool DoesActorMatchModCriteria(Actor source, Actor thirdParty)
        {
            var sourceHuman = source.TryCast<Human>();
            var thirdPartyHuman = thirdParty.TryCast<Human>();

            if (sourceHuman.humanID == thirdPartyHuman.humanID)
                return true;

            // Police officer
            if (thirdPartyHuman.isEnforcer && Plugin.Instance.Config.IncludePoliceOfficers)
            {
                if (Plugin.Instance.Config.EnableDebugMode)
                    Plugin.Log.LogInfo($"Reaction of {thirdPartyHuman.GetCitizenName()} | Reason: Is Enforcer");
                return true;
            }

            /*
             * They are a neighbor of the target
             * They are a friend of the target
             * They are a work colleague of the target
            */
            if (thirdPartyHuman.FindAcquaintanceExists(sourceHuman, out var acquaintance))
            {
                foreach (var connection in acquaintance.connections)
                {
                    switch (connection)
                    {
                        case Acquaintance.ConnectionType.regularStaff:
                        case Acquaintance.ConnectionType.workOther:
                        case Acquaintance.ConnectionType.familiarWork:
                        case Acquaintance.ConnectionType.workTeam:
                        case Acquaintance.ConnectionType.workNotBoss:
                        case Acquaintance.ConnectionType.boss:
                            if (Plugin.Instance.Config.IncludeColleagues)
                            {
                                if (Plugin.Instance.Config.EnableDebugMode)
                                    Plugin.Log.LogInfo($"Reaction of {thirdPartyHuman.GetCitizenName()} | Reason: Is Colleague");
                                return true;
                            }
                            break;

                        case Acquaintance.ConnectionType.friendOrWork:
                        case Acquaintance.ConnectionType.housemate:
                        case Acquaintance.ConnectionType.friend:
                        case Acquaintance.ConnectionType.lover:
                            if (Plugin.Instance.Config.IncludeFriends)
                            {
                                if (Plugin.Instance.Config.EnableDebugMode)
                                    Plugin.Log.LogInfo($"Reaction of {thirdPartyHuman.GetCitizenName()} | Reason: Is Friend");
                                return true;
                            }
                            break;

                        case Acquaintance.ConnectionType.neighbor:
                            if (Plugin.Instance.Config.IncludeNeighbors)
                            {
                                if (Plugin.Instance.Config.EnableDebugMode)
                                    Plugin.Log.LogInfo($"Reaction of {thirdPartyHuman.GetCitizenName()} | Reason: Is Neighbor");
                                return true;
                            }
                            break;
                    }
                }
            }

            return false;
        }
    }
}

using HarmonyLib;
using SOD.Common.Extensions;
using System;
using System.Linq;

namespace SOD.QoL.Patches
{
    internal class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal static class Toolbox_LoadAll
        {
            [HarmonyPostfix]
            internal static void Postfix(Toolbox __instance)
            {
                if (Plugin.Instance.Config.FixTiredness)
                {
                    var allItems = __instance.GetFromResourceCache<RetailItemPreset>();
                    foreach (var item in allItems
                        .Where(a => a.desireCategory == CompanyPreset.CompanyCategory.caffeine))
                    {
                        // If the energy is 0 or smaller, we take 12% of the alertness value
                        if (item.energy <= 0f)
                        {
                            item.energy = (float)Math.Round(item.alertness / 100 * 12, 2);
                            Plugin.Log.LogInfo($"Adjusted energy restore amount for \"{item.name}\" to \"{item.energy}\".");
                        }
                    }
                }

                if (Plugin.Instance.Config.AddWalletLinkToAddress)
                {
                    if (Toolbox.Instance.evidencePresetDictionary.TryGetValue("wallet", out var walletPreset))
                    {
                        if (walletPreset.keyMergeOnDiscovery.Count > 0 && walletPreset.keyMergeOnDiscovery[0].mergeKeys != null)
                        {
                            walletPreset.keyMergeOnDiscovery[0].mergeKeys.Add(Evidence.DataKey.address);
                        }
                        walletPreset.addFactLinks.Add(new EvidencePreset.FactLinkSetup
                        {
                            discovery = true,
                            factDictionary = "LivesAt",
                            subject = EvidencePreset.FactLinkSubject.writer,
                            key = Evidence.DataKey.name
                        });
                    }
                }

                if (Plugin.Instance.Config.AddEmploymentContractLinkToAddress) 
                {
                    if (Toolbox.Instance.evidencePresetDictionary.TryGetValue("employmentcontract",
                            out var employmentContractPreset))
                    {
                        employmentContractPreset.addFactLinks[0].subject = EvidencePreset.FactLinkSubject.writer;
                    }
                }
            }
        }
    }
}

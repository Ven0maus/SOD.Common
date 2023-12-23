using HarmonyLib;
using UnityEngine;

namespace SOD.QoL.Patches
{
    internal class RoutePlotPatches
    {
        /// <summary>
        /// Allows a new route to be plotted even when there is already a route plotted.
        /// </summary>
        [HarmonyPatch(typeof(EvidenceLocationalControls), nameof(EvidenceLocationalControls.OnPlotRoute))]
        internal class EvidenceLocationalControls_OnPlotRoute
        {
            [HarmonyPrefix]
            internal static bool Prefix(EvidenceLocationalControls __instance)
            {
                // If a route already exists, remove it but store the previous
                Vector3Int? prev = null;
                if (MapController.Instance.playerRoute != null)
                {
                    prev = MapController.Instance.playerRoute.end.nodeCoord;
                    MapController.Instance.playerRoute.Remove();
                    if (__instance.plotRouteButton != null)
                        __instance.plotRouteButton.button.image.color = __instance.plotRouteButton.baseColour;
                }

                // Plot route
                MapController.Instance.PlotPlayerRoute(__instance.parentWindow.passedEvidence);

                // If the location is the same, simply remove it
                if (prev != null && MapController.Instance.playerRoute.end.nodeCoord == prev)
                    MapController.Instance.playerRoute.Remove();

                return false;
            }
        }
    }
}

using SOD.Common;
using SOD.LifeAndLiving.Patches.SocialRelationPatches.DialogLogic;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches
{
    internal static class CivilianDialogAdditions
    {
        public static void Initialize()
        {
            TheUsualDialog.Register();
        }
    }
}

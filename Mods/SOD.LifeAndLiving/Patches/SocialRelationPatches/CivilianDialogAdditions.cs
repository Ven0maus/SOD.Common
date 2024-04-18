using SOD.Common;
using SOD.LifeAndLiving.Patches.SocialRelationPatches.DialogLogic;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches
{
    internal static class CivilianDialogAdditions
    {
        public static void Initialize()
        {
            AddPurchaseRelatedDialog();
        }

        /// <summary>
        /// Add's several dialog options regarding purchasing items often at places.
        /// </summary>
        private static void AddPurchaseRelatedDialog()
        {
            _ = Lib.Dialog.Builder("TheUsualPurchase")
                .SetText("You already know what I want.")
                .AddResponse("Ahh yes, the usual for you good sir.")
                .ModifyDialogOptions((a) => 
                {
                    a.useSuccessTest = false;
                    a.ranking = 100;
                })
                .SetDialogLogic(new TheUsualDialogLogic())
                .CreateAndRegister();
        }
    }
}

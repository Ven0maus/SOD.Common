using SOD.LifeAndLiving.Content.SocialRelation.Dialogs;

namespace SOD.LifeAndLiving.Content.SocialRelation
{
    internal static class CivilianDialogAdditions
    {
        public static void Initialize()
        {
            if (Plugin.Instance.Config.DisableSocialRelations) return;
            TheUsualDialog.Register();
        }
    }
}

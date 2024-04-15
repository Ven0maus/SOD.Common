namespace SOD.Common.Helpers.DialogObjects
{
    /// <summary>
    /// Interface that provides dialog condition context for a dialog.
    /// <br>Inherit this to implement conditional dialogs.</br>
    /// </summary>
    public interface IDialogLogic
    {
        /// <summary>
        /// Determines if the dialog should be shown or not.
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="saysTo"></param>
        /// <param name="jobRef"></param>
        /// <returns></returns>
        public abstract bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef);

        /// <summary>
        /// The method that is called when the dialog is executed.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="saysTo"></param>
        /// <param name="saysToInteractable"></param>
        /// <param name="where"></param>
        /// <param name="saidBy"></param>
        /// <param name="success"></param>
        /// <param name="roomRef"></param>
        /// <param name="jobRef"></param>
        public abstract void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef);

        /// <summary>
        /// Should the succesion call be overriden incase it is succesful but it should not be.
        /// <br>Example: Calling partner to the door of an npc, but their partner is not home. Then this should not succeed, and dialog should not be shown.</br>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dialog"></param>
        /// <param name="saysTo"></param>
        /// <param name="where"></param>
        /// <param name="saidBy"></param>
        /// <returns></returns>
        public abstract DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy);
    }
}

using SOD.Common.Helpers.DialogObjects;
using System;
using System.Collections.Generic;

namespace SOD.Common.Helpers
{
    /// <summary>
    /// Helper class to create dialogs between Player and Npcs
    /// </summary>
    public sealed class Dialog
    {
        internal Dialog() { }

        /// <summary>
        /// All the registered dialogs in game by mods.
        /// </summary>
        internal List<DialogObject> RegisteredDialogs = new();
        /// <summary>
        /// Returns a list of all the known dialogs that have so far been registered by mods using SOD.Common Dialog helper functionality.
        /// </summary>
        public IReadOnlyList<DialogObject> KnownModDialogs => RegisteredDialogs;

        /// <summary>
        /// Creates a builder object that can help you build a custom dialog.
        /// <br>When finished setting up your properties on the builder, call builder.Create();</br>
        /// <br>This will return a Dialog object which you can call .Register() on the register it into the game.</br>
        /// </summary>
        /// <param name="dialogName">A unique name for your dialog, if null a random guid will be assigned.</param>
        /// <returns></returns>
        public DialogBuilder Builder(string dialogName = null)
        {
            return new DialogBuilder(dialogName);
        }

        /// <summary>
        /// Force a citizen to say a specific dds entry from a dialog.
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="ddsEntryRef"></param>
        /// <param name="endsDialog"></param>
        public void Speak(Citizen citizen, Guid blockId, bool endsDialog)
        {
            SpeakInternal(citizen, blockId.ToString(), endsDialog);
        }

        /// <summary>
        /// Force a citizen to say a specific text string.
        /// </summary>
        /// <param name="citizen"></param>
        /// <param name="customText"></param>
        /// <param name="endsDialog"></param>
        public void Speak(Citizen citizen, string customText, bool endsDialog)
        {
            // Get unique code for the text value
            var fnv = Lib.SaveGame.GetUniqueString(customText);

            // Add to dds cache
            if (!Lib.DdsStrings.Exists("dds.blocks", fnv))
                Lib.DdsStrings["dds.blocks", fnv] = customText;

            SpeakInternal(citizen, fnv, endsDialog);
        }

        private static void SpeakInternal(Citizen citizen, string blockId, bool endsDialog)
        {
            citizen.speechController.Speak("dds.blocks", blockId,
                false,
                false,
                true,
                0f,
                false,
                default,
                null,
                false,
                false,
                null,
                null,
                null,
                null,
                null
            );
        }
    }
}

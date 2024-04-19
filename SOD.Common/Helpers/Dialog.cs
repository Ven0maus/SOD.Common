using SOD.Common.Extensions;
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
    }
}

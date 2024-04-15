using System;

namespace SOD.Common.Helpers.DialogObjects
{
    /// <summary>
    /// An object that contains the response of a dialog.
    /// </summary>
    public class DialogResponse
    {
        /// <summary>
        /// Unique Guid for DDS structure.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// The text content of the dialog.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The condition for when this dialog/response should be shown.
        /// </summary>
        public BlockCondition Condition { get; } = new BlockCondition();

        /// <summary>
        /// You can manipulate this object to manipulate the response.
        /// <br>The dictionaryString and ddsMessageID are filled in automatically.</br>
        /// </summary>
        public AIActionPreset.AISpeechPreset ResponseInfo { get; }

        /// <summary>
        /// The block that corresponds to this dialog.
        /// </summary>
        internal DDSSaveClasses.DDSBlockSave Block { get; }

        internal DialogResponse(string text)
        {
            Id = Guid.NewGuid().ToString();
            Text = text;

            // The internal one doesn't use response info.

            // Create dds block
            Block = new DDSSaveClasses.DDSBlockSave
            {
                id = Id,
                name = Id
            };
        }

        public DialogResponse(string name, string text)
            : this(text)
        {
            ResponseInfo = new AIActionPreset.AISpeechPreset
            {
                dictionaryString = name,
                endsDialog = true,
                isSuccessful = true,
                useParsing = true,
                chance = 1,
            };
        }
    }
}

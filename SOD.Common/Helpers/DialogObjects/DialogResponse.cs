using System;

namespace SOD.Common.Helpers.DialogObjects
{
    /// <summary>
    /// An object that contains the response of a dialog.
    /// </summary>
    public class DialogResponse
    {
        /// <summary>
        /// The ID used for the block.
        /// </summary>
        public string BlockId { get; }
        /// <summary>
        /// The text content of the dialog.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// The ID used for the message.
        /// </summary>
        public Guid? MessageId { get; }

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

        /// <summary>
        /// Internal constructor for the DialogObject, cannot be used by library users.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="text"></param>
        internal DialogResponse(Guid? messageId, string text)
        {
            MessageId = messageId;
            BlockId = Guid.NewGuid().ToString();
            Text = text;

            // Create dds block
            Block = new DDSSaveClasses.DDSBlockSave
            {
                id = BlockId,
                name = BlockId
            };
        }

        /// <summary>
        /// A more advanced constructor, which allows to set a custom guid that you can use to raise the message by code.
        /// <br>This response will NOT be automatically triggered by the dialog.</br>
        /// <br>An example of raising it in code: "citizen.speechController.Speak(messageId.ToString())"</br>
        /// <br>If you wanna use the "citizen.speechController.Speak(dictionary, entryRef, ..);" overload, you must pass "dds.blocks" as dictionary and the BlockId of this object as entryRef.</br>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="messageId"></param>
        public DialogResponse(string text, Guid messageId)
            : this(messageId, text)
        { }

        /// <summary>
        /// Constructor for a response object, which is automatically triggered by the dialog.
        /// <br>Contains a lambda to modify the dialog option directly from the constructor.</br>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="action"></param>
        public DialogResponse(string text, Action<AIActionPreset.AISpeechPreset> dialogOptions = null, Action<BlockCondition> dialogCondition = null)
            : this(null, text)
        {
            ResponseInfo = new AIActionPreset.AISpeechPreset
            {
                dictionaryString = Guid.NewGuid().ToString(),
                endsDialog = true,
                isSuccessful = true,
                useParsing = true,
                chance = 1,
            };
            dialogOptions?.Invoke(ResponseInfo);
            dialogCondition?.Invoke(Condition);
        }

        /// <summary>
        /// Most basic constructor for a response object, which is automatically triggered by the dialog.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isSuccesful"></param>
        /// <param name="endsDialog"></param>
        public DialogResponse(string text, bool isSuccesful, bool endsDialog = true)
            : this(text, (a) =>
            {
                a.isSuccessful = isSuccesful;
                a.endsDialog = endsDialog;
            })
        { }
    }
}

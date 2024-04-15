﻿using System;
using System.Collections.Generic;

namespace SOD.Common.Helpers.DialogObjects
{
    /// <summary>
    /// An object that holds information about a DDS setup.
    /// </summary>
    public sealed class DialogObject : DialogResponse
    {
        private readonly List<DDSSaveClasses.DDSMessageSave> _messages = new();
        internal IReadOnlyList<DDSSaveClasses.DDSMessageSave> Messages => _messages;

        private readonly List<DDSSaveClasses.DDSBlockSave> _blocks = new();
        internal IReadOnlyList<DDSSaveClasses.DDSBlockSave> Blocks => _blocks;

        /// <summary>
        /// True if the dialog is available in game.
        /// </summary>
        public bool RegisteredInGame
        {
            get
            {
                return Lib.Dialog.RegisteredDialogs.Contains(this);
            }
        }

        /// <summary>
        /// The dialog preset that is used in the game, this will be automatically created.
        /// </summary>
        public DialogPreset Preset { get; private set; }

        /// <summary>
        /// Responses to this dialog.
        /// </summary>
        public List<DialogResponse> Responses { get; }

        /// <summary>
        /// The logic that is to be executed for this dialog.
        /// </summary>
        public IDialogLogic DialogLogic { get; set; }

        /// <summary>
        /// This one is set to new, so it is now visible to the lib user.
        /// Since response info is only useful for responses.
        /// </summary>
        internal new AIActionPreset.AISpeechPreset ResponseInfo { get; }

        internal DialogObject(DialogPreset dialogPreset, string text, IDialogLogic dialogLogic) 
            : base(text)
        {
            Responses = new();
            DialogLogic = dialogLogic;
            Preset = dialogPreset;
        }

        private void Build_DDS_Blocks(DialogResponse info)
        {
            _blocks.Add(info.Block);

            // Process responses
            foreach (var response in Responses)
                _blocks.Add(response.Block);
        }

        private void Build_DDS_Messages(DialogResponse info)
        {
            var message = new DDSSaveClasses.DDSMessageSave
            {
                id = Guid.NewGuid().ToString(),
                name = info.Id
            };
            var block = new DDSSaveClasses.DDSBlockCondition { blockID = info.Id, instanceID = Guid.NewGuid().ToString() };
            message.blocks.Add(block);

            // Copy over the conditions
            block.alwaysDisplay = info.Condition.alwaysDisplay;
            block.group = info.Condition.group;
            block.traitConditions = info.Condition.traitConditions;
            block.traits = info.Condition.traits;
            block.useTraits = info.Condition.useTraits;

            _messages.Add(message);

            // Process children further down
            if (info.Id != Id) return;
            foreach (var response in Responses)
                Build_DDS_Messages(response);
        }

        private void Build_Preset()
        {
            if (Messages.Count == 0)
                throw new Exception("Cannot register a dialog with no content.");

            // First message is the main parent
            Preset.msgID = Messages[0].id;

            for (int i=0; i < Responses.Count; i++)
            {
                var response = Responses[i];
                // Start counting from 1, because 0 is the parent
                response.ResponseInfo.ddsMessageID = _messages[i + 1].id;
                Preset.responses.Add(response.ResponseInfo);
            }
        }

        public void Register()
        {
            if (RegisteredInGame) return;

            // Build the dds structure and the preset
            Build_DDS_Blocks(this);
            Build_DDS_Messages(this);
            Build_Preset();

            // Now register the dialog into the game
            Lib.Dialog.RegisteredDialogs.Add(this);
        }
    }
}
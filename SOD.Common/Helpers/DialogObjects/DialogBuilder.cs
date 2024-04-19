using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Common.Helpers.DialogObjects
{
    /// <summary>
    /// The dialog builder object which handles creation of the dialog object.
    /// </summary>
    public sealed class DialogBuilder
    {
        private readonly DialogPreset _dialogPreset;
        private string _text;
        private IDialogLogic _dialogLogic;
        private List<DialogResponse> _dialogResponses;

        internal DialogBuilder(string dialogName = null) 
        {
            _dialogPreset = ScriptableObject.CreateInstance<DialogPreset>();

            // Base preset information
            _dialogPreset.name = dialogName ?? Guid.NewGuid().ToString();
            _dialogPreset.defaultOption = true;
            _dialogPreset.tiedToKey = Evidence.DataKey.photo;
            _dialogPreset.useSuccessTest = true;
            _dialogPreset.baseChance = 1;
            _dialogPreset.removeAfterSaying = false;
            _dialogPreset.ranking = 1;
        }

        /// <summary>
        /// Set's the main text of the dialog.
        /// </summary>
        /// <param name="text"></param>
        public DialogBuilder SetText(string text)
        {
            _text = text;
            return this;
        }

        /// <summary>
        /// Set's the dialog logic.
        /// </summary>
        /// <param name="dialogLogic"></param>
        public DialogBuilder SetDialogLogic(IDialogLogic dialogLogic)
        {
            _dialogLogic = dialogLogic;
            return this;
        }

        /// <summary>
        /// Add's a new custom dialog response to the dialog.
        /// </summary>
        /// <param name="response"></param>
        public DialogBuilder AddResponse(DialogResponse response)
        {
            _dialogResponses ??= new();
            _dialogResponses.Add(response);
            return this;
        }

        /// <summary>
        /// Add's a new response to the dialog with an optional lambda to modify the response preset info.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="action">The action to modify the response preset info</param>
        public DialogBuilder AddResponse(string text, Action<AIActionPreset.AISpeechPreset> action = null)
            => AddResponse(new DialogResponse(text, action));

        /// <summary>
        /// Add's a new response to the dialog.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isSuccesful">Is this a succes response?</param>
        /// <param name="endsDialog">Should the dialog end with this response?</param>
        public DialogBuilder AddResponse(string text, bool isSuccesful, bool endsDialog = true)
            => AddResponse(new DialogResponse(text, isSuccesful, endsDialog));

        /// <summary>
        /// Allows modificiation of the dialog options.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public DialogBuilder ModifyDialogOptions(Action<DialogPreset> action)
        {
            action.Invoke(_dialogPreset);
            return this;
        }

        /// <summary>
        /// Creates the new dialog based on the current builder configuration.
        /// </summary>
        /// <returns></returns>
        public DialogObject Create()
        {
            var dialogObject = new DialogObject(_dialogPreset, _text, _dialogLogic);
            if (_dialogResponses != null)
                dialogObject.Responses.AddRange(_dialogResponses);
            return dialogObject;
        }

        /// <summary>
        /// Creates and register the new dialog based on the current builder configuration.
        /// </summary>
        /// <param name="registrationOptions"></param>
        /// <returns></returns>
        public DialogObject CreateAndRegister()
        {
            var dialogObject = Create();
            dialogObject.Register();
            return dialogObject;
        }
    }
}

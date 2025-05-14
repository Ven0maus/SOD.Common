﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniverseLib.Utility;

namespace SOD.Common.Helpers
{
    public sealed class GameMessage
    {
        internal GameMessage() { }
        /// <summary>
        /// Broadcasts a message to be shown on the screen.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="delay"></param>
        public void Broadcast(
            string message,
            InterfaceController.GameMessageType messageType =
                InterfaceController.GameMessageType.notification,
            InterfaceControls.Icon icon = InterfaceControls.Icon.empty,
            UnityEngine.Color? color = null,
            float delay = 0f
        )
        {
            if (color == null)
                color = InterfaceControls.Instance.messageRed;
            InterfaceController.Instance.NewGameMessage(
                newType: messageType,
                newNumerical: 0,
                newMessage: message,
                newIcon: icon,
                colourOverride: true,
                col: color.Value,
                newMessageDelay: delay
            );

            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Broadcasted message \"{message}\".");
        }

        private readonly HashSet<string> _queuedPlayerSpeechMessages = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Queues a message on the bottom of the screen resembling the player's internal monologue, like seen in the tutorial during the "wake up" section.
        /// TODO: Use a less hacky method once we figure out custom DDS strings.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="durationSec">How long to show the message for.</param>
        /// <param name="doNotQueueMultiple">If true, do not queue the message if it is currently being shown or if it is already queued.</param>
        public void ShowPlayerSpeech(
            string message,
            float durationSec,
            bool doNotQueueMultiple = true
        )
        {
            if (doNotQueueMultiple && _queuedPlayerSpeechMessages.Contains(message))
            {
                return;
            }
            _queuedPlayerSpeechMessages.Add(message);
            Player.Instance.speechController.Speak(
                "",
                "temp",
                false,
                false,
                true,
                0f,
                true,
                Color.white,
                null,
                false,
                false,
                null,
                null,
                null,
                null, 
                null
            );
            UniverseLib.RuntimeHelper.StartCoroutine(
                ShowPlayerSpeechWaitForBubble(message, durationSec)
            );

            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Showed player speech message \"{message}\".");
        }

        private IEnumerator ShowPlayerSpeechWaitForBubble(string message, float durationSec)
        {
            var speechController = Player.Instance.speechController;
            while (
                speechController.activeSpeechBubble.IsNullOrDestroyed(true)
                || speechController.activeSpeechBubble.actualString != String.Empty
            )
            {
                yield return new WaitForEndOfFrame();
            }
            var bubble = speechController.activeSpeechBubble;
            bubble.actualString = message;
            bubble.oncreenTime = durationSec;
            while (!speechController.activeSpeechBubble.IsNullOrDestroyed(true) && speechController.activeSpeechBubble.actualString == message)
            {
                yield return new WaitForEndOfFrame();
            }
            _queuedPlayerSpeechMessages.Remove(message);
        }
    }
}

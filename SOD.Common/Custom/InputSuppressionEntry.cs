using System;
using System.Collections;
using BepInEx;
using Rewired;
using UnityEngine;
using UniverseLib;

namespace SOD.Common.Custom
{
    public sealed class InputSuppressionEntry
    {
        public InputSuppressionEntry(string callerGuid, KeyCode keyCode, TimeSpan? time = null)
        {
            CallerGuid = callerGuid;
            KeyCode = keyCode;
            InteractionKey = InteractablePreset.InteractionKey.none;
            ElementIdentifierName = string.Empty;
            ElementIdentifierId = -1;
            TimeRemainingSec = (float)(time?.TotalSeconds ?? 0d);
            IsSetOnInteractionKey = false;
        }

        public InputSuppressionEntry(string callerGuid, InteractablePreset.InteractionKey interactionKey, TimeSpan? time = null)
        {
            CallerGuid = callerGuid;
            InteractionKey = interactionKey;
            var binding = Lib.InputDetection.GetBinding(interactionKey);
            ElementIdentifierName = binding.elementIdentifierName;
            ElementIdentifierId = binding.elementIdentifierId;
            KeyCode = Lib.InputDetection.GetApproximateKeyCode(binding);
            TimeRemainingSec = (float)(time?.TotalSeconds ?? 0d);
            IsSetOnInteractionKey = true;
        }

        public KeyCode KeyCode { get; }
        public string ElementIdentifierName { get; }
        public int ElementIdentifierId { get; }
        public string CallerGuid { get; }
        public InteractablePreset.InteractionKey InteractionKey { get; }
        public bool IsSetOnInteractionKey { get; }
        public float TimeRemainingSec { get; set; }

        private Coroutine _coroutine;

        public void Start()
        {
            if (_coroutine != null || TimeRemainingSec <= 0f)
                return;
            _coroutine = RuntimeHelper.StartCoroutine(Tick());
        }

        public void Stop()
        {
            if (_coroutine == null) return;
            RuntimeHelper.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        internal string ConvertToDictionaryKey()
        {
            return Lib.InputDetection.ConvertToDictionaryKey(CallerGuid, InteractionKey, KeyCode);
        }

        private IEnumerator Tick()
        {
            while (TimeRemainingSec > 0f)
            {
                yield return new WaitForEndOfFrame();

                if (!Lib.Time.IsInitialized || Lib.Time.IsGamePaused || Lib.SaveGame.IsSaving)
                    continue;

                float deltaTime = Time.deltaTime;
                TimeRemainingSec -= deltaTime;
            }

            TimeRemainingSec = 0f;
            Lib.InputDetection.InputSuppressionDictionary.Remove(ConvertToDictionaryKey());
            _coroutine = null;
        }

        /// <summary>
        /// Used to store the data into json object
        /// </summary>
        internal class JsonData
        {
            public string CallerGuid { get; set; }
            public KeyCode KeyCode { get; set; }
            public InteractablePreset.InteractionKey InteractionKey { get; set; }
            public float Time { get; set; }
            public bool IsSetOnInteractionKey { get; set; }
        }
    }
}
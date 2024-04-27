using System;
using System.Collections;
using UnityEngine;
using UniverseLib;

namespace SOD.Common.Custom
{
    internal sealed class IllegalStatusModifier
    {
        public IllegalStatusModifier(string key, TimeSpan? time = null)
        {
            Key = key;
            TimeRemainingSec = (float)(time?.TotalSeconds ?? 0d);
        }

        public string Key { get; }
        public float TimeRemainingSec { get; set; }

        private Coroutine _coroutine;

        public void Start()
        {
            if (_coroutine != null || TimeRemainingSec <= 0) return;
            _coroutine = RuntimeHelper.StartCoroutine(Tick());
        }

        public void Stop()
        {
            if (_coroutine == null) return;
            RuntimeHelper.StopCoroutine(_coroutine);
            _coroutine = null;
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
            if (Lib.PlayerStatus.IllegalStatusModifierDictionary != null)
                Lib.PlayerStatus.IllegalStatusModifierDictionary.Remove(Key);
            Lib.PlayerStatus.UpdatePlayerIllegalStatus();
            _coroutine = null;
        }

        /// <summary>
        /// Used to store the data into json object
        /// </summary>
        public class JsonData
        {
            public string Key { get; set; }
            public float Time { get; set; }
        }
    }
}
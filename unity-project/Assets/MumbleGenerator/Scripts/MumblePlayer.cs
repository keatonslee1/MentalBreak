using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MumbleGenerator
{
    public class MumblePlayer : MonoBehaviour
    {
        public Mumble Mumble;

        MumbleManager _mumbleManager;

        AudioSource _audioSourceOne;
        AudioSource _audioSourceTwo;

        IEnumerator _mumbleCoroutine;

        public event Action OnStartMumble;
        public event Action OnEndMumble;
        public event Action<string> OnLetter;

        void Awake()
        {
            _mumbleManager = MumbleManager.Instance;
            _audioSourceOne = gameObject.AddComponent<AudioSource>();
            _audioSourceTwo = gameObject.AddComponent<AudioSource>();
        }

        public void PlayMumble()
        {
            _audioSourceOne.pitch = Mumble.Pitch;
            _audioSourceTwo.pitch = Mumble.Pitch;

            _audioSourceOne.volume = Mumble.Volume;
            _audioSourceTwo.volume = Mumble.Volume;

            if (_mumbleCoroutine != null) StopCoroutine(_mumbleCoroutine);
            _mumbleCoroutine = MumbleCoroutine();
            StartCoroutine(_mumbleCoroutine);
        }

        IEnumerator MumbleCoroutine()
        {
            List<MumbleSound> sounds = _mumbleManager.GetMumblings(Mumble);

            OnStartMumble?.Invoke();
            bool isSecond = false; // bouncing audio sources to deal with sound crackling
            int count = 1; // skipping some letters if speed is high
            float speed = Mathf.RoundToInt(Mumble.Speed);
            foreach (MumbleSound sound in sounds)
            {
                OnLetter?.Invoke(sound.Letter);

                if (sound.Clip == null) continue;

                AudioSource currentSource = _audioSourceOne;
                if (isSecond) currentSource = _audioSourceTwo;
                isSecond = !isSecond;

                if (count >= speed)
                {
                    count = 0;
                    currentSource.pitch = Mumble.Pitch * sound.PitchChange;
                    currentSource.PlayOneShot(sound.Clip);
                }

                count++;

                yield return new WaitForSeconds(sound.Clip.length / Mumble.Pitch / Mumble.Speed);
            }

            OnEndMumble?.Invoke();
        }
    }
}
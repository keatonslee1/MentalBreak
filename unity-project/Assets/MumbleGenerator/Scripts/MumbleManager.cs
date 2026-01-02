using System;
using System.Collections.Generic;
using MumbleGenerator.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MumbleGenerator
{
    // https://tntc-lab.itch.io/godot-voice-generator
    // https://github.com/equalo-official/animalese-generator/blob/master/animalese.py
    public class MumbleManager : Singleton<MumbleManager>
    {
        public Animalese[] Animalese;

        readonly Dictionary<string, AudioClip> _animaleseDictionary = new();
        AudioClip _shClip;
        AudioClip _thClip;

        void Start()
        {
            PopulateAnimalese();
        }

        void PopulateAnimalese()
        {
            foreach (Animalese a in Animalese)
            {
                _animaleseDictionary.Add(a.Letter, a.Sound);
                if (a.Letter == "sh") _shClip = a.Sound;
                if (a.Letter == "th") _thClip = a.Sound;
            }
        }

        public List<MumbleSound> GetMumblings(Mumble mumble)
        {
            if (mumble == null)
            {
                Debug.LogError("Mumble is null");
                return null;
            }

            return mumble.LanguageType == LanguageType.Simple ? SimpleSoundText(mumble) : AnimalizeText(mumble);
        }

        List<MumbleSound> SimpleSoundText(Mumble mumble)
        {
            List<MumbleSound> result = new List<MumbleSound>();

            AudioClip clip = GetSimpleSoundClip(mumble);

            string lower = mumble.Text.ToLower();
            float pitch = 1;
            for (int i = mumble.Text.Length - 1; i >= 0; i--)
            {
                pitch = GetPitch(pitch, lower[i], mumble);

                if (IsCharWhitespaceOrPunctuation(lower[i]))
                    result.Add(new(mumble.Text.Substring(i, 1), null, pitch));

                if (char.IsLetter(lower[i]) || char.IsDigit(lower[i]))
                    result.Add(new(mumble.Text.Substring(i, 1), clip, pitch));
            }

            result.Reverse(); // we were looping backwards to handle ? and ! pitch increase easier
            return result;
        }

        List<MumbleSound> AnimalizeText(Mumble mumble)
        {
            List<MumbleSound> result = new List<MumbleSound>();

            AudioClip digitClip = GetAnimaleseDigitAudioClip(mumble);

            string lower = mumble.Text.ToLower();
            float pitch = 1;
            for (int i = mumble.Text.Length - 1; i >= 0; i--)
            {
                pitch = GetPitch(pitch, lower[i], mumble);

                if (IsCharShTh(i, lower, out AudioClip clip))
                {
                    result.Add(new(mumble.Text.Substring(i - 1, 2), clip, pitch));
                    i--;
                    continue;
                }

                // skip repeat letters
                if (IsRepeat(i, lower))
                {
                    result.Add(new(mumble.Text.Substring(i, 1), null, pitch));
                    continue;
                }

                // white space & punctuation
                if (IsCharWhitespaceOrPunctuation(lower[i]))
                    result.Add(new(mumble.Text.Substring(i, 1), null, pitch));

                // normal letter
                if (char.IsLetter(lower[i]))
                    result.Add(new(mumble.Text.Substring(i, 1), _animaleseDictionary[lower[i].ToString()], pitch));

                // digits
                if (char.IsDigit(lower[i]))
                    result.Add(new(mumble.Text.Substring(i, 1), digitClip, pitch));
            }

            result.Reverse();
            return result;
        }

        bool IsCharShTh(int i, string lower, out AudioClip clip)
        {
            clip = null;

            if (i == 0) return false;
            if (lower[i] != 'h') return false;

            switch (lower[i - 1])
            {
                case 's':
                    clip = _shClip;
                    return true;
                case 't':
                    clip = _thClip;
                    return true;
                default:
                    return false;
            }
        }

        bool IsRepeat(int i, string lower)
        {
            if (i == 0) return false;
            return lower[i] == lower[i - 1];
        }

        AudioClip GetAnimaleseDigitAudioClip(Mumble mumble)
        {
            return _animaleseDictionary.ContainsKey(mumble.DigitLetterBase.ToString().ToLower())
                ? _animaleseDictionary[mumble.DigitLetterBase.ToString().ToLower()]
                : _animaleseDictionary["a"];
        }

        AudioClip GetSimpleSoundClip(Mumble mumble)
        {
            if (mumble.CustomClip != null) return mumble.CustomClip;

            return _animaleseDictionary.ContainsKey(mumble.LetterBase.ToString().ToLower())
                ? _animaleseDictionary[mumble.LetterBase.ToString().ToLower()]
                : _animaleseDictionary["a"];
        }

        float GetPitch(float pitch, char c, Mumble mumble)
        {
            float p = pitch;

            // coming back to 1
            float pitchComebackRate = mumble.PitchChangeOnExclamation * 0.02f;
            float topBand = 1 + pitchComebackRate;
            float lowBand = 1 - pitchComebackRate;

            if (pitch > 1) p -= pitchComebackRate;
            if (pitch < 1) p += pitchComebackRate;

            if (IsCharExclamationOrQuestion(c))
                p = mumble.PitchChangeOnExclamation;

            // adding variance if pitch is close to one
            if (pitch > lowBand && pitch < topBand)
                p *= Random.Range(1 - mumble.PitchVariance, 1 + mumble.PitchVariance);

            return p;
        }

        bool IsCharWhitespaceOrPunctuation(char c)
        {
            return c == ',' || c == '.' || c == '?' || c == '!' || char.IsWhiteSpace(c);
        }

        bool IsCharExclamationOrQuestion(char c)
        {
            return c is '?' or '!';
        }
    }

    [Serializable]
    public struct Animalese
    {
        public string Letter;
        public AudioClip Sound;
    }

    public struct MumbleSound
    {
        public readonly string Letter;
        public readonly AudioClip Clip;
        public readonly float PitchChange;

        public MumbleSound(string letter, AudioClip clip, float pitchChange)
        {
            Letter = letter;
            Clip = clip;
            PitchChange = pitchChange;
        }
    }

    public enum LanguageType
    {
        Animalese,
        Simple
    }

    public enum LetterBase
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        Th,
        Sh,
    }
}
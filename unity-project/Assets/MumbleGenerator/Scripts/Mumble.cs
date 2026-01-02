using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MumbleGenerator
{
    [CreateAssetMenu(fileName = "Mumble", menuName = "ScriptableObjects/Mumble")]
    public class Mumble : ScriptableObject
    {
        [TextArea]
        public string Text =
            "Hello I am reading some text. This is exclamation! Is this question? The quick brown fox jumps over the lazy dog.";

        public float Volume = 1f;
        public float Pitch = 2f;
        public float PitchVariance = 0.05f;
        public float Speed = 1f;
        public float PitchChangeOnExclamation = 1.2f;

        public LanguageType LanguageType;

        public LetterBase LetterBase; // simple
        public AudioClip CustomClip; // simple

        public LetterBase DigitLetterBase; // animalese

        public void Randomize()
        {
            Pitch = Random.Range(0.1f, 3f);
            PitchVariance = Random.Range(0.1f, 0.3f);
            Speed = Random.Range(1f, 3f);
            PitchChangeOnExclamation = Random.Range(1.1f, 1.4f);

            if (LanguageType == LanguageType.Simple)
                LetterBase = (LetterBase)Random.Range(0, Enum.GetValues(typeof(LetterBase)).Length);

            if (LanguageType == LanguageType.Animalese)
                DigitLetterBase = (LetterBase)Random.Range(0, Enum.GetValues(typeof(LetterBase)).Length);
        }
    }
}
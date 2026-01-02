using UnityEngine;
using UnityEngine.UIElements;

namespace MumbleGenerator
{
    public class TextPrinter : MonoBehaviour
    {
        Label _textLabel;

        void Start()
        {
            _textLabel = GetComponent<UIDocument>().rootVisualElement.Q<Label>("text");

            GetComponent<MumblePlayer>().OnStartMumble += OnStartMumble;
            GetComponent<MumblePlayer>().OnLetter += OnLetter;
        }

        void OnLetter(string c)
        {
            _textLabel.text += c;
        }

        void OnStartMumble()
        {
            _textLabel.text = "";
        }
    }
}
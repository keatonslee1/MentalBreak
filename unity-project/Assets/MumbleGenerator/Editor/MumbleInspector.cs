using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MumbleGenerator.Editor
{
    [CustomEditor(typeof(Mumble))]
    public class MumbleInspector : UnityEditor.Editor
    {
        Mumble _mumble;
        VisualElement _languageOptionsContainer;

        public override VisualElement CreateInspectorGUI()
        {
            _mumble = target as Mumble;

            VisualElement myInspector = new();

            AddHeader(myInspector);
            AddTextField(myInspector);
            AddVolumeField(myInspector);
            AddPitchField(myInspector);
            AddPitchVarianceField(myInspector);
            AddSpeedField(myInspector);
            AddPitchChangeOnExclamation(myInspector);
            AddLanguageChooser(myInspector);
            AddLanguageOptions(myInspector);
            ResolveLanguageOptions(null);
            AddRandomizeButton(myInspector);

            return myInspector;
        }

        void AddRandomizeButton(VisualElement myInspector)
        {
            Button randomizeButton = new();
            randomizeButton.text = "Randomize";
            randomizeButton.clickable.clicked += () =>
            {
                _mumble.Randomize();

                if (_simpleLanguageLetterBaseField != null) _simpleLanguageLetterBaseField.value = _mumble.LetterBase;
                if (_animaleseDigitLetterBaseField != null)
                    _animaleseDigitLetterBaseField.value = _mumble.DigitLetterBase;
            };
            myInspector.Add(randomizeButton);
        }

        void ResolveLanguageOptions(ChangeEvent<Enum> _)
        {
            _languageOptionsContainer.Clear();
            if (_mumble.LanguageType == LanguageType.Simple)
                HandleSimpleLanguageOptions();

            if (_mumble.LanguageType == LanguageType.Animalese)
                HandleAnimaleseLanguageOptions();
        }

        EnumField _animaleseDigitLetterBaseField;

        void HandleAnimaleseLanguageOptions()
        {
            _languageOptionsContainer.tooltip = "What letter will be used for digit sounds.";
            _animaleseDigitLetterBaseField = new()
            {
                label = "Digit Letter Base"
            };
            _animaleseDigitLetterBaseField.Init(_mumble.DigitLetterBase);
            _animaleseDigitLetterBaseField.bindingPath = "DigitLetterBase";
            _languageOptionsContainer.Add(_animaleseDigitLetterBaseField);
        }

        EnumField _simpleLanguageLetterBaseField;

        void HandleSimpleLanguageOptions()
        {
            _languageOptionsContainer.tooltip = "";

            _simpleLanguageLetterBaseField = new()
            {
                label = "Letter Base"
            };
            _simpleLanguageLetterBaseField.Init(_mumble.LetterBase);
            _simpleLanguageLetterBaseField.bindingPath = "LetterBase";
            _languageOptionsContainer.Add(_simpleLanguageLetterBaseField);

            ObjectField customClipField = new()
            {
                style =
                {
                    marginTop = 24
                },
                label = "Custom Clip",
                bindingPath = "CustomClip",
                objectType = typeof(AudioClip)
            };

            customClipField.RegisterValueChangedCallback((evt) =>
            {
                _mumble.CustomClip = (AudioClip)evt.newValue;
                customClipField.value = _mumble.CustomClip;
            });
            if (_mumble.CustomClip != null)
                customClipField.value = _mumble.CustomClip;

            _languageOptionsContainer.Add(customClipField);

            Label newLabel = new()
            {
                style =
                {
                    marginLeft = 5
                },
                text = "Custom clip takes precedence over letter base."
            };
            _languageOptionsContainer.Add(newLabel);
        }

        void AddLanguageOptions(VisualElement myInspector)
        {
            _languageOptionsContainer = CreateContainer("");
            myInspector.Add(_languageOptionsContainer);
        }

        void AddLanguageChooser(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Language Type"
            };
            container.Add(label);

            EnumField field = new();
            field.Init(_mumble.LanguageType);
            field.bindingPath = "LanguageType";
            container.Add(field);
            field.RegisterCallback<ChangeEvent<Enum>>(ResolveLanguageOptions);
        }

        void AddPitchChangeOnExclamation(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Pitch Change On Exclamation and Question"
            };
            container.Add(label);

            Slider slider = new()
            {
                bindingPath = "PitchChangeOnExclamation",
                lowValue = 0f,
                highValue = 2f,
                showInputField = true
            };

            container.Add(slider);
        }

        void AddSpeedField(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("Sounds will be skipped on higher speeds");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Speed"
            };
            container.Add(label);

            Slider slider = new()
            {
                bindingPath = "Speed",
                lowValue = 0.1f,
                highValue = 10f,
                showInputField = true
            };

            container.Add(slider);
        }

        void AddPitchVarianceField(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("Rate applied to pitch for sound variety");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Pitch Variance"
            };
            container.Add(label);

            Slider slider = new()
            {
                bindingPath = "PitchVariance",
                lowValue = 0f,
                highValue = 0.5f,
                showInputField = true
            };

            container.Add(slider);
        }

        void AddVolumeField(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Volume"
            };
            container.Add(label);

            Slider slider = new()
            {
                bindingPath = "Volume",
                lowValue = 0f,
                highValue = 1f,
                showInputField = true
            };

            container.Add(slider);
        }

        void AddPitchField(VisualElement myInspector)
        {
            VisualElement container = CreateContainer("Affects speed");
            myInspector.Add(container);

            Label label = new()
            {
                text = "Pitch"
            };
            container.Add(label);

            Slider slider = new()
            {
                bindingPath = "Pitch",
                lowValue = 0.1f,
                highValue = 10f,
                showInputField = true
            };

            container.Add(slider);
        }

        static void AddTextField(VisualElement myInspector)
        {
            TextField textField = new()
            {
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                    marginBottom = 25,
                    whiteSpace = WhiteSpace.Normal
                },
                multiline = true,
                bindingPath = "Text"
            };

            myInspector.Add(textField);
        }

        static void AddHeader(VisualElement myInspector)
        {
            Label header = new()
            {
                text = "Mumble",
                style =
                {
                    fontSize = 18
                }
            };

            myInspector.Add(header);
        }

        VisualElement CreateContainer(string tooltip)
        {
            VisualElement container = new()
            {
                tooltip = tooltip,
                style =
                {
                    marginBottom = 25
                }
            };
            return container;
        }
    }
}
using UnityEngine;
using UnityEngine.UIElements;

namespace MumbleGenerator
{
    public class PlayButton : MonoBehaviour
    {
        void Start()
        {
            GetComponent<UIDocument>().rootVisualElement.Q<Button>("playButton").clickable.clicked += () =>
            {
                GetComponent<MumbleSequencer>().PlayNextMumble();
            };
        }
    }
}
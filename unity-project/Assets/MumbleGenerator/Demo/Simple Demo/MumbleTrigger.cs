using UnityEngine;

namespace MumbleGenerator.Simple_Demo
{
    public class MumbleTrigger : MonoBehaviour
    {
        void Start()
        {
            GetComponent<MumblePlayer>().PlayMumble();
        }
    }
}

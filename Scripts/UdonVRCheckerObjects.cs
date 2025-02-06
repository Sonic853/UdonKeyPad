
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class UdonVRCheckerObjects : UdonSharpBehaviour
    {
        public GameObject[] pcObjects;
        public GameObject[] vrObjects;
        void Start()
        {
            var isVr = Networking.LocalPlayer.IsUserInVR();
            foreach (var obj in pcObjects)
            {
                obj.SetActive(!isVr);
            }
            foreach (var obj in vrObjects)
            {
                obj.SetActive(isVr);
            }
        }
    }
}

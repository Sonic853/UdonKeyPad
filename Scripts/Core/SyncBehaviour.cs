
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sonic853.Udon.UdonKeypad
{
    public class SyncBehaviour : UdonSharpBehaviour
    {
        protected bool isSynced = false;
        protected virtual void Start()
        {
            if (Networking.IsOwner(gameObject))
                OnDeserialization();
            else
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(NeedSync));
        }
        public void NeedSync()
        {
            if (!Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(NeedSync));
                return;
            }
            RequestSerialization_();
        }
        public void RequestSerialization_()
        {
            if (!isSynced) return;
            isSynced = false;
            RequestSerialization();
            if (Networking.IsOwner(gameObject))
                OnDeserialization();
        }
        public override void OnDeserialization()
        {
            isSynced = true;
        }
    }
}

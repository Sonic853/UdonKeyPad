
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class CardReader : UdonSharpBehaviour
    {
        public KeypadBase keypad;
        /// <summary>
        /// 声音
        /// </summary>
        [Header("声音")]
        public AudioSource audioSource;
        void OnTriggerEnter(Collider other)
        {
            var cardobj = other.gameObject.GetComponentInChildren(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            if (audioSource != null) audioSource.Play();
            var card = (KeyCard)cardobj;
            if (!card.valid)
            {
                if (keypad != null && keypad.isLocked)
                {
                    keypad.ButtonPushClear();
                    keypad.SetPlaceholder("Invalid");
                }
                return;
            }
            if (card.singleUse && card.isUsed)
            {
                if (keypad != null && keypad.isLocked)
                {
                    keypad.ButtonPushClear();
                    keypad.SetPlaceholder("Card Used");
                }
                return;
            }
            if (card.expireTime != -1 && (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds > card.expireTime)
            {
                if (keypad != null && keypad.isLocked)
                {
                    keypad.ButtonPushClear();
                    keypad.SetPlaceholder("Expired");
                }
                return;
            }
            if (keypad == null || !keypad.isLocked) { return; }
            keypad.ButtonPushClear();
            keypad.ButtonPush(card.passcode);
            if (!keypad._autoEnter) keypad.ButtonPushEnter();
            if (!keypad.isLocked)
            {
                card.isUsed = true;
            }
        }
    }
}

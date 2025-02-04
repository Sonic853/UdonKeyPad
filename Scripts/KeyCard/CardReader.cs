
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
        void OnTriggerEnter(Collider other)
        {
            var cardobj = other.gameObject.GetComponent(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            var card = (KeyCard)cardobj;
            if (!card.valid) { return; }
            if (card.singleUse && card.isUsed)
            {
                if (keypad != null && keypad.isLocked) 
                {
                    keypad.ButtonPushClear();
                    keypad.SetPlaceholder("Card Used");
                }
                return;
            }
            if (card.expireTime != -1 && (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds > card.expireTime) { return; }
            if (keypad == null || !keypad.isLocked) { return; }
            keypad.ButtonPushClear();
            keypad.ButtonPush(card.passcode);
            keypad.ButtonPushEnter();
            if (!keypad.isLocked)
            {
                card.isUsed = true;
            }
        }
    }
}

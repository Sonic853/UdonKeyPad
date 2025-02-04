
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class CardWriter : UdonSharpBehaviour
    {
        /// <summary>
        /// 卡片是否有效
        /// </summary>
        [Header("卡片是否有效")]
        public Toggle validUI;
        /// <summary>
        /// 卡片是否有效
        /// </summary>
        public bool valid => validUI.isOn;
        /// <summary>
        /// 密码
        /// </summary>
        [Header("密码")]
        public InputField passcodeUI;
        /// <summary>
        /// 密码
        /// </summary>
        public string passcode => passcodeUI.text;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        [Header("一次性卡片")]
        public Toggle singleUseUI;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        public bool singleUse => singleUseUI.isOn;
        /// <summary>
        /// 卡片有效时间
        /// </summary>
        [Header("卡片有效时间")]
        public InputField expireTimeUI;
        public int expireTime
        {
            get
            {
                if (int.TryParse(expireTimeUI.text.Trim(), out int result))
                    return result;
                return -1;
            }
        }
        KeyCard targetCard;
        public Text CardStatus;
        void OnTriggerEnter(Collider other)
        {
            var cardobj = other.gameObject.GetComponent(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            targetCard = (KeyCard)cardobj;
            var expireTimeText = targetCard.expireTime == -1 ? "Unlimited" : ConvertTime(targetCard.expireTime);
            if (CardStatus != null) CardStatus.text = $"Card Info:\nValid: {targetCard.valid}\nSingle Use: {targetCard.singleUse}\nIs Used: {targetCard.isUsed}\nExpire Time: {expireTimeText}";
        }
        void OnTriggerExit(Collider other)
        {
            var cardobj = other.gameObject.GetComponent(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            var card = (KeyCard)cardobj;
            if (card != targetCard) { return; }
            targetCard = null;
            if (CardStatus != null) CardStatus.text = "Please place the card on the reader";
        }
        public void WriteCard()
        {
            if (targetCard == null) { return; }
            targetCard.valid = valid;
            if (!string.IsNullOrWhiteSpace(passcode)) targetCard.passcode = passcode;
            targetCard.singleUse = singleUse;
            targetCard.isUsed = false;
            targetCard.expireTime = expireTime;
        }
        /// <summary>
        /// 转换时间
        /// </summary>
        /// <param name="expireTime">unix时间</param>
        /// <returns>hh时mm分ss秒</returns>
        string ConvertTime(int expireTime)
        {
            var time = new DateTime(1970, 1, 1).AddSeconds(expireTime);
            return time.ToString("HH:mm:ss");
        }
    }
}

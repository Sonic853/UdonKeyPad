
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class CardWriter : SyncBehaviour
    {
        /// <summary>
        /// 请求写入
        /// </summary>
        [UdonSynced]
        bool needWriteCard = false;
        /// <summary>
        /// 卡片是否有效
        /// </summary>
        [Header("卡片是否有效")]
        public Toggle validUI;
        /// <summary>
        /// 卡片是否有效
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(Valid))]
        bool valid = true;
        /// <summary>
        /// 卡片是否有效
        /// </summary>
        public bool Valid
        {
            get => valid;
            set
            {
                valid = value;
                validUI.SetIsOnWithoutNotify(value);
            }
        }
        /// <summary>
        /// 密码
        /// </summary>
        [Header("密码")]
        public InputField passcodeUI;
        /// <summary>
        /// 密码
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(Passcode))]
        string passcode = "";
        /// <summary>
        /// 密码
        /// </summary>
        public string Passcode
        {
            get => passcode;
            set
            {
                passcode = value;
                passcodeUI.text = value;
            }
        }
        /// <summary>
        /// 一次性卡片
        /// </summary>
        [Header("一次性卡片")]
        public Toggle singleUseUI;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(SingleUse))]
        bool singleUse = false;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        public bool SingleUse
        {
            get => singleUse;
            set
            {
                singleUse = value;
                singleUseUI.SetIsOnWithoutNotify(value);
            }
        }
        /// <summary>
        /// 卡片有效时间
        /// </summary>
        [Header("卡片有效时间")]
        public Dropdown expireTimeUI;
        DataDictionary expireTimeData = new DataDictionary()
        {
            { "Unlimited", -1 },
            { "30 seconds", 30 },
            { "1 minute", 60 },
            { "3 minutes", 180 },
            { "5 minutes", 300 },
            { "10 minutes", 600 },
            { "15 minutes", 900 },
            { "30 minutes", 1800 },
            { "1 hour", 3600 },
            { "3 hours", 10800 },
            { "6 hours", 21600 },
            { "12 hours", 43200 },
            { "1 day", 86400 },
        };
        DataList expireTimeList = new DataList()
        {
            -1,
            30,
            60,
            180,
            300,
            600,
            900,
            1800,
            3600,
            10800,
            21600,
            43200,
            86400,
        };
        /// <summary>
        /// 卡片有效时间
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(ExpireTime))]
        int expireTime = -1;
        /// <summary>
        /// 卡片有效时间
        /// </summary>
        public int ExpireTime
        {
            get => expireTime;
            // {
            //     if (!expireTimeData.TryGetValue(expireTimeUI.captionText.text, out var itemToken)) { return -1; }
            //     return itemToken.Int;
            // }
            set
            {
                expireTime = value;
                var index = expireTimeList.IndexOf(expireTime);
                if (index == -1) index = 0;
                expireTimeUI.SetValueWithoutNotify(index);
            }
        }
        /// <summary>
        /// 目标卡片
        /// </summary>
        KeyCard targetCard;
        /// <summary>
        /// 显示卡片信息
        /// </summary>
        [Header("显示卡片信息")]
        public Text CardStatus;
        /// <summary>
        /// 声音
        /// </summary>
        [Header("声音")]
        public AudioSource audioSource;
        void OnTriggerEnter(Collider other)
        {
            var cardobj = other.gameObject.GetComponentInChildren(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            targetCard = (KeyCard)cardobj;
            var targetCardExpireTime = targetCard.expireTime;
            var expireTimeText = targetCardExpireTime == -1 ? "Unlimited" : ConvertTime(targetCardExpireTime);
            var isExpired = targetCardExpireTime != -1 && (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds > targetCardExpireTime;
            if (CardStatus != null) CardStatus.text = $"Card Info:\nValid: {targetCard.valid}\nSingle Use: {targetCard.singleUse}\nIs Used: {targetCard.isUsed}\nExpire Time: {expireTimeText}\nIs Expired: {isExpired}";
        }
        void OnTriggerExit(Collider other)
        {
            var cardobj = other.gameObject.GetComponentInChildren(typeof(UdonSharpBehaviour));
            if (((UdonSharpBehaviour)cardobj).GetUdonTypeName() != "Sonic853.Udon.Keypad.KeyCard") { return; }
            var card = (KeyCard)cardobj;
            if (card != targetCard) { return; }
            targetCard = null;
            if (CardStatus != null) CardStatus.text = "Please place the card on the reader";
        }
        public void WriteCardLocal()
        {
            if (targetCard == null)
            {
                if (CardStatus != null) CardStatus.text = "Please place the card on the reader";
                return;
            }
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            Valid = validUI.isOn;
            SingleUse = singleUseUI.isOn;
            var time = -1;
            if (expireTimeData.TryGetValue(expireTimeUI.captionText.text, out var itemToken)) time = itemToken.Int;
            ExpireTime = time;
            Passcode = passcodeUI.text;
            if (targetCard.isGlobal)
            {
                needWriteCard = true;
                RequestSerialization_();
            }
            else
            {
                WriteCard();
            }
        }
        public void WriteCard()
        {
            if (targetCard == null)
            {
                if (CardStatus != null) CardStatus.text = "Please place the card on the reader";
                return;
            }
            targetCard.valid = Valid;
            if (!string.IsNullOrWhiteSpace(Passcode)) targetCard.passcode = Passcode;
            targetCard.singleUse = SingleUse;
            targetCard.isUsed = false;
            if (ExpireTime == -1) targetCard.expireTime = -1;
            else targetCard.expireTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + ExpireTime;
            if (CardStatus != null) CardStatus.text = "Write Success";
            if (audioSource != null) audioSource.Play();
        }
        /// <summary>
        /// 转换时间
        /// </summary>
        /// <param name="expireTime">unix时间</param>
        /// <returns>hh时mm分ss秒</returns>
        string ConvertTime(int expireTime)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(expireTime);
            return time.ToLocalTime().ToString("HH:mm:ss");
        }
        public override void OnDeserialization()
        {
            base.OnDeserialization();
            if (needWriteCard)
            {
                WriteCard();
                needWriteCard = false;
            }
        }
    }
}

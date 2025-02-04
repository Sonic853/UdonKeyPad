
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class KeyCard : UdonSharpBehaviour
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        [Header("是否有效")]
        public bool valid = true;
        /// <summary>
        /// 密码
        /// </summary>
        [Header("密码")]
        public string passcode;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        [Header("一次性卡片")]
        public bool singleUse = false;
        /// <summary>
        /// 是否已使用
        /// </summary>
        [NonSerialized] public bool isUsed = false;
        /// <summary>
        /// 卡片有效期（unix秒，-1为无限制）
        /// </summary>
        [Header("卡片有效期（unix秒，-1为无限制）")]
        public int expireTime = -1;
        // public long expireTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}

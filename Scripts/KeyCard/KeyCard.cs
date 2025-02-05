
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class KeyCard : SyncBehaviour
    {
        /// <summary>
        /// 全局卡
        /// </summary>
        [Header("全局卡")]
        [UdonSynced]
        public bool isGlobal = false;
        /// <summary>
        /// 是否有效
        /// </summary>
        [Header("是否有效")]
        [UdonSynced]
        public bool valid = true;
        /// <summary>
        /// 密码
        /// </summary>
        [Header("密码")]
        [UdonSynced]
        public string passcode;
        /// <summary>
        /// 一次性卡片
        /// </summary>
        [Header("一次性卡片")]
        [UdonSynced]
        public bool singleUse = false;
        /// <summary>
        /// 是否已使用
        /// </summary>
        [NonSerialized]
        [UdonSynced]
        public bool isUsed = false;
        /// <summary>
        /// 卡片有效期（unix秒，-1为无限制）
        /// </summary>
        [Header("卡片有效期（unix秒，-1为无限制）")]
        [UdonSynced]
        public int expireTime = -1;
        protected override void Start() {
            if (isGlobal && !Networking.IsOwner(gameObject))
            {
                base.Start();
                return;
            }
        }
    }
}

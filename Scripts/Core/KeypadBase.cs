﻿
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Udon.Keypad
{
    public class KeypadBase : SyncBehaviour
    {
        /// <summary>
        /// 全局锁
        /// </summary>
        [Header("全局锁")]
        public bool isGlobal = false;
        /// <summary>
        /// 密码
        /// </summary>
        [Header("密码")]
        public string Passcode;
        /// <summary>
        /// 是锁定的
        /// </summary>
        [Header("是锁定的")]
        [UdonSynced]
        public bool isLocked = true;
        /// <summary>
        /// 自动输入
        /// </summary>
        [Header("自动输入")]
        public bool _autoEnter = false;
        /// <summary>
        /// 解锁时显示的物体
        /// </summary>
        [Header("解锁时显示的物体")]
        public GameObject[] _lockHiedObjects;
        /// <summary>
        /// 锁定时显示的物体
        /// </summary>
        [Header("锁定时显示的物体")]
        public GameObject[] _lockShowObjects;
        /// <summary>
        /// 是否打乱按钮顺序
        /// </summary>
        [Header("是否打乱按钮顺序")]
        public bool _isRandomButton = false;
        /// <summary>
        /// 密码按钮
        /// </summary>
        [NonSerialized] protected GameObject[] Buttons = new GameObject[0];
        /// <summary>
        /// 启用玩家传送
        /// </summary>
        [Header("启用玩家传送")]
        public bool _enableTeleport = false;
        /// <summary>
        /// 全局玩家传送（Danger）全部玩家会传送到一个点上
        /// </summary>
        [Header("全局玩家传送 **Danger**")]
        public bool teleportIsGlobal = false;
        /// <summary>
        /// 使用传送点角度方向
        /// </summary>
        [Header("使用传送点角度方向")]
        public bool useTeleportPointRotation = true;
        /// <summary>
        /// 传送点
        /// </summary>
        [Header("传送点")]
        public Transform _teleportPoint;
        /// <summary>
        /// 启用白名单
        /// </summary>
        [Header("启用白名单")]
        public bool enableWhiteList = false;
        /// <summary>
        /// 白名单，在白名单内的玩家无需输入密码即可解锁
        /// </summary>
        [Header("白名单")]
        public string[] whiteListPlayerNames = new string[0];
        protected bool isWhitelist = false;
        /// <summary>
        /// 记住解锁状态，当作为全局锁时无效
        /// </summary>
        [Header("记住解锁状态，当作为全局锁时无效")]
        public bool rememberUnlockStatus = false;
        /// <summary>
        /// 锁名，记住解锁状态需要
        /// </summary>
        [Header("锁名")]
        public string lockName = "Global";
        protected override void Start()
        {
            // 寻找名为 KeypadButtons 的 GameObject
            var KeypadButtons = gameObject.transform.Find("KeypadButtons");
            if (KeypadButtons == null)
            {
                Debug.LogError("Keypad: KeypadButtons is not set!");
            }
            else
            {
                // 获取 KeypadButtons 下的所有子物体
                Buttons = new GameObject[KeypadButtons.childCount];
                for (int i = 0; i < KeypadButtons.childCount; i++)
                {
                    Buttons[i] = KeypadButtons.GetChild(i).gameObject;
                }
            }
            WhitelistCheck();
            RandomButton();

            if (isGlobal && !Networking.IsOwner(gameObject))
            {
                base.Start();
                return;
            }
            isSynced = true;

            LockCheck();
        }
        protected virtual void LockCheck() { }
        public virtual bool Lock() => false;
        public virtual bool Unlock() => Unlock(_enableTeleport);
        public virtual bool UnlockWithTeleport() => Unlock(true);
        public virtual bool UnlockWithoutTeleport() => Unlock(false);
        public virtual bool Unlock(bool useTeleport) => false;
        public void GoTeleport()
        {
            if (_enableTeleport && _teleportPoint != null)
            {
                Networking.LocalPlayer.TeleportTo(_teleportPoint.position, useTeleportPointRotation ? _teleportPoint.rotation : Networking.LocalPlayer.GetRotation());
            }
        }
        public virtual string ButtonPush(string buttonValue) => "You need to override this method.";
        
        protected virtual bool CheckPasscode() => false;
        public void RandomButton()
        {
            if (!_isRandomButton)
                return;
            if (Buttons.Length <= 0)
            {
                Debug.LogError("Keypad: Buttons is not set!");
                return;
            }
            // 打乱按钮顺序
            for (int i = 0; i < Buttons.Length; i++)
            {
                var temp = Buttons[i];
                var randomIndex = UnityEngine.Random.Range(i, Buttons.Length);
                Buttons[i] = Buttons[randomIndex];
                Buttons[randomIndex] = temp;
            }
            // 重新设置按钮顺序
            for (int i = 0; i < Buttons.Length; i++)
            {
                Buttons[i].transform.SetSiblingIndex(i);
            }
        }
        public void WhitelistCheck()
        {
            isWhitelist = false;
            if (!enableWhiteList) { return; }
            // 检查白名单
            foreach (var playerName in whiteListPlayerNames)
            {
                isWhitelist = Networking.LocalPlayer.displayName == playerName;
                if (isWhitelist) { break; }
            }
        }
        public string ButtonPush1() => ButtonPush("1");
        public string ButtonPush2() => ButtonPush("2");
        public string ButtonPush3() => ButtonPush("3");
        public string ButtonPush4() => ButtonPush("4");
        public string ButtonPush5() => ButtonPush("5");
        public string ButtonPush6() => ButtonPush("6");
        public string ButtonPush7() => ButtonPush("7");
        public string ButtonPush8() => ButtonPush("8");
        public string ButtonPush9() => ButtonPush("9");
        public string ButtonPush0() => ButtonPush("0");
        public string ButtonPushEnter() => ButtonPush("Enter");
        public string ButtonPushClear() => ButtonPush("Clear");
        public override void OnDeserialization()
        {
            base.OnDeserialization();
            LockCheck();
        }
        public void ReadLockStatus()
        {
            if (!rememberUnlockStatus || isGlobal) { return; }
            if (string.IsNullOrEmpty(lockName)) lockName = "Global";
            // var isUnlocked = PlayerData.GetBool(Networking.LocalPlayer, $"Sonic853.Udon.Keypad.{lockName}");
            if (!PlayerData.TryGetBool(Networking.LocalPlayer, $"Sonic853.Udon.Keypad.{lockName}", out var isUnlocked)) { return; }
            if (isUnlocked && isLocked)
            {
                Unlock();
            }
            else if (!isUnlocked && !isLocked)
            {
                Lock();
            }
        }
        public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
        {
            if (!rememberUnlockStatus || isGlobal) { return; }
            if (!player.isLocal) { return; }
            ReadLockStatus();
        }
        /// <summary>
        /// 当玩家数据加载后触发
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            ReadLockStatus();
        }
    }
}
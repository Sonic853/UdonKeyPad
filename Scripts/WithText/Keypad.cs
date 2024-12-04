
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sonic853.Udon.Keypad
{
    public class Keypad : KeypadBase
    {
        /// <summary>
        /// 密码（用户输入）
        /// </summary>
        public string _passcode
        {
            get
            {
                return text.text;
            }
            set
            {
                text.text = value;
            }
        }
        /// <summary>
        /// 显示文字
        /// </summary>
        [Header("显示文字")]
        [SerializeField] protected Text placeholder;
        /// <summary>
        /// 输入框
        /// </summary>
        [Header("输入框")]
        [SerializeField] protected Text text;
        protected override void Start()
        {
            if (placeholder == null)
            {
                placeholder = (Text)gameObject.GetComponentInChildren(typeof(Text));
            }
            if (placeholder == null)
            {
                Debug.LogError("Keypad: placeholder is not set!");
            }
            if (text == null)
            {
                text = (Text)gameObject.GetComponentInChildren(typeof(Text));
            }
            if (text == null)
            {
                Debug.LogError("Keypad: text is not set!");
            }
            base.Start();
        }
        protected override void LockCheck()
        {
            if (isLocked)
            {
                if (isGlobal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Lock));
                }
                else
                {
                    Lock();
                }
                placeholder.text = "Locked";
                _passcode = "";
            }
            else
            {
                if (isGlobal)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, teleportIsGlobal ? nameof(UnlockWithTeleport) : nameof(UnlockWithoutTeleport));
                }
                else
                {
                    Unlock();
                }
                placeholder.text = "Unlocked";
                _passcode = "";
            }
        }
        public override bool Lock()
        {
            if (isLocked)
            {
                foreach (var obj in _lockHiedObjects)
                {
                    obj.SetActive(false);
                }
                foreach (var obj in _lockShowObjects)
                {
                    obj.SetActive(true);
                }
                return true;
            }
            isLocked = true;
            placeholder.text = "Locked";
            _passcode = "";
            foreach (var obj in _lockHiedObjects)
            {
                obj.SetActive(false);
            }
            foreach (var obj in _lockShowObjects)
            {
                obj.SetActive(true);
            }
            // 解锁后记住状态
            if (rememberUnlockStatus && !isGlobal)
            {
                if (string.IsNullOrEmpty(lockName)) lockName = "Global";
                PlayerData.SetBool($"Sonic853.Udon.Keypad.{lockName}", false);
            }
            return true;
        }
        public override bool Unlock(bool useTeleport)
        {
            if (!isLocked)
            {
                foreach (var obj in _lockHiedObjects)
                {
                    obj.SetActive(true);
                }
                foreach (var obj in _lockShowObjects)
                {
                    obj.SetActive(false);
                }
                return true;
            }
            isLocked = false;
            placeholder.text = "Unlocked";
            _passcode = "";
            if (useTeleport) GoTeleport();
            foreach (var obj in _lockHiedObjects)
            {
                obj.SetActive(true);
            }
            foreach (var obj in _lockShowObjects)
            {
                obj.SetActive(false);
            }
            // 解锁后记住状态
            if (rememberUnlockStatus && !isGlobal)
            {
                if (string.IsNullOrEmpty(lockName)) lockName = "Global";
                PlayerData.SetBool($"Sonic853.Udon.Keypad.{lockName}", true);
            }
            return true;
        }
        public override string ButtonPush(string buttonValue)
        {
            switch (buttonValue)
            {
                case "Enter":
                    {
                        if (!enableWhiteList) isWhitelist = false;
                        if (isWhitelist || CheckPasscode())
                        {
                            if (isGlobal)
                            {
                                SendCustomNetworkEvent(NetworkEventTarget.All, teleportIsGlobal ? nameof(UnlockWithTeleport) : nameof(UnlockWithoutTeleport));
                            }
                            else
                            {
                                Unlock();
                            }
                            return "Unlocked";
                        }
                        else
                        {
                            if (!isLocked)
                            {
                                return "Unlocked";
                            }
                            return "Incorrect";
                        }
                    }
                case "Clear":
                    {
                        placeholder.text = "Cleared";
                        _passcode = "";
                        if (!isLocked)
                        {
                            if (isGlobal)
                            {
                                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Lock));
                            }
                            else
                            {
                                Lock();
                            }
                        }
                        return "Cleared";
                    }
                default:
                    {
                        if (!isLocked)
                        {
                            if (_enableTeleport)
                                GoTeleport();
                            return "Unlocked";
                        }
                        _passcode += buttonValue;
                        placeholder.text = "";
                        if (_autoEnter && Passcode.Length == _passcode.Length)
                        {
                            return ButtonPush("Enter");
                        }
                        return _passcode;
                    }
            }
        }
        protected override bool CheckPasscode()
        {
            RandomButton();
            if (Passcode == _passcode)
            {
                placeholder.text = "Unlocked";
                _passcode = "";
                return true;
            }
            else
            {
                placeholder.text = "Incorrect";
                _passcode = "";
                return false;
            }
        }
        public override void OnDeserialization()
        {
            base.OnDeserialization();
        }
        public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
        {
            base.OnPlayerDataUpdated(player, infos);
        }
        /// <summary>
        /// 当玩家数据加载后触发
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            base.OnPlayerRestored(player);
        }
    }
}

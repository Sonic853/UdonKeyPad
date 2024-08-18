
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Sonic853.Udon.UdonKeypad
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Keypad : SyncBehaviour
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
        /// 显示文字
        /// </summary>
        [Header("显示文字")]
        [SerializeField] private Text placeholder;
        /// <summary>
        /// 输入框
        /// </summary>
        [Header("输入框")]
        [SerializeField] private Text text;
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
        [NonSerialized] private GameObject[] Buttons = new GameObject[0];
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
            if (_lockHiedObjects == null)
            {
                Debug.LogError("Keypad: _lockHiedObjects is not set!");
            }
            if (_lockShowObjects == null)
            {
                Debug.LogError("Keypad: _lockShowObjects is not set!");
            }
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
            RandomButton();

            if (isGlobal && !Networking.IsOwner(gameObject))
            {
                base.Start();
                return;
            }
            isSynced = true;

            LockCheck();
        }
        void LockCheck()
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
        public bool Lock()
        {
            if (isLocked)
            {
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
            return true;
        }
        public bool Unlock() => Unlock(_enableTeleport);
        public bool UnlockWithTeleport() => Unlock(true);
        public bool UnlockWithoutTeleport() => Unlock(false);
        public bool Unlock(bool useTeleport)
        {
            if (!isLocked)
            {
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
            return true;
        }
        public void GoTeleport()
        {
            if (_enableTeleport && _teleportPoint != null)
            {
                Networking.LocalPlayer.TeleportTo(_teleportPoint.position, useTeleportPointRotation ? _teleportPoint.rotation : Networking.LocalPlayer.GetRotation());
            }
        }
        public string ButtonPush(string buttonValue)
        {
            switch (buttonValue)
            {
                case "Enter":
                    {
                        if (CheckPasscode())
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
        bool CheckPasscode()
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
    }
}

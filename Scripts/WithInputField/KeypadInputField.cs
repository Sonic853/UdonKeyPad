
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.Persistence;

namespace Sonic853.Udon.Keypad
{
    public class KeypadInputField : KeypadBase
    {
        /// <summary>
        /// 显示文字
        /// </summary>
        [Header("显示文字")]
        [SerializeField] protected Text placeholder;
        /// <summary>
        /// 输入框
        /// </summary>
        [Header("输入框")]
        [SerializeField] protected InputField inputField;
        protected override void Start()
        {
            if (placeholder == null)
            {
                placeholder = (Text)gameObject.GetComponentInChildren(typeof(Text));
            }
            if (placeholder == null)
            {
                Debug.LogError("Keypad: Placeholder is not set!");
            }
            if (inputField == null)
            {
                inputField = (InputField)gameObject.GetComponentInChildren(typeof(InputField));
            }
            if (inputField == null)
            {
                Debug.LogError("Keypad: inputField is not set!");
            }
            base.Start();
        }
        protected override string GetInputField() => inputField.text;
        protected override void SetInputField(string input) => inputField.text = input;
        protected override void SetPlaceholder(string text) => placeholder.text = text;
    }
}

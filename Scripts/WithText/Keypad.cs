
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
        protected override string GetInputField() => text != null ? text.text : inputFieldText;
        protected override void SetInputField(string input)
        {
            if (text != null)
                inputFieldText = text.text = input;
            else
                inputFieldText = input;
        }
        public override void SetPlaceholder(string text)
        {
            if (placeholder != null)
                placeholderText = placeholder.text = text;
            else
                placeholderText = text;
        }
    }
}

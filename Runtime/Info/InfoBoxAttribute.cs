using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Text { get; }
        public EInfoBoxType Type { get; }

        public InfoBoxAttribute(string text, EInfoBoxType type = EInfoBoxType.Normal)
        {
            Text = text;
            Type = type;
        }
        
        public enum EInfoBoxType
        {
            Normal,
            Warning,
            Error
        }
    }
}
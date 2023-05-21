using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringInClassAttribute : PropertyAttribute
    {
        public Type TargetType { get; }
        public string MatchRule { get; }

        public StringInClassAttribute(Type targetType, string matchRule = "")
        {
            TargetType = targetType;
            MatchRule = matchRule;
        }
    }
}
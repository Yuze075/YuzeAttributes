using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringInClassAttribute : PropertyAttribute
    {
        public Type TargetType { get; }
        public string MatchRule { get; }
        public bool HasLabel  { get; }

        public StringInClassAttribute(Type targetType, string matchRule = "", bool hasLabel = true)
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
        }
    }
}
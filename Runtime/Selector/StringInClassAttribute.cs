using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [Serializable]
    public class StringInClassAttribute : PropertyAttribute
    {
        public Type TargetType { get; }
        public string MatchRule { get; }
        public bool HasLabel { get; }
        public bool UseValueToName { get; }
        public bool DrawNewTextToMatchRule { get; }
        public Type NewTextTargetType { get; }
        public string NewTextMatchRule { get; set; } = "";

        public StringInClassAttribute(Type targetType, string matchRule = "", bool hasLabel = true,
            bool useValueToName = true, bool drawNewTextToMatchRule = false, Type newTextTargetType = null)
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
            UseValueToName = useValueToName;
            DrawNewTextToMatchRule = drawNewTextToMatchRule;
            NewTextTargetType = newTextTargetType;
        }
    }
}
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
        public bool DrawNewTextToMatchRule => NewTextTargetType != null;
        public Type NewTextTargetType { get; }
        public string NewTextMatchRule { get; set; } = "";

        public StringInClassAttribute(Type targetType,string matchRule = "", Type matchRuleType = null, bool hasLabel = true,
            bool useValueToName = true)
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
            UseValueToName = useValueToName;
            NewTextTargetType = matchRuleType;
        }
    }
}
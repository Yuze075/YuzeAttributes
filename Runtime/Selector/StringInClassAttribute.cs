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
        public string MatchRuleValueName { get; }

        public StringInClassAttribute(Type targetType, string matchRule = "", bool hasLabel = true,
            string matchRuleValueName = "")
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
            MatchRuleValueName = matchRuleValueName;
        }
    }
}
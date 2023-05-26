﻿using System;
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

        [Obsolete]
        public StringInClassAttribute(Type targetType, string matchRule = "", bool hasLabel = true,
            bool useValueToName = true, bool drawNewTextToMatchRule = false, Type newTextTargetType = null)
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
            UseValueToName = useValueToName;
            NewTextTargetType = newTextTargetType;
        }

        public StringInClassAttribute(Type targetType, string matchRule = "", bool hasLabel = true,
            bool useValueToName = true
        )
        {
            TargetType = targetType;
            MatchRule = matchRule;
            HasLabel = hasLabel;
            UseValueToName = useValueToName;
        }

        public StringInClassAttribute(Type targetType, Type matchRuleType = null, bool hasLabel = true,
            bool useValueToName = true)
        {
            TargetType = targetType;
            NewTextTargetType = matchRuleType;
            HasLabel = hasLabel;
            UseValueToName = useValueToName;
        }
    }
}
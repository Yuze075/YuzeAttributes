using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class MatchRuleAttribute : PropertyAttribute
{
    public Type TargetType { get; }

    public bool UseValueToName { get; }

    public string MatchRule { get; set; } = "";

    public MatchRuleAttribute(Type targetType = null, bool useValueToName = false)
    {
        UseValueToName = useValueToName;
        TargetType = targetType;
    }
}
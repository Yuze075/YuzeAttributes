using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RangeSliderAttribute : PropertyAttribute
    {
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        public RangeSliderAttribute(float minValue, float maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
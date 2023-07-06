using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ProgressBarAttribute : PropertyAttribute
    {
        public string Name { get; }
        public float MaxValue { get; }
        public string MaxValueName  { get; }
        public string ValueName { get; }
        public ColorType Color { get; }

        public ProgressBarAttribute(string name, float maxValue, string valueName,
            ColorType color = ColorType.Blue)
        {
            Name = name;
            MaxValue = maxValue;
            ValueName = valueName;
            Color = color;
        }
        
        public ProgressBarAttribute(string name, float maxValue,
            ColorType color = ColorType.Blue) : this(name, maxValue, null, color)
        {
        }

        public ProgressBarAttribute(float maxValue, string valueName,
            ColorType color = ColorType.Blue) : this(null, maxValue, valueName, color)
        {
        }
        
        public ProgressBarAttribute(float maxValue,
            ColorType color = ColorType.Blue) : this(null,maxValue, null, color)
        {
        }
        
        public ProgressBarAttribute(string name, string maxValueName, string valueName,
            ColorType color = ColorType.Blue)
        {
            Name = name;
            MaxValueName = maxValueName;
            ValueName = valueName;
            Color = color;
        }

        public ProgressBarAttribute(string maxValue,
            ColorType color = ColorType.Blue) : this(null,maxValue, null, color)
        {
        }
    }
}
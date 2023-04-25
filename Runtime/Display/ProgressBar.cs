using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ProgressBarAttribute : PropertyAttribute
    {
        public string Name { get; }
        public float MaxValue { get; }
        public string MaxValueName { get; }
        public ColorType Color { get; }

        public ProgressBarAttribute(string name, float maxValue, ColorType color = ColorType.Blue)
        {
            Name = name;
            MaxValue = maxValue;
            Color = color;
        }

        public ProgressBarAttribute(string name, string maxValueName, ColorType color = ColorType.Blue)
        {
            Name = name;
            MaxValueName = maxValueName;
            Color = color;
        }

        public ProgressBarAttribute(float maxValue, ColorType color = ColorType.Blue)
            : this("", maxValue, color)
        {
        }

        public ProgressBarAttribute(string maxValueName, ColorType color = ColorType.Blue)
            : this("", maxValueName, color)
        {
        }
        
        public enum ColorType
        {
            Clear,
            White,
            Black,
            Gray,
            Red,
            Pink,
            Orange,
            Yellow,
            Green,
            Blue,
            Indigo,
            Violet
        }


        public static Color GetColor(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.Clear => new Color32(0, 0, 0, 0),
                ColorType.White => new Color32(255, 255, 255, 255),
                ColorType.Black => new Color32(0, 0, 0, 255),
                ColorType.Gray => new Color32(128, 128, 128, 255),
                ColorType.Red => new Color32(255, 0, 63, 255),
                ColorType.Pink => new Color32(255, 152, 203, 255),
                ColorType.Orange => new Color32(255, 128, 0, 255),
                ColorType.Yellow => new Color32(255, 211, 0, 255),
                ColorType.Green => new Color32(98, 200, 79, 255),
                ColorType.Blue => new Color32(0, 135, 189, 255),
                ColorType.Indigo => new Color32(75, 0, 130, 255),
                ColorType.Violet => new Color32(128, 0, 255, 255),
                _ => new Color32(0, 0, 0, 255)
            };
        }
    }
}
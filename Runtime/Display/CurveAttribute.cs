using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CurveAttribute : PropertyAttribute
    {
        public Vector2 Min { get; }
        public Vector2 Max { get; }
        public ColorType Color { get; }
        public CurveAttribute(Vector2 min, Vector2 max, ColorType color = ColorType.Clear)
        {
            Min = min;
            Max = max;
            Color = color;
        }

        public CurveAttribute(ColorType color = ColorType.Clear)
            : this(Vector2.zero, Vector2.one, color)
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
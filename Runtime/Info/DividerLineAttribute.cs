using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DividerLineAttribute : PropertyAttribute
    {
        public float Height { get; }
        public ColorType Color { get; }

        public DividerLineAttribute(float height = 2.0f, ColorType color = ColorType.Gray)
        {
            Height = height;
            Color = color;
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
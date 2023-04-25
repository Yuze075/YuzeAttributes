using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetPreviewAttribute : PropertyAttribute
    {
        public const int DefaultWidth = 64;
        public const int DefaultHeight = 64;

        public int Width { get; }
        public int Height { get; }

        public AssetPreviewAttribute(int width = DefaultWidth, int height = DefaultHeight)
        {
            Width = width;
            Height = height;
        }
    }
}
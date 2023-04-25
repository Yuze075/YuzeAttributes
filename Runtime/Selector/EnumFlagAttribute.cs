using System;
using UnityEngine;

namespace YuzeToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumFlagAttribute : PropertyAttribute
    {
        /// <summary>
        /// Minimum width of toggle created within the drawer.
        /// </summary>
        public float ToggleWidth { get; set; } = 85.0f;

        /// <summary>
        /// Height of a single toggle.
        /// </summary>
        public float ToggleHeight { get; set; } = 16.0f;

        /// <summary>
        /// Spacing between toggle buttons.
        /// </summary>
        public float ToggleSpacing { get; set; } = 2.0f;
    }
}
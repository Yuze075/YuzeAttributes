using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarDrawer : PropertyDrawer
    {
        private static class Style
        {
            internal static readonly float RowHeight = EditorGUIUtility.singleLineHeight;
            internal static readonly float BoxHeight = EditorGUIUtility.singleLineHeight * 2.1f;
            internal static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var progressBarAttribute = (ProgressBarAttribute)attribute;
            float maxValue;
            if (!string.IsNullOrWhiteSpace(progressBarAttribute.MaxValueName) &&
                AttributeHelperEditor.TryGetValue(property, progressBarAttribute.MaxValueName, out object obj1) &&
                IsNumber(obj1))
            {
                maxValue = CastToFloat(obj1);
            }
            else
            {
                maxValue = progressBarAttribute.MaxValue;
            }
            return IsNumber(property) && IsNumber(maxValue)
                ? EditorGUI.GetPropertyHeight(property, label)
                : EditorGUI.GetPropertyHeight(property, label) + Style.BoxHeight + Style.Spacing * 2;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (!IsNumber(property))
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

            var progressBarAttribute = (ProgressBarAttribute)attribute;
            float value;
            if (!string.IsNullOrWhiteSpace(progressBarAttribute.ValueName) &&
                AttributeHelperEditor.TryGetValue(property, progressBarAttribute.ValueName, out object obj) &&
                IsNumber(obj))
            {
                value = CastToFloat(obj);
            }
            else
            {
                value = property.propertyType == SerializedPropertyType.Integer
                    ? property.intValue
                    : property.floatValue;
            }

            var valueFormatted = property.propertyType == SerializedPropertyType.Integer
                ? value.ToString(CultureInfo.InvariantCulture)
                : $"{value:0.00}";

            float maxValue;
            if (!string.IsNullOrWhiteSpace(progressBarAttribute.MaxValueName) &&
                AttributeHelperEditor.TryGetValue(property, progressBarAttribute.MaxValueName, out object obj1) &&
                IsNumber(obj1))
            {
                maxValue = CastToFloat(obj1);
            }
            else
            {
                maxValue = progressBarAttribute.MaxValue;
            }

            if (IsNumber(maxValue))
            {
                var fillPercentage = value / CastToFloat(maxValue);
                var barLabel =
                    (!string.IsNullOrEmpty(progressBarAttribute.Name)
                        ? "[" + progressBarAttribute.Name + "] "
                        : "") +
                    valueFormatted + "/" + maxValue;
                var barColor = progressBarAttribute.Color.GetColor();
                var labelColor = Color.white;

                var indentLength = GetIndentLength(rect);
                var barRect = new Rect
                {
                    x = rect.x + indentLength,
                    y = rect.y,
                    width = rect.width - indentLength,
                    height = EditorGUIUtility.singleLineHeight
                };

                DrawBar(barRect, Mathf.Clamp01(fillPercentage), barLabel, barColor, labelColor);
            }
            else
            {
                rect = DrawWarningMessage(rect);
                rect.yMin = rect.yMax + Style.Spacing;
                rect.yMax = rect.yMin + Style.RowHeight;
            }

            EditorGUI.EndProperty();
        }

        private static void DrawBar(Rect rect, float fillPercent, string label, Color barColor, Color labelColor)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var fillRect = new Rect(rect.x, rect.y, rect.width * fillPercent, rect.height);

            EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f));
            EditorGUI.DrawRect(fillRect, barColor);

            // set alignment and cache the default
            var align = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.UpperCenter;

            // set the color and cache the default
            var c = GUI.contentColor;
            GUI.contentColor = labelColor;

            // calculate the position
            var labelRect = new Rect(rect.x, rect.y - 2, rect.width, rect.height);

            // draw~
            EditorGUI.DropShadowLabel(labelRect, label);

            // reset color and alignment
            GUI.contentColor = c;
            GUI.skin.label.alignment = align;
        }

        private static bool IsNumber(SerializedProperty property)
        {
            var isNumber = property.propertyType is SerializedPropertyType.Float or SerializedPropertyType.Integer;
            return isNumber;
        }

        private static bool IsNumber(object obj)
        {
            return obj is float or int;
        }

        private static float CastToFloat(object obj)
        {
            if (obj is int i)
            {
                return i;
            }

            return (float)obj;
        }

        private static float GetIndentLength(Rect sourceRect)
        {
            var indentRect = EditorGUI.IndentedRect(sourceRect);
            var indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }

        private static Rect DrawWarningMessage(Rect rect)
        {
            rect = new Rect(rect.x,
                rect.y,
                rect.width, Style.BoxHeight);
            EditorGUI.HelpBox(rect, "The provided dynamic max value for the progress bar is not correct.",
                MessageType.Warning);
            return rect;
        }
    }
}
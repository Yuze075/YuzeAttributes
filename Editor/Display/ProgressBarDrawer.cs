using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarDrawer : PropertyDrawer
    {
        private static Rect DrawWarningMessage(Rect rect)
        {
            rect = new Rect(rect.x,
                rect.y,
                rect.width, Style.BoxHeight);
            EditorGUI.HelpBox(rect, "The provided dynamic max value for the progress bar is not correct.",
                MessageType.Warning);
            return rect;
        }

        private static class Style
        {
            internal static readonly float RowHeight = EditorGUIUtility.singleLineHeight;
            internal static readonly float BoxHeight = EditorGUIUtility.singleLineHeight * 2.1f;
            internal static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var progressBarAttribute = attribute as ProgressBarAttribute;
            var maxValue = GetMaxValue(property, progressBarAttribute);
            return IsNumber(property) && IsNumber(maxValue)
                ? EditorGUI.GetPropertyHeight(property, label)
                : EditorGUI.GetPropertyHeight(property, label) + Style.BoxHeight + Style.Spacing * 2;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (!IsNumber(property))
            {
                var warningContent = new GUIContent(property.displayName + "(Incorrect Attribute Used)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
                return;
            }

            var progressBarAttribute = GetAttribute<ProgressBarAttribute>(property);
            var value = property.propertyType == SerializedPropertyType.Integer
                ? property.intValue
                : property.floatValue;
            var valueFormatted = property.propertyType == SerializedPropertyType.Integer
                ? value.ToString(CultureInfo.InvariantCulture)
                : $"{value:0.00}";
            var maxValue = GetMaxValue(property, progressBarAttribute);

            if (maxValue != null && IsNumber(maxValue))
            {
                var fillPercentage = value / CastToFloat(maxValue);
                var barLabel =
                    (!string.IsNullOrEmpty(progressBarAttribute.Name) ? "[" + progressBarAttribute.Name + "] " : "") +
                    valueFormatted + "/" + maxValue;
                var barColor = ProgressBarAttribute.GetColor(progressBarAttribute.Color);
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

        private static object GetMaxValue(SerializedProperty property, ProgressBarAttribute progressBarAttribute)
        {
            if (string.IsNullOrEmpty(progressBarAttribute.MaxValueName))
            {
                return progressBarAttribute.MaxValue;
            }

            var target = GetTargetObjectWithProperty(property);

            var valuesFieldInfo = GetAllFields(target,
                    f => f.Name.Equals(progressBarAttribute.MaxValueName, StringComparison.Ordinal))
                .FirstOrDefault();
            if (valuesFieldInfo != null)
            {
                return valuesFieldInfo.GetValue(target);
            }

            var valuesPropertyInfo = GetAllProperties(target,
                p => p.Name.Equals(progressBarAttribute.MaxValueName, StringComparison.Ordinal)).FirstOrDefault();
            if (valuesPropertyInfo != null)
            {
                return valuesPropertyInfo.GetValue(target);
            }

            var methodValuesInfo =
                GetAllMethods(target, m => m.Name.Equals(progressBarAttribute.MaxValueName, StringComparison.Ordinal))
                    .FirstOrDefault();
            if (methodValuesInfo != null &&
                (methodValuesInfo.ReturnType == typeof(float) || methodValuesInfo.ReturnType == typeof(int)) &&
                methodValuesInfo.GetParameters().Length == 0)
            {
                return methodValuesInfo.Invoke(target, null);
            }

            return null;
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

        public static float GetIndentLength(Rect sourceRect)
        {
            var indentRect = EditorGUI.IndentedRect(sourceRect);
            var indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }

        private static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            var attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        private static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            var fieldInfo = GetAllFields(GetTargetObjectWithProperty(property),
                f => f.Name.Equals(property.name, StringComparison.Ordinal)).FirstOrDefault();
            if (fieldInfo == null)
            {
                return new T[] { };
            }

            return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
        }

        private static object GetTargetObjectWithProperty(SerializedProperty property)
        {
            var path = property.propertyPath.Replace(".Array.data[", "[");
            object obj = property.serializedObject.targetObject;
            var elements = path.Split('.');

            for (var i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
                    var index = Convert.ToInt32(element[element.IndexOf("[", StringComparison.Ordinal)..]
                        .Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            if (GetValue_Imp(source, name) is not IEnumerable enumerable)
            {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();
            for (var i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            return enumerator.Current;
        }

        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
            {
                return null;
            }

            var type = source.GetType();

            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(source);
                }

                var property = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source, null);
                }

                type = type.BaseType;
            }

            return null;
        }

        private static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            var types = GetSelfAndBaseTypes(target);

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                               BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        private static List<Type> GetSelfAndBaseTypes(object target)
        {
            var types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            return types;
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            var types = GetSelfAndBaseTypes(target);

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var propertyInfos = types[i]
                    .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                   BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        private static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            if (target == null)
            {
                Debug.LogError("The target object is null. Check for missing scripts.");
                yield break;
            }

            var types = GetSelfAndBaseTypes(target);

            for (var i = types.Count - 1; i >= 0; i--)
            {
                var methodInfos = types[i]
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(predicate);

                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }
    }
}
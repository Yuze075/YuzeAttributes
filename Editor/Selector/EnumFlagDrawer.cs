using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
    public sealed class EnumFlagDrawer : PropertyDrawer
    {
        #region static

        private static readonly List<EnumTogglesDrawer> ToggleDrawers = new()
        {
            new BasicEnumTogglesDrawer(),
            new FlagsEnumTogglesDrawer()
        };

        private static readonly GUIStyle ToggleStyle = new(GUI.skin.button)
        {
            fontSize = 10
        };

        private static EnumTogglesDrawer GetDrawer(SerializedProperty property, FieldInfo field)
        {
            var enumType = GetEnumType(property, field);
            return GetDrawer(enumType);
        }

        private static EnumTogglesDrawer GetDrawer(Type enumType)
        {
            var drawer = ToggleDrawers.Find(d => d.IsValid(enumType));
            drawer.Prepare(enumType);
            return drawer;
        }

        private static Type GetEnumType(SerializedProperty property, FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            var fieldType = field.FieldType;
            if (!property.isArray && typeof(IList).IsAssignableFrom(field.FieldType))
            {
                return fieldType.IsGenericType
                    ? fieldType.GetGenericArguments()[0]
                    : fieldType.GetElementType();
            }

            return field.FieldType;
        }

        private static InputData GetInputData(EnumFlagAttribute enumFlagAttribute)
        {
            var inspectorStyle = EditorStyles.inspectorDefaultMargins;
            var leftPadding = inspectorStyle.padding.left;
            var otherRowWidth = EditorGUIUtility.currentViewWidth - 2 * leftPadding;
            var firstRowWidth = EditorGUIUtility.currentViewWidth - 2 * leftPadding - EditorGUIUtility.labelWidth;

            return new InputData()
            {
                ToggleWidth = enumFlagAttribute.ToggleWidth,
                ToggleHeight = enumFlagAttribute.ToggleHeight,
                ToggleSpacing = enumFlagAttribute.ToggleSpacing,
                Spacing = EditorGUIUtility.standardVerticalSpacing,
                FirstRowPosition = new Rect(0, 0, firstRowWidth, 0),
                OtherRowPosition = new Rect(0, 0, otherRowWidth, 0)
            };
        }

        private static InputData GetInputData(EnumFlagAttribute enumFlagAttribute, Rect position, Rect prefixPosition)
        {
            var toggleWidth = enumFlagAttribute.ToggleWidth;
            var toggleHeight = enumFlagAttribute.ToggleHeight;
            var toggleSpacing = enumFlagAttribute.ToggleSpacing;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            var firstRowPosition = new Rect(prefixPosition)
            {
                height = toggleHeight
            };
            var otherRowPosition = new Rect(position)
            {
                yMin = firstRowPosition.yMax + spacing
            };
            otherRowPosition.yMax = otherRowPosition.yMin + toggleHeight;

            return new InputData()
            {
                ToggleWidth = toggleWidth,
                ToggleHeight = toggleHeight,
                ToggleSpacing = toggleSpacing,
                Spacing = spacing,
                FirstRowPosition = firstRowPosition,
                OtherRowPosition = otherRowPosition
            };
        }

        #endregion

        private EnumFlagAttribute Attribute => attribute as EnumFlagAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.propertyType == SerializedPropertyType.Enum
                ? GetPropertyHeightSafe(property)
                : base.GetPropertyHeight(property, label);
        }

        private float GetPropertyHeightSafe(SerializedProperty property)
        {
            var drawer = GetDrawer(property, fieldInfo);
            var input = GetInputData(Attribute);
            return drawer.GetHeight(input);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(rect, label, property);
            if (property.propertyType == SerializedPropertyType.Enum)
            {
                var prefixPosition = EditorGUI.PrefixLabel(rect, label);

                var drawer = GetDrawer(property, fieldInfo);
                var input = GetInputData(Attribute, rect, prefixPosition);
                drawer.OnGUI(input, property);
            }
            else
            {
                var warningContent = new GUIContent(property.displayName + "(Incorrect Attribute Used)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
            }

            EditorGUI.EndProperty();
        }

        #region struct

        private struct InputData
        {
            public Rect FirstRowPosition;
            public Rect OtherRowPosition;
            public float ToggleWidth;
            public float ToggleHeight;
            public float ToggleSpacing;
            public float Spacing;
        }

        #endregion

        #region class

        private abstract class EnumTogglesDrawer
        {
            private readonly Dictionary<Type, EnumData> _cachedEnumData = new();

            protected EnumData Data;

            #region static

            protected static bool IsFlagsEnum([NotNull] Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return System.Attribute.IsDefined(type, typeof(FlagsAttribute));
            }

            protected static DraftInfo GetDraftInfo(InputData input, in EnumData enumData)
            {
                var draft = new DraftInfo
                {
                    TogglesInFirstRow = GetMaxTogglesCount(input.FirstRowPosition.width, input.ToggleWidth)
                };
                var togglesInRows = enumData.Values.Count - draft.TogglesInFirstRow;
                if (togglesInRows <= 0) return draft;
                draft.TogglesInOtherRow = GetMaxTogglesCount(input.OtherRowPosition.width, input.ToggleWidth);
                draft.OtherRowsCount = Mathf.CeilToInt(togglesInRows / (float)draft.TogglesInOtherRow);

                return draft;
            }

            private static int GetMaxTogglesCount(float stripWidth, float toggleWidth)
            {
                return Mathf.Max(1, Mathf.FloorToInt(stripWidth / toggleWidth));
            }

            #endregion

            #region abstract

            protected abstract EnumData GetEnumData(string[] rawLabels, Array rawValues);
            protected abstract int? DrawToggle(Rect position, int? mask, int enumValue, string label);
            protected abstract int? GetMaskValue(SerializedProperty property, in EnumData data);
            protected abstract void SetMaskValue(SerializedProperty property, in EnumData data, int? maskValue);

            #endregion

            #region protected

            protected void DrawGroup(InputData input, in EnumData enumData, ref int? mask)
            {
                var draft = GetDraftInfo(input, enumData);
                var strip = new StripInfo(input.FirstRowPosition, input.ToggleSpacing, draft.TogglesInFirstRow);
                var index = 0;

                DrawStrip(strip, enumData, ref mask, ref index);
                strip.Position = input.OtherRowPosition;
                strip.Division = draft.TogglesInOtherRow;

                for (var i = 0; i < draft.OtherRowsCount; i++)
                {
                    DrawStrip(strip, enumData, ref mask, ref index);
                    strip.Position.y += input.ToggleHeight + input.Spacing;
                }
            }

            protected void DrawStrip(StripInfo strip, in EnumData enumData, ref int? mask, ref int index)
            {
                var enumLabels = enumData.Labels;
                var enumValues = enumData.Values;

                var division = strip.Division;
                var maxRowToggles = Mathf.Min(enumLabels.Count, index + division);
                var togglesToDraw = maxRowToggles - index;
                var position = strip.Position;
                position.width = (position.width - strip.Spacing * (togglesToDraw - 1)) / togglesToDraw;

                for (; index < maxRowToggles; index++)
                {
                    var enumValue = enumValues[index];
                    var enumLabel = enumLabels[index];
                    mask = DrawToggle(position, mask, (int)enumValue, enumLabel);
                    position.x += position.width + strip.Spacing;
                }
            }

            #endregion

            #region public

            public virtual bool IsValid(Type enumType)
            {
                return true;
            }

            public void Prepare(Type enumType)
            {
                if (_cachedEnumData.TryGetValue(enumType, out var data))
                {
                    Data = data;
                    return;
                }

                var rawLabels = Enum.GetNames(enumType);
                var rawValues = Enum.GetValues(enumType);
                Data = _cachedEnumData[enumType] = GetEnumData(rawLabels, rawValues);
            }

            public float GetHeight(InputData input)
            {
                var draft = GetDraftInfo(input, Data);
                return ((input.ToggleHeight + input.Spacing) * draft.OtherRowsCount) + input.ToggleHeight;
            }

            public void OnGUI(InputData input, SerializedProperty property)
            {
                var mask = GetMaskValue(property, Data);
                EditorGUI.BeginChangeCheck();
                DrawGroup(input, Data, ref mask);
                if (EditorGUI.EndChangeCheck())
                {
                    SetMaskValue(property, Data, mask);
                }
            }

            #endregion

            #region struct

            protected struct EnumData
            {
                public List<string> Labels;
                public List<object> Values;
                public int ValuesSum;
                public Type EnumType;
            }

            protected struct DraftInfo
            {
                public int TogglesInFirstRow;
                public int TogglesInOtherRow;
                public int OtherRowsCount;
            }

            protected struct StripInfo
            {
                public Rect Position;
                public int Division;
                public readonly float Spacing;

                public StripInfo(Rect position, float spacing, int division)
                {
                    Position = position;
                    Spacing = spacing;
                    Division = division;
                }
            }

            #endregion
        }

        private sealed class BasicEnumTogglesDrawer : EnumTogglesDrawer
        {
            protected override EnumData GetEnumData(string[] rawLabels, Array rawValues)
            {
                var labels = new List<string>();
                var values = new List<object>();
                var enumData = new EnumData
                {
                    Labels = labels,
                    Values = values,
                };

                labels.AddRange(rawLabels);
                values.AddRange(rawValues.Cast<object>().Select((_, i) => rawValues.GetValue(i)));
                return enumData;
            }

            protected override int? DrawToggle(Rect position, int? mask, int enumValue, string label)
            {
                EditorGUI.BeginChangeCheck();
                var value = mask.HasValue && mask.Value == enumValue;
                var result = GUI.Toggle(position, value, label, ToggleStyle);
                if (!EditorGUI.EndChangeCheck()) return mask;
                if (result)
                {
                    mask = enumValue;
                }

                return mask;
            }

            protected override int? GetMaskValue(SerializedProperty property, in EnumData data)
            {
                return property.hasMultipleDifferentValues ? null : property.intValue;
            }

            protected override void SetMaskValue(SerializedProperty property, in EnumData data, int? maskValue)
            {
                if (maskValue.HasValue)
                {
                    property.intValue = maskValue.Value;
                }
            }

            public override bool IsValid(Type enumType)
            {
                return !IsFlagsEnum(enumType);
            }
        }

        private sealed class FlagsEnumTogglesDrawer : EnumTogglesDrawer
        {
            protected override EnumData GetEnumData(string[] rawLabels, Array rawValues)
            {
                var labels = new List<string>();
                var values = new List<object>();
                var enumData = new EnumData
                {
                    Labels = labels,
                    Values = values
                };

                labels.Add("Everything");
                for (var i = 0; i < rawValues.Length; i++)
                {
                    var value = (int)rawValues.GetValue(i);
                    switch (value)
                    {
                        case 0:
                            continue;
                        case ~0:
                            labels[0] = rawLabels[i];
                            continue;
                    }

                    labels.Add(rawLabels[i]);
                    values.Add(value);
                    enumData.ValuesSum += value;
                }

                values.Insert(0, enumData.ValuesSum);
                return enumData;
            }

            protected override int? DrawToggle(Rect position, int? mask, int enumValue, string label)
            {
                EditorGUI.BeginChangeCheck();
                var value = mask.HasValue && mask.Value == (mask.Value | enumValue);
                var result = GUI.Toggle(position, value, label, ToggleStyle);
                if (!EditorGUI.EndChangeCheck()) return mask;
                mask ??= 0;
                mask = result ? mask | enumValue : mask & ~enumValue;
                return mask;
            }

            protected override int? GetMaskValue(SerializedProperty property, in EnumData data)
            {
                if (property.hasMultipleDifferentValues)
                {
                    return null;
                }

                return property.intValue == ~0 ? data.ValuesSum : property.intValue;
            }

            protected override void SetMaskValue(SerializedProperty property, in EnumData data, int? maskValue)
            {
                if (maskValue.HasValue)
                {
                    property.intValue = maskValue.Value == data.ValuesSum ? ~0 : maskValue.Value;
                }
            }

            public override bool IsValid(Type enumType)
            {
                return IsFlagsEnum(enumType);
            }
        }

        #endregion
    }
}
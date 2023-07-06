using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SetValueInListAttribute))]
    public class SetValueInListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var dropdownAttribute = (SetValueInListAttribute)attribute;
            var target = AttributeHelperEditor.GetTargetObjectWithProperty(property);
            var valuesObject = AttributeHelperRuntime.GetValue(target, dropdownAttribute.ValuesName);
            var dropdownField = AttributeHelperRuntime.GetAllFields(target.GetType(),
                    f => f.Name.Equals(property.name, StringComparison.Ordinal))
                .FirstOrDefault();

            if (!AreValuesValid(valuesObject, dropdownField))
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

            switch (valuesObject)
            {
                case IList list when dropdownField != null && dropdownField.FieldType == GetElementType(list):
                {
                    // Selected value
                    var selectedValue = dropdownField.GetValue(target);

                    // Values and display options
                    var valuesList = list;
                    var values = new object[valuesList.Count];
                    var displayOptions = new string[valuesList.Count];

                    for (var i = 0; i < values.Length; i++)
                    {
                        var value = valuesList[i];
                        values[i] = value;
                        displayOptions[i] = value == null ? "<null>" : value.ToString();
                    }

                    // Selected value index
                    var selectedValueIndex = Array.IndexOf(values, selectedValue);
                    if (selectedValueIndex < 0)
                    {
                        selectedValueIndex = 0;
                    }

                    Dropdown(rect, property.serializedObject, target, dropdownField, label.text, selectedValueIndex,
                        values, displayOptions);
                    break;
                }
                case IListValue dropdown:
                {
                    // Current value
                    if (dropdownField != null)
                    {
                        var selectedValue = dropdownField.GetValue(target);

                        // Current value index, values and display options
                        var index = -1;
                        var selectedValueIndex = -1;
                        var values = new List<object>();
                        var displayOptions = new List<string>();

                        using (var dropdownEnumerator = dropdown.GetEnumerator())
                        {
                            while (dropdownEnumerator.MoveNext())
                            {
                                index++;

                                var current = dropdownEnumerator.Current;
                                if (current.Value?.Equals(selectedValue) == true)
                                {
                                    selectedValueIndex = index;
                                }

                                values.Add(current.Value);

                                if (current.Key == null)
                                {
                                    displayOptions.Add("<null>");
                                }
                                else if (string.IsNullOrWhiteSpace(current.Key))
                                {
                                    displayOptions.Add("<empty>");
                                }
                                else
                                {
                                    displayOptions.Add(current.Key);
                                }
                            }
                        }

                        if (selectedValueIndex < 0)
                        {
                            selectedValueIndex = 0;
                        }

                        Dropdown(rect, property.serializedObject, target, dropdownField, label.text,
                            selectedValueIndex, values.ToArray(), displayOptions.ToArray());
                    }

                    break;
                }
            }
            EditorGUI.EndProperty();
        }

        private static bool AreValuesValid(object values, FieldInfo dropdownField)
        {
            if (values == null || dropdownField == null)
            {
                return false;
            }

            return (values is IList && dropdownField.FieldType == GetElementType(values)) ||
                   values is IListValue;
        }

        private static Type GetElementType(object values)
        {
            var valuesType = values.GetType();
            var elementType = valuesType.IsGenericType
                ? valuesType.GetGenericArguments()[0]
                : valuesType.GetElementType();
            return elementType;
        }

        private static void Dropdown(
            Rect rect, SerializedObject serializedObject, object target, FieldInfo dropdownField,
            string label, int selectedValueIndex, object[] values, string[] displayOptions)
        {
            EditorGUI.BeginChangeCheck();

            var newIndex = EditorGUI.Popup(rect, label, selectedValueIndex, displayOptions);
            var newValue = values[newIndex];

            var dropdownValue = dropdownField.GetValue(target);
            if (dropdownValue != null && dropdownValue.Equals(newValue)) return;
            Undo.RecordObject(serializedObject.targetObject, "Dropdown");
            dropdownField.SetValue(target, newValue);
        }
    }
}
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
            var target = GetTargetObjectWithProperty(property);

            var valuesObject = GetValues(property, dropdownAttribute.ValuesName);
            var dropdownField = GetAllFields(target, f => f.Name.Equals(property.name, StringComparison.Ordinal))
                .FirstOrDefault();

            if (AreValuesValid(valuesObject, dropdownField))
            {
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

        private static object GetValues(SerializedProperty property, string valuesName)
        {
            var target = GetTargetObjectWithProperty(property);

            var valuesFieldInfo = GetAllFields(target, f => f.Name.Equals(valuesName, StringComparison.Ordinal))
                .FirstOrDefault();
            if (valuesFieldInfo != null)
            {
                return valuesFieldInfo.GetValue(target);
            }

            var valuesPropertyInfo =
                GetAllProperties(target, p => p.Name.Equals(valuesName, StringComparison.Ordinal)).FirstOrDefault();
            if (valuesPropertyInfo != null)
            {
                return valuesPropertyInfo.GetValue(target);
            }

            var methodValuesInfo =
                GetAllMethods(target, m => m.Name.Equals(valuesName, StringComparison.Ordinal)).FirstOrDefault();
            if (methodValuesInfo != null &&
                methodValuesInfo.ReturnType != typeof(void) &&
                methodValuesInfo.GetParameters().Length == 0)
            {
                return methodValuesInfo.Invoke(target, null);
            }

            return null;
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

        public static void Dropdown(
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
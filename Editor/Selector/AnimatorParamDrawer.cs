using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public class AnimatorParamDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType is SerializedPropertyType.Integer or SerializedPropertyType.String)
            {
                var animatorParamAttribute = GetAttribute<AnimatorParamAttribute>(property);

                var animatorController =
                    GetAnimatorController(property, animatorParamAttribute.AnimatorName);
                if (animatorController == null)
                {
                    EditorGUI.PropertyField(rect, property, label);
                    return;
                }

                var parametersCount = animatorController.parameters.Length;
                var animatorParameters =
                    new List<AnimatorControllerParameter>(parametersCount);
                for (var i = 0; i < parametersCount; i++)
                {
                    var parameter = animatorController.parameters[i];
                    if (animatorParamAttribute.AnimatorParamType == null ||
                        parameter.type == animatorParamAttribute.AnimatorParamType)
                    {
                        animatorParameters.Add(parameter);
                    }
                }

                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        DrawPropertyForInt(rect, property, label, animatorParameters);
                        break;
                    case SerializedPropertyType.String:
                        DrawPropertyForString(rect, property, label, animatorParameters);
                        break;
                    default:
                        EditorGUI.PropertyField(rect, property, label);
                        break;
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
                var propertyInfos = types[i].GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.NonPublic | BindingFlags.Public |
                                                           BindingFlags.DeclaredOnly).Where(predicate);
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
                                BindingFlags.Public | BindingFlags.DeclaredOnly).Where(predicate);
                foreach (var methodInfo in methodInfos)
                {
                    yield return methodInfo;
                }
            }
        }

        private static T GetAttribute<T>(SerializedProperty property) where T : class
        {
            var attributes = GetAttributes<T>(property);
            return (attributes.Length > 0) ? attributes[0] : null;
        }

        private static T[] GetAttributes<T>(SerializedProperty property) where T : class
        {
            var fieldInfo = GetAllFields(GetTargetObjectWithProperty(property),
                    f => f.Name.Equals(property.name, StringComparison.Ordinal))
                .FirstOrDefault();
            if (fieldInfo == null)
            {
                return new T[] { };
            }

            return (T[])fieldInfo.GetCustomAttributes(typeof(T), true);
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
                               BindingFlags.Public | BindingFlags.DeclaredOnly).Where(predicate);
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
                        .Replace("[", "")
                        .Replace("]", ""));
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
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null)
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
                var field = type.GetField(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
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

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label,
            List<AnimatorControllerParameter> animatorParameters)
        {
            var paramNameHash = property.intValue;
            var index = 0;

            for (var i = 0; i < animatorParameters.Count; i++)
            {
                if (paramNameHash != animatorParameters[i].nameHash) continue;
                index = i + 1; // +1 because the first option is reserved for (None)
                break;
            }

            var displayOptions = GetDisplayOptions(animatorParameters);

            var newIndex = EditorGUI.Popup(rect, label.text, index, displayOptions);
            var newValue = newIndex == 0 ? 0 : animatorParameters[newIndex - 1].nameHash;

            if (property.intValue != newValue)
            {
                property.intValue = newValue;
            }
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label,
            List<AnimatorControllerParameter> animatorParameters)
        {
            var paramName = property.stringValue;
            var index = 0;

            for (var i = 0; i < animatorParameters.Count; i++)
            {
                if (!paramName.Equals(animatorParameters[i].name, StringComparison.Ordinal)) continue;
                index = i + 1; // +1 because the first option is reserved for (None)
                break;
            }

            var displayOptions = GetDisplayOptions(animatorParameters);

            var newIndex = EditorGUI.Popup(rect, label.text, index, displayOptions);
            var newValue = newIndex == 0 ? null : animatorParameters[newIndex - 1].name;

            if (!property.stringValue.Equals(newValue, StringComparison.Ordinal))
            {
                property.stringValue = newValue;
            }
        }

        private static string[] GetDisplayOptions(List<AnimatorControllerParameter> animatorParams)
        {
            var displayOptions = new string[animatorParams.Count + 1];
            displayOptions[0] = "(None)";

            for (var i = 0; i < animatorParams.Count; i++)
            {
                displayOptions[i + 1] = animatorParams[i].name;
            }

            return displayOptions;
        }

        private static AnimatorController GetAnimatorController(SerializedProperty property, string animatorName)
        {
            var target = GetTargetObjectWithProperty(property);

            var animatorFieldInfo =
                GetAllFields(target, f => f.Name.Equals(animatorName, StringComparison.Ordinal)).FirstOrDefault();
            if (animatorFieldInfo != null &&
                animatorFieldInfo.FieldType == typeof(Animator))
            {
                var animator = animatorFieldInfo.GetValue(target) as Animator;
                if (animator != null)
                {
                    AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
                    return animatorController;
                }
            }

            var animatorPropertyInfo =
                GetAllProperties(target, p => p.Name.Equals(animatorName, StringComparison.Ordinal)).FirstOrDefault();
            if (animatorPropertyInfo != null &&
                animatorPropertyInfo.PropertyType == typeof(Animator))
            {
                var animator = animatorPropertyInfo.GetValue(target) as Animator;
                if (animator != null)
                {
                    var animatorController = animator.runtimeAnimatorController as AnimatorController;
                    if (animator != null)
                    {
                        return animatorController;
                    }
                }
            }

            var animatorGetterMethodInfo =
                GetAllMethods(target, m => m.Name.Equals(animatorName, StringComparison.Ordinal)).FirstOrDefault();
            if (animatorGetterMethodInfo == null ||
                animatorGetterMethodInfo.ReturnType != typeof(Animator) ||
                animatorGetterMethodInfo.GetParameters().Length != 0) return null;
            {
                var animator = animatorGetterMethodInfo.Invoke(target, null) as Animator;
                if (animator == null) return null;
                var animatorController = animator.runtimeAnimatorController as AnimatorController;
                return animatorController;
            }
        }
    }
}
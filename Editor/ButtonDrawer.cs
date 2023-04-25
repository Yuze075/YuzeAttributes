using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ButtonAttribute buttonAttribute) return;
            var methodName = buttonAttribute.MethodName;
            var target = property.serializedObject.targetObject;
            var type = target.GetType();
            var method = type.GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (method == null)
            {
                var warningContent = new GUIContent(property.displayName + "(Method could not be found.)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
                return;
            }

            if (method.GetParameters().Length > 0)
            {
                var warningContent = new GUIContent(property.displayName + "(Method cannot have parameters.)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
                return;
            }

            var canInvoke = buttonAttribute.SelectedEnableMode switch
            {
                ButtonEnableMode.Always => true,
                ButtonEnableMode.Editor => !EditorApplication.isPlaying,
                ButtonEnableMode.Playmode => EditorApplication.isPlaying,
                _ => false
            };

            if (!canInvoke)
            {
                var warningContent = new GUIContent(property.displayName + "(Method can't Invoke.)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
                return;
            }

            var text = string.IsNullOrEmpty(buttonAttribute.Text)
                ? method.Name
                : buttonAttribute.Text;

            if (GUI.Button(rect, text))
            {
                method.Invoke(target, null);
            }
        }
    }
}
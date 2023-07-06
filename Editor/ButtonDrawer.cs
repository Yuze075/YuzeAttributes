using System;
using System.Linq;
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
            EditorGUI.BeginProperty(rect, label, property);
            var buttonAttribute = (ButtonAttribute)attribute;

            var target = AttributeHelperEditor.GetTargetObjectWithProperty(property);
            var methodInfos = AttributeHelperRuntime.GetAllMethods(target.GetType(), buttonAttribute.MethodName);

            // 如果没有对应函数, 提升出错
            var enumerable = methodInfos as MethodInfo[] ?? methodInfos.ToArray();
            if (enumerable.Length <= 0)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(无法找到对应函数!)");
                EditorGUI.EndProperty();
                return;
            }

            var methodInfo = enumerable.FirstOrDefault(info => info.GetParameters().Length <= 0);
            if (methodInfo == null)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(不存在无参函数!)");
                EditorGUI.EndProperty();
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
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(当前不能触发函数!)");
                EditorGUI.EndProperty();
                return;
            }
            
            var text = string.IsNullOrEmpty(buttonAttribute.Text)
                ? methodInfo.Name
                : buttonAttribute.Text;
            
            if (GUI.Button(rect, text))
            {
                methodInfo.Invoke(target, null);
            }
            EditorGUI.EndProperty();
        }
    }
}
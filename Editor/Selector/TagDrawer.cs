using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

            // 添加基础的Tag和所有自定义Tag
            var tagList = new List<string>
            {
                "(None)",
                "Untagged"
            };
            tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

            var propertyString = property.stringValue;
            var index = 0;
            // 检查当前变量是否在Tags列表中
            for (var i = 1; i < tagList.Count; i++)
            {
                if (!tagList[i].Equals(propertyString, StringComparison.Ordinal)) continue;
                index = i;
                break;
            }

            // 绘制下拉菜单
            var newIndex = EditorGUI.Popup(rect, label.text, index, tagList.ToArray());

            // 获取到最新的tag值, 并进行赋值
            var newValue = newIndex > 0 ? tagList[newIndex] : string.Empty;

            if (!property.stringValue.Equals(newValue, StringComparison.Ordinal))
            {
                property.stringValue = newValue;
            }

            EditorGUI.EndProperty();
        }
    }
}
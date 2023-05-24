using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(StringInClassAttribute))]
    public class StringInClassDrawer : PropertyDrawer
    {
        private bool _isDropdownMode = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var stringInClassAttribute =
                (StringInClassAttribute)attribute;
            return base.GetPropertyHeight(property, label) * (stringInClassAttribute.DrawNewTextToMatchRule ? 2 : 1);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            if (property.propertyType == SerializedPropertyType.String)
            {
                var stringInClassAttribute =
                    (StringInClassAttribute)attribute;
                var targetType = stringInClassAttribute.TargetType;

                // 获取到对应的matchRule
                if (stringInClassAttribute.DrawNewTextToMatchRule)
                {
                    rect = new Rect(rect.position, new Vector2(rect.width, rect.height / 2));
                    if (stringInClassAttribute.NewTextTargetType != null)
                    {
                        _isDropdownMode =
                            EditorGUI.Toggle(new Rect(rect.position, new Vector2(rect.height, rect.height)),
                                _isDropdownMode);
                        if (_isDropdownMode)
                        {
                            stringInClassAttribute.NewTextMatchRule = DrawNewTextString(new Rect(rect.x + rect.height,
                                rect.y, rect.width - rect.height, rect.height));
                        }
                        else
                        {
                            stringInClassAttribute.NewTextMatchRule = EditorGUI.TextField(
                                new Rect(rect.x + rect.height, rect.y, rect.width - rect.height, rect.height),
                                "MatchRule", stringInClassAttribute.NewTextMatchRule);
                        }
                    }
                    else
                    {
                        stringInClassAttribute.NewTextMatchRule = EditorGUI.TextField(rect, "MatchRule", stringInClassAttribute.NewTextMatchRule);
                    }

                    rect = new Rect(new Vector2(rect.x, rect.y + rect.height), rect.size);
                }
                else
                {
                    stringInClassAttribute.NewTextMatchRule = stringInClassAttribute.MatchRule;
                }

                // 创建两个列表分别存name和value
                var listName = new List<string> { "<empty>" };
                var listValue = new List<string> { "<empty>" };

                // 找到类中所有的字段
                var fields = targetType.GetFields(BindingFlags.Static | BindingFlags.Public);
                foreach (var field in fields)
                {
                    // 获取name和value
                    var name = field.Name;
                    var value = field.GetValue(null) is string ? (string)field.GetValue(null) : "";

                    // 如果值为空, 或者不满足筛选条件则不添加进列表中
                    if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, stringInClassAttribute.NewTextMatchRule)) continue;
                    listName.Add(name);
                    listValue.Add(value);
                }

                // 获取当前字符串选择的值的index
                var selectedIndex = 0;
                if (listValue.Contains(property.stringValue))
                {
                    selectedIndex = listValue.IndexOf(property.stringValue);
                }

                // 选择列表显示的是name和value
                var popupList = stringInClassAttribute.UseValueToName ? listValue : listName;

                // 绘制列表, 根据设置选择是否绘制Label
                selectedIndex = stringInClassAttribute.HasLabel
                    ? EditorGUI.Popup(rect, label.text, selectedIndex, popupList.ToArray())
                    : EditorGUI.Popup(rect, selectedIndex, popupList.ToArray());

                // 更新字段的值
                property.stringValue = selectedIndex switch
                {
                    0 => "",
                    > 0 when selectedIndex < listValue.Count => listValue[selectedIndex],
                    _ => property.stringValue
                };
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

        private string DrawNewTextString(Rect rect)
        {
            var stringInClassAttribute =
                (StringInClassAttribute)attribute;
            var targetType = stringInClassAttribute.NewTextTargetType;

            // 创建两个列表分别存name和value
            var listName = new List<string> { "<empty>" };
            var listValue = new List<string> { "<empty>" };

            // 找到类中所有的字段
            var fields = targetType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                // 获取name和value
                var name = field.Name;
                var value = field.GetValue(null) is string ? (string)field.GetValue(null) : "";

                // 如果值为空, 则不添加进列表中
                if (string.IsNullOrWhiteSpace(value)) continue;
                listName.Add(name);
                listValue.Add(value);
            }

            // 获取当前字符串选择的值的index
            var selectedIndex = 0;
            if (listValue.Contains(stringInClassAttribute.NewTextMatchRule))
            {
                selectedIndex = listValue.IndexOf(stringInClassAttribute.NewTextMatchRule);
            }

            // 绘制列表, 根据设置选择是否绘制Label
            selectedIndex = EditorGUI.Popup(rect, "MatchRule", selectedIndex, listName.ToArray());

            // 更新字段的值
            return selectedIndex switch
            {
                0 => "",
                > 0 when selectedIndex < listValue.Count => listValue[selectedIndex],
                _ => stringInClassAttribute.NewTextMatchRule
            };
        }
    }
}
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
        private bool _stringIsInClass;
        private bool _meetMatchRule;
        private bool _openMatchRule;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUI.GetPropertyHeight(property);
            var ret = height;
            if (property.propertyType != SerializedPropertyType.String)
            {
                return ret;
            }

            if (!_meetMatchRule)
            {
                ret += height;
            }

            if (_openMatchRule)
            {
                ret += height;
            }

            return ret;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // 判断是否为string类型, 不是直接报错
            if (property.propertyType != SerializedPropertyType.String)
            {
                DrawPromptBox(rect, property.displayName + "(Incorrect Attribute Used)");
                return;
            }

            var stringInClassAttribute = (StringInClassAttribute)attribute;
            var height = EditorGUI.GetPropertyHeight(property);
            var matchRule = "";

            // 绘制提示框
            if (!_meetMatchRule)
            {
                DrawPromptBox(new Rect(rect.position, new Vector2(rect.width, height)),
                    _stringIsInClass ? "String is not meeting match rule!" : "String is not in class!");

                // 重新设置矩形大小
                rect = new Rect(new Vector2(rect.x, rect.y + height),
                    new Vector2(rect.width, rect.height - height));
            }

            // 绘制MatchRule输入框
            if (_openMatchRule)
            {
                // 绘制字符串显示框体
                stringInClassAttribute.MatchRule = EditorGUI.TextField(
                    new Rect(rect.position, new Vector2(rect.width - height, height)), "MatchRule: ",
                    stringInClassAttribute.MatchRule);

                // 获取MatchRuleType中的所有字符串
                var (matchListName, matchListValue) = GetStringInClass(stringInClassAttribute.MatchRuleType);
                var matchIndex = matchListValue.IndexOf(stringInClassAttribute.MatchRule);

                // 绘制下拉菜单框体
                matchIndex = EditorGUI.Popup(new Rect(
                        new Vector2(rect.x + rect.width - height, rect.y), new Vector2(height, height)),
                    matchIndex, matchListName.ToArray());

                // 设置MatchRule字符串数据
                stringInClassAttribute.MatchRule = matchIndex switch
                {
                    0 => "",
                    > 0 when matchIndex < matchListValue.Count => matchListValue[matchIndex],
                    _ => stringInClassAttribute.MatchRule
                };

                matchRule = stringInClassAttribute.MatchRule;

                // 重新设置矩形大小
                rect = new Rect(new Vector2(rect.x, rect.y + height), new Vector2(rect.width, rect.height - height));
            }

            // 绘制MatchRule开关
            _openMatchRule = EditorGUI.Toggle(
                new Rect(rect.position, new Vector2(height, height)),
                _openMatchRule);

            // 绘制不同的PropertyField
            if (stringInClassAttribute.HasLabel)
            {
                property.stringValue = EditorGUI.TextField(new Rect(
                        new Vector2(rect.x + height, rect.y),
                        new Vector2(rect.width - 2 * height, height)),
                    property.displayName, property.stringValue);
            }
            else
            {
                property.stringValue = EditorGUI.TextField(new Rect(
                        new Vector2(rect.x + height, rect.y),
                        new Vector2(rect.width - 2 * height, height)),
                    property.stringValue);
            }

            List<string> listName;
            List<string> listValue;

            // 判断是否在整个Class中
            (_, listValue) = GetStringInClass(stringInClassAttribute.TargetType);
            var index = listValue.IndexOf(property.stringValue);
            _stringIsInClass = index >= 0;

            // 判断是否为MatchRule所需要的
            (listName, listValue) =
                GetStringInClass(stringInClassAttribute.TargetType, matchRule);
            index = listValue.IndexOf(property.stringValue);
            _meetMatchRule = index >= 0;

            // 绘制下拉菜单
            index = EditorGUI.Popup(
                new Rect(new Vector2(rect.x + rect.width - height, rect.y), new Vector2(height, height)),
                index, stringInClassAttribute.UseValueToName ? listValue.ToArray() : listName.ToArray());

            // 绑定数据
            property.stringValue = index switch
            {
                0 => "",
                > 0 when index < listValue.Count => listValue[index],
                _ => property.stringValue
            };
        }

        /// <summary>
        /// 绘制提示框
        /// </summary>
        public static void DrawPromptBox(Rect rect, string prompt)
        {
            var warningContent = new GUIContent(prompt)
            {
                image = EditorGUIUtility.IconContent("console.warnicon").image
            };
            EditorGUI.LabelField(rect, warningContent);
        }

        /// <summary>
        /// 获取对于的<see cref="string"/>列表, 默认返回列表包含一个空值
        /// </summary>
        public static (List<string> listName, List<string> listValue) GetStringInClass(Type targetType,
            string matchRule = "")
        {
            var listName = new List<string> { "<Empty>" };
            var listValue = new List<string> { "<Empty>" };

            if (targetType == null) return (listName, listValue);

            var fields = targetType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                // 获取name和value
                var name = field.Name;
                var value = GetStingValue(field);

                // 如果值为空, 或者不满足筛选条件则不添加进列表中
                if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, matchRule)) continue;
                listName.Add(name);
                listValue.Add(value);
            }

            return (listName, listValue);

            static string GetStingValue(FieldInfo fieldInfo)
            {
                var value = fieldInfo.GetValue(null);
                if (value is string s)
                {
                    return s;
                }

                return "";
            }
        }
    }
}
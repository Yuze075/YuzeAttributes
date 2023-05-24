using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(MatchRuleAttribute))]
    public class MatchRuleDrawer : DecoratorDrawer
    {
        private bool _isDropdownMode;

        public override void OnGUI(Rect rect)
        {
            var matchRuleAttribute = (MatchRuleAttribute)attribute;
            var targetType = matchRuleAttribute.TargetType;

            if (targetType != null)
            {
                _isDropdownMode = EditorGUI.Toggle(new Rect(rect.position, new Vector2(rect.height, rect.height)), _isDropdownMode);

                if (_isDropdownMode)
                {
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
                    if (listValue.Contains(matchRuleAttribute.MatchRule))
                    {
                        selectedIndex = listValue.IndexOf(matchRuleAttribute.MatchRule);
                    }

                    // 选择列表显示的是name和value
                    var popupList = matchRuleAttribute.UseValueToName ? listValue : listName;

                    // 绘制列表, 根据设置选择是否绘制Label
                    selectedIndex = EditorGUI.Popup(
                        new Rect(rect.x + rect.height, rect.y, rect.width - rect.height, rect.height),
                        "MatchRule", selectedIndex, popupList.ToArray());

                    // 更新字段的值
                    matchRuleAttribute.MatchRule = selectedIndex switch
                    {
                        0 => "",
                        > 0 when selectedIndex < listValue.Count => listValue[selectedIndex],
                        _ => matchRuleAttribute.MatchRule
                    };
                }
                else
                {
                    matchRuleAttribute.MatchRule =
                        EditorGUI.TextField(
                            new Rect(rect.x + rect.height, rect.y, rect.width - rect.height, rect.height),
                            "MatchRule", "");
                }
            }
            else
            {
                matchRuleAttribute.MatchRule = EditorGUI.TextField(rect, "MatchRule", "");
            }
        }
    }
}
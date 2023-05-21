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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get targetsType
            var stringInClassAttribute =
                (StringInClassAttribute)attribute;
            var targetType = stringInClassAttribute.TargetType;
            var matchRule = string.IsNullOrWhiteSpace(stringInClassAttribute.MatchRuleValueName)
                ? stringInClassAttribute.MatchRule
                : property.serializedObject.FindProperty(stringInClassAttribute.MatchRuleValueName).stringValue;

            var listName = new List<string> { "<empty>" };
            var listValue = new List<string> { "<empty>" };

            var fields = targetType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                var name = field.Name;
                var value = (string)field.GetValue(null);
                if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, matchRule)) continue;
                listName.Add(name);
                listValue.Add(value);
            }

            var selectedIndex = 0;
            if (listValue.Contains(property.stringValue))
            {
                selectedIndex = listValue.IndexOf(property.stringValue);
            }

            var popupList = stringInClassAttribute.UseValueToName ? listValue : listName;

            selectedIndex = stringInClassAttribute.HasLabel
                ? EditorGUI.Popup(position, label.text, selectedIndex, popupList.ToArray())
                : EditorGUI.Popup(position, selectedIndex, popupList.ToArray());

            property.stringValue = selectedIndex switch
            {
                0 => "",
                > 0 when selectedIndex < listValue.Count => listValue[selectedIndex],
                _ => property.stringValue
            };

            EditorGUI.EndProperty();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var matchRule = stringInClassAttribute.MatchRule;

            var listStr = new List<string> { "<empty>" };
            listStr.AddRange(targetType.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(t => (string)t.GetValue(null))
                .Where(str => !string.IsNullOrEmpty(str) && Regex.IsMatch(str, matchRule)));

            var selectedIndex = 0;
            if (listStr.Contains(property.stringValue))
            {
                selectedIndex = listStr.IndexOf(property.stringValue);
            }

            selectedIndex = EditorGUI.Popup(position, label.text,
                selectedIndex, listStr.ToArray());

            property.stringValue = selectedIndex switch
            {
                0 => "",
                > 0 when selectedIndex < listStr.Count => listStr[selectedIndex],
                _ => property.stringValue
            };

            EditorGUI.EndProperty();
        }
    }
}
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

            if (property.propertyType == SerializedPropertyType.String)
            {
                // generate the tagList + custom tags
                var tagList = new List<string>
                {
                    "(None)",
                    "Untagged"
                };
                tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

                var propertyString = property.stringValue;
                var index = 0;
                // check if there is an entry that matches the entry and get the index
                // we skip index 0 as that is a special custom case
                for (var i = 1; i < tagList.Count; i++)
                {
                    if (!tagList[i].Equals(propertyString, StringComparison.Ordinal)) continue;
                    index = i;
                    break;
                }

                // Draw the popup box with the current selected index
                var newIndex = EditorGUI.Popup(rect, label.text, index, tagList.ToArray());

                // Adjust the actual string value of the property based on the selection
                var newValue = newIndex > 0 ? tagList[newIndex] : string.Empty;

                if (!property.stringValue.Equals(newValue, StringComparison.Ordinal))
                {
                    property.stringValue = newValue;
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
    }
}
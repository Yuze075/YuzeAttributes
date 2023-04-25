using System;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(InlineAttribute))]
    public class InlineDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var obj = property.objectReferenceValue;
            if (obj == null || !property.isExpanded)
                return EditorGUI.GetPropertyHeight(property, includeChildren: true);

            using var serializedObject = new SerializedObject(obj);
            var totalHeight = EditorGUIUtility.singleLineHeight;

            using (var iterator = serializedObject.GetIterator())
            {
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        var childProperty = serializedObject.FindProperty(iterator.name);
                        if (childProperty.name.Equals("m_Script", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var height = EditorGUI.GetPropertyHeight(childProperty, includeChildren: true);
                        totalHeight += height;
                    } while (iterator.NextVisible(false));
                }
            }

            totalHeight += EditorGUIUtility.standardVerticalSpacing;
            return totalHeight;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var obj = property.objectReferenceValue;
            if (obj == null)
            {
                EditorGUI.PropertyField(rect, property, label, true);
            }
            else
            {
                // Draw a foldout
                var foldoutRect = new Rect
                {
                    x = rect.x,
                    y = rect.y,
                    width = EditorGUIUtility.labelWidth,
                    height = EditorGUIUtility.singleLineHeight
                };

                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label,
                    toggleOnLabelClick: true);

                // Draw the scriptable object field
                var propertyRect = new Rect()
                {
                    x = rect.x,
                    y = rect.y,
                    width = rect.width,
                    height = EditorGUIUtility.singleLineHeight
                };

                EditorGUI.PropertyField(propertyRect, property, label, true);

                // Draw the child properties
                if (property.isExpanded)
                {
                    DrawChildProperties(rect, property);
                }
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        private static void DrawChildProperties(Rect rect, SerializedProperty property)
        {
            var obj = property.objectReferenceValue;
            if (obj == null) return;

            var boxRect = new Rect()
            {
                x = 0.0f,
                y = rect.y + EditorGUIUtility.singleLineHeight,
                width = rect.width * 2.0f,
                height = rect.height - EditorGUIUtility.singleLineHeight
            };

            GUI.Box(boxRect, GUIContent.none);

            using (new EditorGUI.IndentLevelScope())
            {
                var serializedObject = new SerializedObject(obj);
                serializedObject.Update();

                using (var iterator = serializedObject.GetIterator())
                {
                    var yOffset = EditorGUIUtility.singleLineHeight;

                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            var childProperty = serializedObject.FindProperty(iterator.name);
                            if (childProperty.name.Equals("m_Script", StringComparison.Ordinal))
                            {
                                continue;
                            }


                            var childHeight = EditorGUI.GetPropertyHeight(childProperty, includeChildren: true);
                            var childRect = new Rect()
                            {
                                x = rect.x,
                                y = rect.y + yOffset,
                                width = rect.width,
                                height = childHeight
                            };

                            EditorGUI.PropertyField(childRect, childProperty, true);

                            yOffset += childHeight;
                        } while (iterator.NextVisible(false));
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
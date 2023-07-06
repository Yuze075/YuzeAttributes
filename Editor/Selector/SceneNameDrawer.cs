using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameDrawer : PropertyDrawer
    {
        #region static

        private static bool SceneExists(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
                var name = scenePath.Substring(lastSlash + 1,
                    scenePath.LastIndexOf(".", StringComparison.Ordinal) - lastSlash - 1);

                if (string.Compare(name, sceneName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Rect DrawWarningMessage(Rect position)
        {
            position = new Rect(position.x,
                position.y,
                position.width, Style.BoxHeight);
            EditorGUI.HelpBox(position, "Scene does not exist. " +
                                        "Check available Scenes in the Build options.", MessageType.Warning);
            return position;
        }

        private static void HandleTargetPicker(Rect position, SerializedProperty property)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            if (GUI.Button(position, Style.PickerButtonContent, EditorStyles.miniButton))
            {
                EditorGUIUtility.ShowObjectPicker<SceneAsset>(null, false, null, controlId);
            }

            if (Event.current.commandName != "ObjectSelectorUpdated") return;
            if (controlId != EditorGUIUtility.GetObjectPickerControlID()) return;
            var target = EditorGUIUtility.GetObjectPickerObject();
            if (!target) return;
            property.serializedObject.Update();
            property.stringValue = target.name;
            property.serializedObject.ApplyModifiedProperties();
            GUI.changed = true;
        }

        private static class Style
        {
            internal static readonly float RowHeight = EditorGUIUtility.singleLineHeight;
            internal static readonly float BoxHeight = EditorGUIUtility.singleLineHeight * 2.1f;
            internal static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;
            internal const float PickerWidth = 30.0f;
            internal static readonly GUIContent PickerButtonContent;

            static Style()
            {
                PickerButtonContent = EditorGUIUtility.IconContent("SceneAsset Icon");
                PickerButtonContent.tooltip = "Pick Scene";
            }
        }

        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return SceneExists(property.stringValue)
                ? EditorGUI.GetPropertyHeight(property, label)
                : EditorGUI.GetPropertyHeight(property, label) + Style.BoxHeight + Style.Spacing * 2;
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            if (property.propertyType != SerializedPropertyType.String)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

            if (!SceneExists(property.stringValue))
            {
                rect = DrawWarningMessage(rect);
                rect.yMin = rect.yMax + Style.Spacing;
                rect.yMax = rect.yMin + Style.RowHeight;
            }

            rect.xMax -= Style.PickerWidth + Style.Spacing;
            EditorGUI.PropertyField(rect, property, label);
            rect.xMax += Style.PickerWidth + Style.Spacing;
            rect.xMin = rect.xMax - Style.PickerWidth;

            HandleTargetPicker(rect, property);

            EditorGUI.EndProperty();
        }
    }
}
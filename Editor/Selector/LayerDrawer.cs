using System;
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public sealed class LayerDrawer : PropertyDrawer
    {
        private static string[] Layers => UnityEditorInternal.InternalEditorUtility.layers;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    DrawPropertyForString(rect, property, label, Layers);
                    break;
                case SerializedPropertyType.Integer:
                    DrawPropertyForInt(rect, property, label, Layers);
                    break;
                default:
                    var warningContent = new GUIContent(property.displayName + "(Incorrect Attribute Used)")
                    {
                        image = EditorGUIUtility.IconContent("console.warnicon").image
                    };
                    EditorGUI.LabelField(rect, warningContent);
                    break;
            }

            EditorGUI.EndProperty();
        }

        private static void DrawPropertyForString(Rect rect, SerializedProperty property, GUIContent label,
            string[] layers)
        {
            var index = IndexOf(layers, property.stringValue);
            var newIndex = EditorGUI.Popup(rect, label.text, index, layers);
            var newLayer = layers[newIndex];

            if (!property.stringValue.Equals(newLayer, StringComparison.Ordinal))
            {
                property.stringValue = layers[newIndex];
            }
        }

        private static void DrawPropertyForInt(Rect rect, SerializedProperty property, GUIContent label,
            string[] layers)
        {
            var index = 0;
            var layerName = LayerMask.LayerToName(property.intValue);
            for (var i = 0; i < layers.Length; i++)
            {
                if (!layerName.Equals(layers[i], StringComparison.Ordinal)) continue;
                index = i;
                break;
            }

            var newIndex = EditorGUI.Popup(rect, label.text, index, layers);
            var newLayerName = layers[newIndex];
            var newLayerNumber = LayerMask.NameToLayer(newLayerName);

            if (property.intValue != newLayerNumber)
            {
                property.intValue = newLayerNumber;
            }
        }

        private static int IndexOf(string[] layers, string layer)
        {
            var index = Array.IndexOf(layers, layer);
            return Mathf.Clamp(index, 0, layers.Length - 1);
        }
    }
}
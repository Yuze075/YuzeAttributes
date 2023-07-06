using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AssetPreviewAttribute))]
    public class AssetPreviewDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return EditorGUI.GetPropertyHeight(property);
            var previewTexture = GetAssetPreview(property);
            if (previewTexture != null)
            {
                return EditorGUI.GetPropertyHeight(property) +
                       GetAssetPreviewSize(property, attribute as AssetPreviewAttribute).y;
            }

            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

            var propertyRect = new Rect()
            {
                x = rect.x,
                y = rect.y,
                width = rect.width,
                height = EditorGUIUtility.singleLineHeight
            };

            EditorGUI.PropertyField(propertyRect, property, label);

            var previewTexture = GetAssetPreview(property);
            if (previewTexture != null)
            {
                var previewRect = new Rect()
                {
                    x = rect.x + GetIndentLength(rect),
                    y = rect.y + EditorGUIUtility.singleLineHeight,
                    width = rect.width,
                    height = GetAssetPreviewSize(property, attribute as AssetPreviewAttribute).y
                };

                GUI.Label(previewRect, previewTexture);
            }

            EditorGUI.EndProperty();
        }

        private static float GetIndentLength(Rect sourceRect)
        {
            var indentRect = EditorGUI.IndentedRect(sourceRect);
            var indentLength = indentRect.x - sourceRect.x;
            return indentLength;
        }

        private static Texture2D GetAssetPreview(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference) return null;
            if (property.objectReferenceValue == null) return null;
            var previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
            return previewTexture;
        }

        private static Vector2 GetAssetPreviewSize(SerializedProperty property, AssetPreviewAttribute attribute)
        {
            var previewTexture = GetAssetPreview(property);
            if (previewTexture == null)
            {
                return Vector2.zero;
            }

            var targetWidth = AssetPreviewAttribute.DefaultWidth;
            var targetHeight = AssetPreviewAttribute.DefaultHeight;

            if (attribute != null)
            {
                targetWidth = attribute.Width;
                targetHeight = attribute.Height;
            }

            var width = Mathf.Clamp(targetWidth, 0, previewTexture.width);
            var height = Mathf.Clamp(targetHeight, 0, previewTexture.height);

            return new Vector2(width, height);
        }
    }
}
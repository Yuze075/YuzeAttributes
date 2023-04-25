using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(DividerLineAttribute))]
    public class DividerLineDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var lineAttr = (DividerLineAttribute)attribute;
            return EditorGUIUtility.singleLineHeight + lineAttr.Height;
        }

        public override void OnGUI(Rect position)
        {
            var rect = EditorGUI.IndentedRect(position);
            rect.y += EditorGUIUtility.singleLineHeight / 3.0f;
            var lineAttr = (DividerLineAttribute)attribute;
            rect.height = lineAttr.Height;
            EditorGUI.DrawRect(rect, DividerLineAttribute.GetColor(lineAttr.Color));
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return GetHelpBoxHeight();
        }

        public override void OnGUI(Rect rect)
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;

            var indentLength = GetIndentLength(rect);
            var infoBoxRect = new Rect(
                rect.x + indentLength,
                rect.y,
                rect.width - indentLength,
                GetHelpBoxHeight());

            DrawInfoBox(infoBoxRect, infoBoxAttribute.Text, infoBoxAttribute.Type);
        }

        private float GetHelpBoxHeight()
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;
            var minHeight = EditorGUIUtility.singleLineHeight * 2.0f;
            var desiredHeight =
                GUI.skin.box.CalcHeight(new GUIContent(infoBoxAttribute.Text), EditorGUIUtility.currentViewWidth);
            var height = Mathf.Max(minHeight, desiredHeight);
            return height;
        }

        private static void DrawInfoBox(Rect rect, string infoText, InfoBoxAttribute.EInfoBoxType infoBoxType)
        {
            var messageType = infoBoxType switch
            {
                InfoBoxAttribute.EInfoBoxType.Normal => MessageType.Info,
                InfoBoxAttribute.EInfoBoxType.Warning => MessageType.Warning,
                InfoBoxAttribute.EInfoBoxType.Error => MessageType.Error,
                _ => MessageType.None
            };
            EditorGUI.HelpBox(rect, infoText, messageType);
        }

        private static float GetIndentLength(Rect sourceRect)
        {
            var indentRect = EditorGUI.IndentedRect(sourceRect);
            var indentLength = indentRect.x - sourceRect.x;

            return indentLength;
        }
    }
}
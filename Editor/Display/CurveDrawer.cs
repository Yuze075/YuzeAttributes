using UnityEditor;
using UnityEngine;

namespace YuzeToolkit.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(CurveAttribute))]
    public class CurveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType == SerializedPropertyType.AnimationCurve)
            {
                var curveRangeAttribute = (CurveAttribute)attribute;
                var curveRanges = new Rect(
                    curveRangeAttribute.Min.x,
                    curveRangeAttribute.Min.y,
                    curveRangeAttribute.Max.x - curveRangeAttribute.Min.x,
                    curveRangeAttribute.Max.y - curveRangeAttribute.Min.y);

                EditorGUI.CurveField(
                    rect,
                    property,
                    curveRangeAttribute.Color == CurveAttribute.ColorType.Clear
                        ? Color.green
                        : CurveAttribute.GetColor(curveRangeAttribute.Color),
                    curveRanges,
                    label);
            }
            else
            {
                var warningContent = new GUIContent(property.displayName + "(Incorrect Attribute Used)")
                {
                    image = EditorGUIUtility.IconContent("console.warnicon").image
                };
                EditorGUI.LabelField(rect, warningContent);
                return;
            }


            EditorGUI.EndProperty();
        }
    }
}
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

            if (property.propertyType != SerializedPropertyType.AnimationCurve)
            {
                AttributeHelperEditor.DrawWarningMessage(rect, property.displayName + "(错误的特性使用!)");
                EditorGUI.EndProperty();
                return;
            }

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
            
            EditorGUI.EndProperty();
        }
    }
}
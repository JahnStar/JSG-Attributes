// Developed by Halil Emre Yildiz - 2024
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyReadonlyAttribute : PropertyAttribute
    {
        public string label;
        public HeyReadonlyAttribute(string label = "") => this.label = label;
    }
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyReadonlyAttribute))]
    public class HeyReadonlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            HeyReadonlyAttribute _attribute = attribute as HeyReadonlyAttribute;
            if (_attribute.label != "") label.text = _attribute.label;

            if (property.propertyType == SerializedPropertyType.Generic)
            {
                if (property.isExpanded) EditorGUI.PropertyField(position, property, label, true);
                else 
                {
                    var firstChild = property.Copy();
                    if (firstChild.NextVisible(true))
                    {
                        label.text = firstChild.propertyType switch
                        {
                            SerializedPropertyType.String => $"{label.text} : {firstChild.stringValue}",
                            SerializedPropertyType.Integer => $"{label.text} : {firstChild.intValue}",
                            SerializedPropertyType.Float => $"{label.text} : {firstChild.floatValue}",
                            SerializedPropertyType.Boolean => $"{label.text} : {firstChild.boolValue}",
                            SerializedPropertyType.Enum => $"{label.text} : {firstChild.enumNames[firstChild.enumValueIndex]}",
                            _ => $"{label.text} : {firstChild.propertyType}",
                        };
                    }
                    property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
                }
            }
            else EditorGUI.PropertyField(position, property, label, true);

            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
        => property.isExpanded ? EditorGUI.GetPropertyHeight(property, label, true) : EditorGUIUtility.singleLineHeight;
    }
    #endif
}
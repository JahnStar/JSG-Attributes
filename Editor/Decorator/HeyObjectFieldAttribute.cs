// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyObjectFieldAttribute : PropertyAttribute
    {
        public bool minimizeWhenNull, required;
        public HeyObjectFieldAttribute(bool required = false, bool minimizeWhenNull = false)
        {
            this.minimizeWhenNull = minimizeWhenNull;
            this.required = required;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyObjectFieldAttribute))]
    public class HeyObjectFieldDrawer :  PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyObjectFieldAttribute attribute = (HeyObjectFieldAttribute)this.attribute;
            if (IsPropertyNull(property))
            {
                var prevColor = GUI.backgroundColor;
                if (attribute.required) GUI.backgroundColor = Color.red;
                if (attribute.minimizeWhenNull)
                {
                    EditorGUI.LabelField(position, label);
                    float fieldWidth = EditorGUIUtility.singleLineHeight;
                    var minimizedRect = new Rect(position.x + position.width - fieldWidth, position.y, fieldWidth, position.height);
                    EditorGUI.ObjectField(minimizedRect, property, GUIContent.none);
                }
                else EditorGUI.PropertyField(position, property, label, true);
                GUI.backgroundColor = prevColor;
            }
            else EditorGUI.PropertyField(position, property, label, true);
        }

        private bool IsPropertyNull(SerializedProperty property)
        {
            // Check if the serialized property is null. This might need adjustments based on property type.
            return property.objectReferenceValue == null;
        }
    }
#endif
}
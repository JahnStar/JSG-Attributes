using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyHelpBoxAttribute : PropertyAttribute
    {
        public readonly string message;
        public readonly BoxType messageType;
        public readonly float heightLineCount = 2;
        public string conditionName;
        public object conditionValue;
        public HeyHelpBoxAttribute(string message, BoxType messageType, string conditionName, object conditionValue, float lineCount = 2)
        {
            this.message = message;
            this.messageType = messageType;
            this.conditionName = conditionName;
            this.conditionValue = conditionValue;
            this.heightLineCount = lineCount;
        }
        public HeyHelpBoxAttribute(string message, BoxType messageType = BoxType.Info)
        {
            this.message = message;
            this.messageType = messageType;
        }


        public enum BoxType
        {
            None,
            Info,
            Warning,
            Error
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyHelpBoxAttribute))]
    public class HeyHelpBoxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyHelpBoxAttribute helpBoxAttribute = attribute as HeyHelpBoxAttribute;

            if (IsConditionMet(helpBoxAttribute, property))
            {
                var helpBoxHeight = EditorGUIUtility.singleLineHeight * helpBoxAttribute.heightLineCount;

                var helpBoxPosition = position;
                helpBoxPosition.height = helpBoxHeight;
                helpBoxHeight += 4;
                position.y += helpBoxHeight;
                position.height -= helpBoxHeight;

                EditorGUI.HelpBox(helpBoxPosition, helpBoxAttribute.message, (MessageType)helpBoxAttribute.messageType);
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HeyHelpBoxAttribute helpBoxAttribute = attribute as HeyHelpBoxAttribute;

            if (IsConditionMet(helpBoxAttribute, property))
            {
                var baseHeight = base.GetPropertyHeight(property, label);
                var additionalHeight = EditorGUIUtility.singleLineHeight * helpBoxAttribute.heightLineCount;
                return baseHeight + additionalHeight + 4;
            }
            else return base.GetPropertyHeight(property, label);
        }

        private bool IsConditionMet(HeyHelpBoxAttribute helpBoxAttribute, SerializedProperty property)
        {
            if (string.IsNullOrEmpty(helpBoxAttribute.conditionName)) return true; // No condition specified, always met

            SerializedProperty conditionProperty = property.serializedObject.FindProperty(helpBoxAttribute.conditionName);
            if (conditionProperty == null) return false; // Condition property not found, consider condition not met
            else if (helpBoxAttribute.conditionValue == null) return conditionProperty.propertyType == SerializedPropertyType.ObjectReference && conditionProperty.objectReferenceValue == null;

            // Check condition based on property type
            switch (conditionProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return conditionProperty.boolValue.Equals(helpBoxAttribute.conditionValue);
                case SerializedPropertyType.Integer:
                    return conditionProperty.intValue.Equals(helpBoxAttribute.conditionValue);
                case SerializedPropertyType.Float:
                    return conditionProperty.floatValue.Equals(helpBoxAttribute.conditionValue);
                case SerializedPropertyType.String:
                    return conditionProperty.stringValue.Equals(helpBoxAttribute.conditionValue);
                // Extend with other types as needed
                default:
                    return false; // Unsupported property type for condition
            }
        }
    }
#endif
}
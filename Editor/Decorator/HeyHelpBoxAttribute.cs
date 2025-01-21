using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    public enum HelpBoxType
    {
        None,
        Info,
        Warning,
        Error
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyHelpBoxAttribute : PropertyAttribute
    {
        public readonly string message;
        public readonly HelpBoxType messageType;
        public readonly float heightLineCount = 2;
        public string conditionName;
        public object conditionValue;
        public HeyHelpBoxAttribute(string message, HelpBoxType messageType, string conditionName, object conditionValue, float lineCount = 2)
        {
            this.message = message;
            this.messageType = messageType;
            this.conditionName = conditionName;
            this.conditionValue = conditionValue;
            this.heightLineCount = lineCount;
        }
        public HeyHelpBoxAttribute(string message, HelpBoxType messageType = HelpBoxType.Info)
        {
            this.message = message;
            this.messageType = messageType;
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
                    return conditionProperty.boolValue == Convert.ToBoolean(helpBoxAttribute.conditionValue);
                case SerializedPropertyType.Integer:
                    return conditionProperty.intValue == Convert.ToInt32(helpBoxAttribute.conditionValue);
                case SerializedPropertyType.Float:
                    const float epsilon = 0.0001f;
                    float conditionFloat = Convert.ToSingle(helpBoxAttribute.conditionValue);
                    return Math.Abs(conditionProperty.floatValue - conditionFloat) < epsilon;
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
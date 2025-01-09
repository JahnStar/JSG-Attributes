// Developed by Halil Emre Yildiz (2023-2024)
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    public class HeyShowIfAttribute : PropertyAttribute
    {
        public string conditionName;
        public object conditionValue;
        public string fieldLabel;
        public bool fieldBox;

        public HeyShowIfAttribute(string conditionName, object conditionValue, bool fieldBox = false, string label = "")
        {
            this.conditionName = conditionName;
            this.conditionValue = conditionValue;
            this.fieldLabel = label;
            this.fieldBox = fieldBox;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public static readonly GUIStyle style = new(EditorStyles.helpBox) { normal = new GUIStyleState() { background = Texture2D.grayTexture } };
        public const int boxHeight = 4;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyShowIfAttribute attribute = (HeyShowIfAttribute)this.attribute;
            if (!string.IsNullOrEmpty(attribute.fieldLabel)) label.text = attribute.fieldLabel;
            if (CheckCondition(attribute, property))
            {
                if (attribute.fieldBox)
                {
                    bool changeColor = GUI.backgroundColor == Color.white;
                    if (changeColor) GUI.backgroundColor = Color.cyan * 0.5f;
                    Rect boxRect = position;
                    boxRect.height += 2;
                    GUI.BeginGroup(boxRect, style);
                    position.y += boxHeight / 2 + 1;
                    position.height = EditorGUIUtility.singleLineHeight + boxHeight;
                    GUI.EndGroup();
                    if (changeColor) GUI.backgroundColor = Color.white;
                }
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HeyShowIfAttribute attribute = (HeyShowIfAttribute)this.attribute;
            if (!string.IsNullOrEmpty(attribute.fieldLabel)) label.text = attribute.fieldLabel;
            return CheckCondition(attribute, property) ? EditorGUI.GetPropertyHeight(property, label) + (attribute.fieldBox ? boxHeight : 0) : -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool CheckCondition(HeyShowIfAttribute attribute, SerializedProperty property)
        {
            string conditionPath = property.propertyPath.Replace(property.name, attribute.conditionName);
            SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionPath);

            if (conditionProperty == null)
            {
                Debug.LogWarning("Property not found: " + conditionPath);
                return false;
            }
            else if (attribute.conditionValue == null) return conditionProperty.propertyType == SerializedPropertyType.ObjectReference && conditionProperty.objectReferenceValue == null;

            switch (conditionProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: return conditionProperty.boolValue == (bool)attribute.conditionValue;
                case SerializedPropertyType.Integer: return conditionProperty.intValue == (int)attribute.conditionValue;
                case SerializedPropertyType.Float: return conditionProperty.floatValue == (float)attribute.conditionValue;
                case SerializedPropertyType.String: return conditionProperty.stringValue == (string)attribute.conditionValue;
                case SerializedPropertyType.Enum:
                    int enumValue = conditionProperty.intValue;
                    int targetValue = (int)attribute.conditionValue;
                    bool isFlag = attribute.conditionValue != null && attribute.conditionValue.GetType().IsDefined(typeof(FlagsAttribute), false);
                    return isFlag ? (enumValue & targetValue) == targetValue : enumValue == targetValue;
                default: if (!Application.isPlaying) Debug.LogWarning("ShowIfAttribute currently supports only boolean, integer, float, string and enum conditions."); break;
            }

            return false;
        }
    }
#endif
}
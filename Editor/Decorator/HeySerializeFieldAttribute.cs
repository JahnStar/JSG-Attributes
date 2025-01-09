// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeySerializeFieldAttribute : PropertyAttribute
    {
        public string foldoutField;
        public string[] fieldNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeySerializeFieldAttribute"/> class with the specified field names.
        /// </summary>
        /// <param name="fieldNames">The names of the serialized fields. For example, [HeySerializeField("mainfield.subfield", "mainfield.anotherSubfield")]. The fields must also be [HideInInspector] and public.</param>
        public HeySerializeFieldAttribute(params string[] fieldNames)
        {
            this.fieldNames = fieldNames;
        }
        /// <param name="foldoutField"> The name of the serialized field that will be used as a foldout. The field must also be [HideInInspector] and public.</param>
        /// <param name="fieldNames">The names of the serialized fields. For example, [HeySerializeField("mainfield.subfield", "mainfield.anotherSubfield")]. The fields must also be [HideInInspector] and public.</param>
        public HeySerializeFieldAttribute(bool foldout, string foldoutField, params string[] fieldNames)
        {
            if (foldout)
            {
                this.foldoutField = foldoutField;
                this.fieldNames = fieldNames;
            }
            else if (string.IsNullOrEmpty(foldoutField)) this.foldoutField = "-";
            else this.fieldNames = fieldNames;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeySerializeFieldAttribute))]
    public class HeySerializeFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeySerializeFieldAttribute heyAttribute = (HeySerializeFieldAttribute)attribute;
            float singleFieldHeight = EditorGUIUtility.singleLineHeight;
            var currentPosition = new Rect(position.x, position.y, position.width, singleFieldHeight);

            if (!string.IsNullOrEmpty(heyAttribute.foldoutField))
            {
                if (heyAttribute.foldoutField == "-")
                {
                    EditorGUI.LabelField(currentPosition, label);
                    return;
                }
                SerializedProperty foldoutProperty = property.serializedObject.FindProperty(heyAttribute.foldoutField);
                if (foldoutProperty != null && foldoutProperty.propertyType == SerializedPropertyType.Boolean)
                {
                    foldoutProperty.boolValue = EditorGUI.Foldout(currentPosition, foldoutProperty.boolValue, label);
                    currentPosition.y += singleFieldHeight + EditorGUIUtility.standardVerticalSpacing;

                    if (foldoutProperty.boolValue)
                    {
                        foreach (string fieldName in heyAttribute.fieldNames)
                        {
                            string fieldLabel = fieldName.Contains(":") ? fieldName.Split(':')[1] : fieldName;
                            string _fieldName = fieldName.Contains(":") ? fieldName.Split(':')[0] : fieldName;
                            SerializedProperty targetProperty = property.serializedObject.FindProperty(_fieldName);
                            if (targetProperty != null) EditorGUI.PropertyField(currentPosition, targetProperty, new GUIContent(fieldLabel), true);
                            else EditorGUI.LabelField(currentPosition, new GUIContent(fieldLabel), new GUIContent("Field not found: " + _fieldName));

                            currentPosition.y += singleFieldHeight + EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
                else
                {
                    EditorGUI.LabelField(currentPosition, new GUIContent("Foldout field not found or not a boolean: " + heyAttribute.foldoutField));
                }
            }
            else
            {
                foreach (string fieldName in heyAttribute.fieldNames)
                {
                    string fieldLabel = fieldName.Contains(":") ? fieldName.Split(':')[1] : fieldName;
                    string _fieldName = fieldName.Contains(":") ? fieldName.Split(':')[0] : fieldName;
                    SerializedProperty targetProperty = property.serializedObject.FindProperty(_fieldName);
                    if (targetProperty != null) EditorGUI.PropertyField(currentPosition, targetProperty, new GUIContent(fieldLabel), true);
                    else EditorGUI.LabelField(currentPosition, new GUIContent(fieldLabel), new GUIContent("Field not found: " + _fieldName));

                    currentPosition.y += singleFieldHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HeySerializeFieldAttribute heyAttribute = (HeySerializeFieldAttribute)attribute;
            if (!string.IsNullOrEmpty(heyAttribute.foldoutField))
            {
                SerializedProperty foldoutProperty = property.serializedObject.FindProperty(heyAttribute.foldoutField);
                if (foldoutProperty != null && foldoutProperty.propertyType == SerializedPropertyType.Boolean && foldoutProperty.boolValue)
                {
                    return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (heyAttribute.fieldNames.Length + 1);
                }
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * heyAttribute.fieldNames.Length;
        }
    }
#endif
}
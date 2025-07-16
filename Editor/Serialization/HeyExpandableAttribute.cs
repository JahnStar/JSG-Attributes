// Developed by Halil Emre Yildiz (2023-2024)
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyExpandableAttribute : PropertyAttribute
    {
        public bool Readonly;
        public HeyExpandableAttribute() { }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyExpandableAttribute))]
    public class HeyExpandableDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference) return EditorGUI.GetPropertyHeight(property, label, true);
            else if (property.objectReferenceValue == null) return base.GetPropertyHeight(property, label);

            Type propertyType = GetElementType(fieldInfo.FieldType);
            if (!typeof(ScriptableObject).IsAssignableFrom(propertyType)) return base.GetPropertyHeight(property, label);

            ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            if (scriptableObject == null) return base.GetPropertyHeight(property, label);

            if (property.isExpanded)
            {
                var serializedObject = new SerializedObject(scriptableObject);
                float totalHeight = EditorGUIUtility.singleLineHeight;

                SerializedProperty iterator = serializedObject.GetIterator();
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (iterator.name.Equals("m_Script", StringComparison.Ordinal)) continue;

                        float height = EditorGUI.GetPropertyHeight(iterator, true);
                        totalHeight += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (iterator.NextVisible(false));
                }

                totalHeight += EditorGUIUtility.standardVerticalSpacing;
                return totalHeight + 7;
            }
            else return EditorGUIUtility.singleLineHeight + 1;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup((attribute as HeyExpandableAttribute).Readonly);
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.ObjectReference) EditorGUI.PropertyField(position, property, label, true);
            else if (property.objectReferenceValue == null) EditorGUI.PropertyField(position, property, label, true);                else
                {
                    Type propertyType = GetElementType(fieldInfo.FieldType);
                    if (typeof(ScriptableObject).IsAssignableFrom(propertyType))
                {
                    ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
                    
                    string labelText = label.text;
                    label.text = " ";
                    var fieldPosition = new Rect(position.x + 20, position.y, position.width - 20, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(fieldPosition, property, label, true);
                    label.text = labelText;

                    if (scriptableObject) 
                    {
                        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

                        if (property.isExpanded)
                        {
                            var helpBoxRect = new Rect(position.x + 15, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 1, position.width - 15, GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);
                            EditorGUI.HelpBox(helpBoxRect, "", MessageType.None);

                            var childPropertyRect = new Rect(helpBoxRect.x + 5, helpBoxRect.y + 5, helpBoxRect.width - 10, helpBoxRect.height - 10);
                            DrawChildProperties(childPropertyRect, property);
                        }
                    }
                }
                else EditorGUI.HelpBox(position, $"{typeof(HeyExpandableAttribute).Name} can only be used on ScriptableObject types.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawChildProperties(Rect position, SerializedProperty property)
        {
            ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;
            if (scriptableObject == null) return;

            var serializedObject = new SerializedObject(scriptableObject);
            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true))
            {
                float yOffset = position.y;
                do
                {
                    if (iterator.name.Equals("m_Script", StringComparison.Ordinal)) continue;

                    float height = EditorGUI.GetPropertyHeight(iterator, true);
                    var propertyRect = new Rect(position.x, yOffset, position.width, height);
                    EditorGUI.PropertyField(propertyRect, iterator, true);
                    yOffset += height + EditorGUIUtility.standardVerticalSpacing;
                }
                while (iterator.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Type GetElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
            
            if (type.IsGenericType && typeof(System.Collections.IList).IsAssignableFrom(type))
                return type.GetGenericArguments()[0];
            
            return type;
        }
    }
#endif
}

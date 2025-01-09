// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyDynamicButtonAttribute : PropertyAttribute
    {
        public string methodName;
        public bool linkStyle;
        public bool autoClick;
        public HeyDynamicButtonAttribute(string methodName, bool linkStyle = true, bool autoClick = false)
        {
            this.methodName = methodName;
            this.linkStyle = linkStyle;
            this.autoClick = autoClick;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyDynamicButtonAttribute))]
    public class LinkedHeaderDrawer : PropertyDrawer
    {
        private HeyDynamicButtonAttribute Attribute => (HeyDynamicButtonAttribute)attribute;
        private MethodInfo methodInfo;
        private string method;
        //
        private readonly GUIStyle linkHeader = new GUIStyle(EditorStyles.linkLabel) { fontStyle = FontStyle.Bold };
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String) return property.name.First() == '_' && string.IsNullOrEmpty(property.stringValue) ? 10f : base.GetPropertyHeight(property, label);
            else if (property.propertyType == SerializedPropertyType.Float) return base.GetPropertyHeight(property, label);
            else if (property.propertyType == SerializedPropertyType.Integer) return base.GetPropertyHeight(property, label);
            else if (property.propertyType == SerializedPropertyType.Boolean) return base.GetPropertyHeight(property, label);
            else return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string title = property.name.First() != '_' ? property.displayName + ": " : "";
            //EditorGUI.LabelField(position, headerAttribute.title, EditorStyles.boldLabel);
            if (property.propertyType == SerializedPropertyType.String) title += property.stringValue;
            else if (property.propertyType == SerializedPropertyType.Float) title += property.floatValue.ToString();
            else if (property.propertyType == SerializedPropertyType.Integer) title += property.intValue.ToString();
            else if (property.propertyType == SerializedPropertyType.Boolean) title += property.boolValue.ToString();
            else
            {
                EditorGUI.HelpBox(position, "Error [LinkedHeader]: field type is not supported.", MessageType.Error);
                return;
            }
            if (GUI.Button(position, title, Attribute.linkStyle ? linkHeader : EditorStyles.miniButton) || Attribute.autoClick)
            {
                if (method != Attribute.methodName)
                {
                    methodInfo = property.serializedObject.targetObject.GetType().GetMethod(Attribute.methodName);
                    method = Attribute.methodName;
                }
                methodInfo?.Invoke(property.serializedObject.targetObject, null);
                if (methodInfo is null) Debug.LogError("Error LinkedHeader[?]: Method not found!");
            }
        }
    }
#endif
}
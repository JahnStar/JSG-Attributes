// Developed by Halil Emre Yildiz - 2023
using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyButtonAttribute : PropertyAttribute
    {
        public string methodName;
        public string label;
        public bool linkStyle;
        public bool serializeField;
        public HeyButtonAttribute(string methodName, string label, bool linkStyle = false, bool serializeField = false)
        {
            this.methodName = methodName;
            this.label = label;
            this.linkStyle = linkStyle;
            this.serializeField = serializeField;
        }
    }
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        private readonly GUIStyle linkHeader = new GUIStyle(EditorStyles.linkLabel) { fontStyle = FontStyle.Bold };
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            HeyButtonAttribute buttonAttribute = (HeyButtonAttribute)this.attribute;
            //
            return EditorGUI.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing + (buttonAttribute.serializeField ? EditorGUIUtility.singleLineHeight : 0);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the button attribute
            HeyButtonAttribute buttonAttribute = (HeyButtonAttribute)attribute;
            // Get the target object and method name
            object target = GetTarget(property);
            string methodName = buttonAttribute.methodName;
            // Get the type of the target object
            Type type = target.GetType();
            // Find the method with the given name
            MethodInfo method = type.GetMethod(methodName);
            // Check if the method exists and has no parameters
            if (method == null)
            {
                GUI.Label(position, "Method could not be found. Is it public?");
                return;
            }
            if (buttonAttribute.serializeField)
            {
                EditorGUI.PropertyField(position, property, label, true);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            var buttonRect = position;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            if (GUI.Button(buttonRect, !string.IsNullOrEmpty(buttonAttribute.label) ? buttonAttribute.label : method.Name, buttonAttribute.linkStyle ? linkHeader : EditorStyles.miniButton)) 
            {
                if (method.GetParameters().Length > 0)
                {
                    Func<SerializedProperty, object> fieldValue = prop =>
                    {
                        if (prop.propertyType == SerializedPropertyType.String) return prop.stringValue;
                        else if (prop.propertyType == SerializedPropertyType.Float) return prop.floatValue;
                        else if (prop.propertyType == SerializedPropertyType.Integer) return prop.intValue;
                        else if (prop.propertyType == SerializedPropertyType.Boolean) return prop.boolValue;
                        else return prop.objectReferenceValue;
                    };
                    method.Invoke(target, new object[] { fieldValue(property) });
                }
                else method.Invoke(target, null);
            }
        }
        internal static object GetTarget(SerializedProperty property)
        {
            // Get the root object
            object target = property.serializedObject.targetObject;
            // Split the property path into parts
            string[] parts = property.propertyPath.Split('.');
            // Traverse the path to get the target object
            for (int i = 0; i < parts.Length - 1; i++)
            {
                // Get the current part
                string part = parts[i];

                // Check if the part is an array element
                if (part.Contains("["))
                {
                    // Split the part into array name and index
                    string arrayName = part.Substring(0, part.IndexOf("["));
                    int index = Convert.ToInt32(part.Substring(part.IndexOf("[")).Replace("[", "").Replace("]", ""));

                    // Get the array field value and element value
                    target = GetValue(target, arrayName);
                    target = ((System.Collections.IList)target)[index];
                    target = ((System.Collections.ArrayList)target)[index];
                }
                else
                {
                    // Get the field value
                    target = GetValue(target, part);
                }
            }

            // Return the final target object
            return target;
        }
        internal static object GetValue(object source, string name)
        {
            if (source == null) return null;
            // Get the type of the source object
            Type type = source.GetType();
            // Get the field with the given name
            FieldInfo field = type.GetField(name);
            // Check if the field exists
            if (field == null) return null;
            // Get the field value from the source object
            return field.GetValue(source);
        }
    }
#endif
}
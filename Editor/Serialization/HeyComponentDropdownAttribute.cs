using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyComponentDropdownAttribute : PropertyAttribute
    {
        public string label;
        public HeyComponentDropdownAttribute(string label = "")
        {
            this.label = label;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyComponentDropdownAttribute))]
    public class HeyComponentDropdownAttributeDrawer : PropertyDrawer
    {
        static HeyComponentDropdownAttributeDrawer()
        {
            EditorApplication.projectChanged += () => monoBehaviourTypesCache = null; 
        }
        private static List<string> monoBehaviourTypesCache = null;

        private string firstValue;
        private bool showDropdown;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyComponentDropdownAttribute attribute = (HeyComponentDropdownAttribute)this.attribute;
            firstValue = property.stringValue;
            if (!string.IsNullOrEmpty(attribute.label)) label.text = attribute.label;

            monoBehaviourTypesCache ??= GetAllMonoBehaviourTypes();
            if (property.propertyType == SerializedPropertyType.String) 
            {
                // draw field with button on the right, if button is clicked, show dropdown
                Rect fieldRect = position;
                fieldRect.width -= 22;
                if (showDropdown) 
                {
                    // set enum value
                    string selectedEnumString = DrawDropdown(fieldRect, label, monoBehaviourTypesCache);
                    if (selectedEnumString != property.stringValue) 
                    {
                        property.stringValue = selectedEnumString;
                        property.serializedObject.ApplyModifiedProperties();
			            property.serializedObject.Update();
                    }
                }
                else EditorGUI.PropertyField(fieldRect, property, label);
                // draw a button with find icon
                var buttonRect = new Rect(fieldRect.x + fieldRect.width + 2, fieldRect.y, 20, fieldRect.height);
                if (GUI.Button(buttonRect, string.IsNullOrEmpty(property.stringValue) ? EditorGUIUtility.IconContent("console.erroricon") : EditorGUIUtility.IconContent("dll Script Icon"))) showDropdown = !showDropdown;
            }
            else EditorGUI.HelpBox(position, $"{label.text} is not a string", MessageType.Error);
        }

        private string DrawDropdown(Rect position, GUIContent label, List<string> list)
        {
            var monoBehaviours = new List<string> { "" }.Concat(monoBehaviourTypesCache).ToList();
            int currentIndex = Mathf.Max(0, monoBehaviours.FindIndex(t => t == firstValue));
            var options = monoBehaviours.Select(t => new GUIContent(t)).ToArray();

            int selectedIndex = EditorGUI.Popup(position, label, currentIndex, options);
            return monoBehaviours[selectedIndex];
        }

        public static List<string> GetAllMonoBehaviourTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)))
                .Select(type => type.FullName)
                .ToList();
        }
    }
#endif
}
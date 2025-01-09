﻿// Developed by Halil Emre Yildiz (2023-2024)
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    /// <summary>
    /// Specifies that a field should be displayed as a method dropdown in the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyMethodDropdownAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the name of the component type field.
        /// </summary>
        public string ComponentTypeField { get; private set; }

        /// <summary>
        /// Gets the name of the method with ParameterInfo[] parameter.
        /// </summary>
        public string ParameterInfoListMethodName { get; private set; }

        /// <param name="componentTypeField">The name of the component type field.</param>
        /// <param name="ParameterInfoListMethod">The name of the method with ParameterInfo[] parameter.</param>
        public HeyMethodDropdownAttribute(string componentTypeField, string ParameterInfoListMethod = null)
        {
            ComponentTypeField = componentTypeField;
            ParameterInfoListMethodName = ParameterInfoListMethod;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyMethodDropdownAttribute))]
    public class HeyMethodDropdownAttributeDrawer : PropertyDrawer
    {
        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        private static Dictionary<Type, List<string>> methodNamesCache = new Dictionary<Type, List<string>>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyMethodDropdownAttribute attribute = (HeyMethodDropdownAttribute)this.attribute;
            SerializedProperty componentTypeNameProperty = property.serializedObject.FindProperty(attribute.ComponentTypeField);

            string ComponentTypeName = componentTypeNameProperty.stringValue;
            Type componentType = GetTypeByName(ComponentTypeName);
            Rect fieldRect = position;
            fieldRect.width -= 22;

            if (componentTypeNameProperty != null && componentType == null)
            {
                EditorGUI.LabelField(position, label.text, "Select a component type");
                return;
            }
            else if (componentType == null)
            {
                EditorGUI.LabelField(position, label.text, "Component type not found");
                return;
            }

            List<string> methodNames = GetMethodNames(componentType);
            int currentIndex = Mathf.Max(0, methodNames.IndexOf(property.stringValue));
            currentIndex = EditorGUI.Popup(fieldRect, label.text, currentIndex, methodNames.ToArray());

            if (currentIndex >= 0 && currentIndex < methodNames.Count) 
            {
                // set enum value
                string selectedEnumString = methodNames[currentIndex];
                if (selectedEnumString != property.stringValue) 
                {
                    property.stringValue = selectedEnumString;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
            }

            string methodName = attribute.ParameterInfoListMethodName;
            // Get the type of the target object
            object target = ButtonDrawer.GetTarget(property);
            Type type = target.GetType();
            // Find the method with the given name
            MethodInfo parameterInfoListMethod = type.GetMethod(methodName);

            MethodInfo selectedMethod = componentType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .FirstOrDefault(method => method.Name == property.stringValue);

            if (selectedMethod != null)
            {
                ParameterInfo[] parameterTypes = selectedMethod.GetParameters();
                if (GUI.Button(new Rect(position.x + position.width - 20, position.y, 20, position.height), ">", EditorStyles.miniButton)) parameterInfoListMethod?.Invoke(target, new object[] { parameterTypes });
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        private Type GetTypeByName(string typeName)
        {
            if (!typeCache.TryGetValue(typeName, out Type type))
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
                typeCache[typeName] = type;
            }
            return type;
        }

        private List<string> GetMethodNames(Type type)
        {
            if (!methodNamesCache.TryGetValue(type, out List<string> methodNames))
            {
                methodNames = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(method => method.Name)
                    .Distinct()
                    .ToList();
                methodNamesCache[type] = methodNames;
            }
            return methodNames;
        }

        private List<(string name, string type)> GetMethodParameters(MethodInfo method)
        {
            return method.GetParameters()
                .Select(parameter => (parameter.Name, parameter.ParameterType.FullName ?? parameter.ParameterType.Name))
                .ToList();
        }
    }
#endif
}
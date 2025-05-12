using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace JahnStarGames.Attributes
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeySerializableDictionary<,>))]
    public class HeySerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 2f;
        private const float WarningBoxHeight = 1.5f;
        private static float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private static float Padding = 5f;

        // Dictionary to hold ReorderableList instances per property path
        private static readonly Dictionary<string, ReorderableList> ListCache = new Dictionary<string, ReorderableList>();

        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            // Generate a unique key for this property
            string key = property.propertyPath;

            // Return cached list if exists
            if (ListCache.TryGetValue(key, out ReorderableList cachedList) &&
                cachedList != null &&
                cachedList.serializedProperty?.serializedObject == property.serializedObject)
            {
                return cachedList;
            }

            // Create new list
            SerializedProperty listProperty = property.FindPropertyRelative("list");
            ReorderableList newList = new ReorderableList(property.serializedObject, listProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Key-Value Pairs"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                    DrawListElement(rect, listProperty, index, isActive, isFocused),
                elementHeightCallback = index => GetElementHeight(listProperty, index),
                onAddCallback = list =>
                {
                    var index = list.count;
                    list.serializedProperty.InsertArrayElementAtIndex(index);
                    list.index = index;
                },
                onChangedCallback = (list) =>
                {
                    // Ensure the changes are applied
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            };

            // Cache and return
            ListCache[key] = newList;
            return newList;
        }

        private void DrawListElement(Rect rect, SerializedProperty listProperty, int index, bool isActive, bool isFocused)
        {
            // Validate list and index
            if (listProperty == null || index < 0 || index >= listProperty.arraySize)
            {
                EditorGUI.LabelField(rect, "Invalid element");
                return;
            }

            SerializedProperty element;
            try
            {
                element = listProperty.GetArrayElementAtIndex(index);
                if (element == null)
                {
                    EditorGUI.LabelField(rect, "Element is null");
                    return;
                }
            }
            catch (Exception ex)
            {
                EditorGUI.LabelField(rect, $"Element error: {ex.Message}");
                return;
            }

            SerializedProperty keyProperty = null;
            SerializedProperty valueProperty = null;

            try
            {
                keyProperty = element.FindPropertyRelative("Key");
                valueProperty = element.FindPropertyRelative("Value");
            }
            catch (Exception ex)
            {
                EditorGUI.LabelField(rect, $"Property access error: {ex.Message}");
                return;
            }

            if (keyProperty == null)
            {
                EditorGUI.LabelField(rect, "Key property is missing");
                return;
            }

            rect.y += VerticalSpacing;

            float keyWidth = rect.width * 0.35f;
            float valueHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty, true) : SingleLineHeight;

            EditorGUI.BeginChangeCheck();

            var keyRect = new Rect(rect.x, rect.y, keyWidth, SingleLineHeight);
            var valueRect = new Rect(rect.x + keyWidth + Padding, rect.y, rect.width - keyWidth - Padding, SingleLineHeight);

            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);

            if (valueProperty == null)
            {
                EditorGUI.LabelField(valueRect, "Non-Serializable Value");
            }
            else if (IsBasicType(valueProperty.propertyType))
            {
                // Special handling for Object references
                if (valueProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    EditorGUI.BeginProperty(valueRect, GUIContent.none, valueProperty);
                    valueProperty.objectReferenceValue = EditorGUI.ObjectField(
                        valueRect,
                        GUIContent.none,
                        valueProperty.objectReferenceValue,
                        typeof(UnityEngine.Object),
                        true
                    );
                    EditorGUI.EndProperty();
                }
                else
                {
                    // Simple direct field for other basic types
                    EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
                }
            }
            else if (valueProperty.hasVisibleChildren)
            {
                // For complex types with children
                SerializedProperty firstChildProperty = null;
                bool hasBasicFirstChild = false;

                // Try to get the first child property if it exists
                if (valueProperty.hasVisibleChildren)
                {
                    SerializedProperty originalValue = valueProperty.Copy();
                    valueProperty.Next(true); // Enter the property
                    firstChildProperty = valueProperty.Copy();
                    valueProperty = originalValue; // Go back to original property

                    hasBasicFirstChild = firstChildProperty != null && IsBasicType(firstChildProperty.propertyType);
                }

                // Get type name
                string typeName = GetTypeName(valueProperty);

                // Calculate label width based on content
                GUIContent typeNameContent = new GUIContent(typeName);
                float labelWidth = EditorStyles.foldout.CalcSize(typeNameContent).x + Padding * 3f;
                float fieldWidth = valueRect.width - labelWidth;

                // Draw the foldout with type name
                var foldoutRect = new Rect(valueRect.x + Padding * 2f, valueRect.y, labelWidth, SingleLineHeight);
                valueProperty.isExpanded = EditorGUI.Foldout(foldoutRect, valueProperty.isExpanded, typeName, true);

                // If we have a basic first child property, show it next to the foldout
                if (hasBasicFirstChild)
                {
                    var firstPropertyRect = new Rect(
                        valueRect.x + labelWidth,
                        valueRect.y,
                        fieldWidth,
                        SingleLineHeight
                    );

                    // Special handling for Object references to ensure proper display
                    if (firstChildProperty.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        EditorGUI.BeginProperty(firstPropertyRect, GUIContent.none, firstChildProperty);
                        firstChildProperty.objectReferenceValue = EditorGUI.ObjectField(
                            firstPropertyRect,
                            GUIContent.none,
                            firstChildProperty.objectReferenceValue,
                            typeof(UnityEngine.Object),
                            true
                        );
                        EditorGUI.EndProperty();
                    }
                    else
                    {
                        EditorGUI.PropertyField(firstPropertyRect, firstChildProperty, GUIContent.none);
                    }
                }

                // If expanded, show all properties underneath
                if (valueProperty.isExpanded)
                {
                    var expandedRect = new Rect(
                        rect.x,
                        rect.y + SingleLineHeight + VerticalSpacing,
                        rect.width,
                        valueHeight - SingleLineHeight
                    );

                    EditorGUI.PropertyField(expandedRect, valueProperty, GUIContent.none, true);
                }
            }
            else
            {
                // Other non-basic values with no children
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
            }

            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    // Apply the changes immediately
                    listProperty.serializedObject.ApplyModifiedProperties();

                    // Sync to dictionary (call SyncListToDictionary on the target object)
                    var targetObject = listProperty.serializedObject.targetObject;
                    var methodInfo = targetObject?.GetType().GetMethod("SyncListToDictionary",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    methodInfo?.Invoke(targetObject, new object[] { index });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error updating dictionary: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        // Check if the property is a basic type (float, int, bool, string, enum, color) or a Unity Object
        private bool IsBasicType(SerializedPropertyType propertyType)
        {
            return propertyType == SerializedPropertyType.Float ||
                   propertyType == SerializedPropertyType.Integer ||
                   propertyType == SerializedPropertyType.Boolean ||
                   propertyType == SerializedPropertyType.String ||
                   propertyType == SerializedPropertyType.Enum ||
                   propertyType == SerializedPropertyType.Color ||
                   propertyType == SerializedPropertyType.ObjectReference;
        }

        private float GetElementHeight(SerializedProperty listProperty, int index)
        {
            // Ensure listProperty is valid and index is in range
            if (listProperty == null || index < 0 || index >= listProperty.arraySize)
                return SingleLineHeight + VerticalSpacing * 2;

            var element = listProperty.GetArrayElementAtIndex(index);
            var valueProperty = element.FindPropertyRelative("Value");

            if (valueProperty == null)
            {
                return SingleLineHeight + VerticalSpacing * 2;
            }

            // Basic types always use single line
            if (IsBasicType(valueProperty.propertyType))
            {
                return SingleLineHeight + VerticalSpacing * 2;
            }

            // Complex types that aren't expanded use single line too
            if (!valueProperty.isExpanded)
            {
                return SingleLineHeight + VerticalSpacing * 2;
            }

            // For expanded complex types, calculate full height
            float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty, true);
            return SingleLineHeight + VerticalSpacing + valuePropertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reorderableList = GetReorderableList(property);

            EditorGUI.BeginProperty(position, label, property);

            var headerRect = new Rect(position.x, position.y, position.width, SingleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                var listRect = new Rect(
                    position.x + SingleLineHeight,
                    position.y + SingleLineHeight + VerticalSpacing,
                    position.width - 18f,
                    reorderableList.GetHeight()
                );

                try
                {
                    reorderableList.DoList(listRect);
                }
                catch (NullReferenceException)
                {
                    EditorGUI.HelpBox(listRect, "Error: Non-Serializable Key-Value Pairs", MessageType.None);
                }

                var keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
                if (keyCollision)
                {
                    var warningRect = new Rect(
                        position.x,
                        listRect.y + listRect.height + VerticalSpacing,
                        position.width,
                        SingleLineHeight * WarningBoxHeight
                    );
                    EditorGUI.HelpBox(warningRect, "Duplicate keys will not be serialized.", MessageType.Warning);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var reorderableList = GetReorderableList(property);

            float height = SingleLineHeight;

            if (property.isExpanded)
            {
                height += VerticalSpacing;
                height += reorderableList.GetHeight();

                var keyCollision = property.FindPropertyRelative("keyCollision").boolValue;
                if (keyCollision)
                {
                    height += VerticalSpacing;
                    height += SingleLineHeight * WarningBoxHeight;
                }
            }

            return height;
        }

        public string GetTypeName(SerializedProperty property)
        => property.isArray ? $"List <{ObjectNames.NicifyVariableName(property.arrayElementType)}>" : ObjectNames.NicifyVariableName(property.type.Replace("PPtr<$", "").Replace(">", ""));
    }
#endif
}
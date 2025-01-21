using UnityEngine;
using System;

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
        private ReorderableList _reorderableList;
        private SerializedProperty _listProperty;
        private static float SingleLineHeight => EditorGUIUtility.singleLineHeight;
        private static float Padding = 5f;

        private void InitializeReorderableList(SerializedProperty property)
        {
            if (_reorderableList != null) return;

            _listProperty = property.FindPropertyRelative("list");
            _reorderableList = new ReorderableList(property.serializedObject, _listProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Key-Value Pairs"),
                drawElementCallback = DrawListElement,
                elementHeightCallback = GetElementHeight,
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
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var keyProperty = element.FindPropertyRelative("Key");
            var valueProperty = element.FindPropertyRelative("Value");

            rect.y += VerticalSpacing;
            
            float keyWidth = rect.width * 0.35f;
            float valueHeight = EditorGUI.GetPropertyHeight(valueProperty, true);

            EditorGUI.BeginChangeCheck();
            
            var keyRect = new Rect(rect.x, rect.y, keyWidth, SingleLineHeight);
            var valueRect = new Rect(rect.x + keyWidth + Padding, rect.y, rect.width - keyWidth - Padding, valueHeight);

            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
            if (valueProperty == null) EditorGUI.LabelField(valueRect, "Non-Serializable Value");
            else if (valueProperty.hasVisibleChildren)
            {
                var foldoutRect = new Rect(valueRect.x + Padding * 2f, valueRect.y, valueRect.width, SingleLineHeight);
                valueProperty.isExpanded = EditorGUI.Foldout(foldoutRect, valueProperty.isExpanded, GetTypeName(valueProperty), true);

                if (valueProperty.isExpanded)
                {
                    valueRect = new Rect(rect.x, rect.y + SingleLineHeight + 2, rect.width - SingleLineHeight, valueHeight);
                    EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, true);
                }
            }
            else EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, true);

            if (EditorGUI.EndChangeCheck())
            {
                // Apply the changes immediately
                _reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
                
                // Update the dictionary value
                var targetObject = _reorderableList.serializedProperty.serializedObject.targetObject;
                var methodInfo = targetObject.GetType().GetMethod("UpdateValueFromList", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                methodInfo?.Invoke(targetObject, new object[] { index });
            }
        }

        private float GetElementHeight(int index)
        {
            var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var valueProperty = element.FindPropertyRelative("Value");
            if (!valueProperty.isExpanded) return SingleLineHeight + VerticalSpacing * 2;
            float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty, true);

            float valueHeight = valueProperty != null ? valuePropertyHeight : SingleLineHeight;
            if (valuePropertyHeight > SingleLineHeight * 1.5f) valueHeight += SingleLineHeight;

            return valueHeight + VerticalSpacing * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeReorderableList(property);

            EditorGUI.BeginProperty(position, label, property);

            var headerRect = new Rect(position.x, position.y, position.width, SingleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                var listRect = new Rect(
                    position.x + SingleLineHeight,
                    position.y + SingleLineHeight + VerticalSpacing,
                    position.width - 18f,
                    _reorderableList.GetHeight()
                );

                try 
                {
                    _reorderableList.DoList(listRect);
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
            InitializeReorderableList(property);

            float height = SingleLineHeight;

            if (property.isExpanded)
            {
                height += VerticalSpacing;
                height += _reorderableList.GetHeight();

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
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;

namespace JahnStarGames.Attributes
{
    public class HeyObjectPickerAttribute : PropertyAttribute
    {
        public string filter;
        public bool allowSceneObjects;

        public HeyObjectPickerAttribute(string filter, bool allowSceneObjects = false)
        {
            this.filter = filter;
            this.allowSceneObjects = allowSceneObjects;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyObjectPickerAttribute))]
    public class HeyObjectPickerPropertyDrawer : PropertyDrawer
    {
        private bool isProcessingEvent = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (isProcessingEvent) 
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            isProcessingEvent = true;
            
            try
            {
                EditorGUI.PropertyField(position, property, label, false);

                Event e = Event.current;
                int controlID = GUIUtility.GetControlID(FocusType.Passive);

                if (!property.hasMultipleDifferentValues)
                {
                    if (e.type == EventType.MouseUp && position.Contains(e.mousePosition)) HandleMouseUpEvent(e, property, controlID);
                    else if (e.type == EventType.ExecuteCommand && e.commandName == "ObjectSelectorUpdated"  && EditorGUIUtility.GetObjectPickerControlID() == controlID)
                    {
                        HandleObjectSelectorUpdatedEvent(e, property);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            finally
            {
                isProcessingEvent = false;
            }
        }

        private void HandleMouseUpEvent(Event e, SerializedProperty property, int controlID)
        {
            if (e == null || property == null) return;
            
            e.Use();
            if (property.objectReferenceValue != null) return;
            
            Type type = GetFieldType();
            if (type != null) ShowObjectPickerForType(type, null, controlID);
        }

        private void HandleObjectSelectorUpdatedEvent(Event e, SerializedProperty property)
        {
            if (e == null || property == null) 
            {
                property.objectReferenceValue = null;
                return;
            }

            Object selectedObject = EditorGUIUtility.GetObjectPickerObject();
            if (selectedObject == null) 
            {
                property.objectReferenceValue = null;
                return;
            }

            Type fieldType = GetFieldType();
            if (!fieldType.IsAssignableFrom(selectedObject.GetType()))
            {
                Debug.Log($"Error: Invalid object type. Expected {fieldType}, got {selectedObject.GetType()}");
                property.objectReferenceValue = null;
                return;
            }

            var att = (HeyObjectPickerAttribute)attribute;
            if (!att.allowSceneObjects && selectedObject is GameObject go && go.scene.IsValid())
            {
                Debug.Log("Error: Scene objects are not allowed for this field.");
                property.objectReferenceValue = null;
                return;
            }

            property.objectReferenceValue = selectedObject;
            e.Use();
        }

        private Type GetFieldType()
        {
            if (fieldInfo == null) return typeof(Object);

            Type type = fieldInfo.FieldType;
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType && type.GenericTypeArguments.Length > 0) return type.GenericTypeArguments[0];
            return type;
        }

        private void ShowObjectPickerForType(Type type, Object obj, int controlID)
        {
            var att = (HeyObjectPickerAttribute)attribute;
            
            if (type == typeof(GameObject)) EditorGUIUtility.ShowObjectPicker<GameObject>(obj, att.allowSceneObjects, att.filter, controlID);
            else if (type == typeof(Mesh)) EditorGUIUtility.ShowObjectPicker<Mesh>(obj, att.allowSceneObjects, att.filter, controlID);
            else if (type == typeof(Material)) EditorGUIUtility.ShowObjectPicker<Material>(obj, att.allowSceneObjects, att.filter, controlID);
            else if (type == typeof(Texture2D)) EditorGUIUtility.ShowObjectPicker<Texture2D>(obj, att.allowSceneObjects, att.filter, controlID);
            else if (type == typeof(Sprite)) EditorGUIUtility.ShowObjectPicker<Sprite>(obj, att.allowSceneObjects, att.filter, controlID);
            else if (type.IsSubclassOf(typeof(ScriptableObject))) EditorGUIUtility.ShowObjectPicker<ScriptableObject>(obj, att.allowSceneObjects, att.filter, controlID);
            else EditorGUIUtility.ShowObjectPicker<Object>(obj, att.allowSceneObjects, att.filter, controlID);
        }
    }
#endif
}
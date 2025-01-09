// Developed by Halil Emre Yildiz - 2024
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    /// <summary>
    /// Attribute used to reference a scene object by its name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class HeyGameObjectNameAttribute : PropertyAttribute
    {
        public string label;
        public HeyGameObjectNameAttribute(string label = "")
        {
            this.label = label;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyGameObjectNameAttribute))]
    public class HeyGameObjectNameAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyGameObjectNameAttribute _attribute = (HeyGameObjectNameAttribute)attribute;
            GUIContent newLabel = new GUIContent(string.IsNullOrEmpty(_attribute.label) ? property.displayName : _attribute.label);
            // Check if the property is not a string
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "The attribute can only be used with string fields. Because it returns the name of the object.", MessageType.Error);
                return; // Exit the method
            }
            Event currentEvent = Event.current;
            Rect dropArea = EditorGUI.PrefixLabel(position, newLabel);
            dropArea.height = EditorGUIUtility.singleLineHeight + 2;
            dropArea.width -= dropArea.height;
            // Get the ViewToolMove icon
            GUIContent viewToolMoveIcon = EditorGUIUtility.IconContent("ViewToolMove");
            Rect iconPosition = new Rect(dropArea.x + dropArea.width, dropArea.y, dropArea.height, dropArea.height);

            Color defaultColor = GUI.color;
            // Draw the icon inside the box
            if (string.IsNullOrEmpty(property.stringValue))
            {
                GUI.Box(iconPosition, viewToolMoveIcon);
                GUI.color += Color.red * 0.75f;
            }
            else
            {
                GUIContent viewToolMoveOn = EditorGUIUtility.IconContent("d_P4_DeletedLocal");
                GUI.Box(iconPosition, viewToolMoveOn);

                if (GUI.Button(iconPosition, "", GUIStyle.none)) 
                {
                    // display a dialog box to confirm the deletion
                    if (EditorUtility.DisplayDialog("Delete GameObject Reference", "Do you want to clear the reference?", "Yes", "No")) 
                    {
                        property.stringValue = "";
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    }
                }
            }
            GUI.Box(dropArea, "");
            GUI.color = defaultColor;

            if (dropArea.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown && currentEvent.clickCount == 1 && !string.IsNullOrEmpty(property.stringValue))
                {
                    GameObject gameObject = GameObject.Find(property.stringValue);
                    if (gameObject != null) 
                    {
                        Selection.activeGameObject = gameObject;
                        EditorGUIUtility.PingObject(gameObject);
                    }
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    currentEvent.Use();
                }
                else if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        // Check if the dragged object is a GameObject
                        if (draggedObject is GameObject)
                        {
                            GameObject gameObject = draggedObject as GameObject;
                            // Check if an object with the same name already exists in the scene
                            if (GameObject.Find(gameObject.name) != null && GameObject.Find(gameObject.name) != gameObject)
                            {
                                EditorUtility.DisplayDialog("Duplicate Name Detected", $"An object named '{gameObject.name}' already exists in the scene. Please use a unique name.", "OK");
                            }
                            else 
                            {
                                // set string value
                                string newString = gameObject.name; // Assign the GameObject's name to the string property
                                if (newString != property.stringValue) 
                                {
                                    property.stringValue = newString;
                                    property.serializedObject.ApplyModifiedProperties();
                                    property.serializedObject.Update();
                                }
                            }
                            break; // Assuming you only want to assign one GameObject
                        }
                    }
                    currentEvent.Use();
                }
            }

            // Optionally, display the current value of the string property below the icon
            EditorGUI.LabelField(dropArea, !string.IsNullOrEmpty(property.stringValue) ? " " + property.stringValue : " Drop GameObject", EditorStyles.miniLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => base.GetPropertyHeight(property, label) + 2;
    }
#endif
}
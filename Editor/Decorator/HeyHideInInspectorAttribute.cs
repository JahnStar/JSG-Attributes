// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyHideInInspectorAttribute : PropertyAttribute
    {
        public string spaceText;
        /// <summary>
        /// Add '#' to the beginning of the text to not use disabled group.
        /// </summary>
        public HeyHideInInspectorAttribute(string spaceText = "")
        {
            this.spaceText = spaceText;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyHideInInspectorAttribute))]
    public class HeyHideInInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyHideInInspectorAttribute _attribute = attribute as HeyHideInInspectorAttribute;
            string _spaceText = _attribute.spaceText;
            if (_spaceText != "") 
            {
                bool _disabled = true;
                if (_spaceText[0] == '#') { _disabled = false; _spaceText = _spaceText[1..]; }
                EditorGUI.BeginDisabledGroup(_disabled);
                EditorGUILayout.LabelField(_spaceText, EditorStyles.boldLabel);
                EditorGUI.EndDisabledGroup();
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;
    }
#endif
}
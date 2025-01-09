// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyFormerlySerializedAsAttribute : PropertyAttribute
    {
        public string text = "";
        public bool drawChildren = true;
        public HeyFormerlySerializedAsAttribute(string text, bool drawChildren = true)
        {
            this.text = text;
            this.drawChildren = drawChildren;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyFormerlySerializedAsAttribute))]
    public class HeyFormerlySerializedAsDrawer : PropertyDrawer
    {
        HeyFormerlySerializedAsAttribute target;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            target ??= attribute as HeyFormerlySerializedAsAttribute;
            EditorGUI.PropertyField(position, property, new GUIContent(target.text), target.drawChildren);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            target ??= attribute as HeyFormerlySerializedAsAttribute;
            return EditorGUI.GetPropertyHeight(property, label, target.drawChildren);
        }
    }
#endif
}
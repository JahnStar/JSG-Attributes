// Developed by Halil Emre Yildiz - 2023
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JahnStarGames.Attributes
{
    public enum LabelColor
    {
        White,
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta,
        Grey
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class HeyLabelAttribute : PropertyAttribute
    {
        public string getFromField = "";
        public string labelText = "";
        public LabelColor labelColor = LabelColor.White;
        public bool fieldBox = false;
        public float alpha = 1f;
        public HeyLabelAttribute(string getFromField)
        {
            this.getFromField = getFromField;
        }
        public HeyLabelAttribute(LabelColor labelColor, bool fieldBox = false, float alpha = 1f)
        {
            this.labelColor = labelColor;
            this.alpha = alpha;
            this.fieldBox = fieldBox;
        }
        public HeyLabelAttribute(string labelText, LabelColor labelColor, bool fieldBox = false, float alpha = 1f)
        {
            this.labelText = labelText;
            this.labelColor = labelColor;
            this.alpha = alpha;
            this.fieldBox = fieldBox;
        }
        public HeyLabelAttribute(string labelText, bool fieldBox = false)
        {
            this.labelText = labelText;
            this.labelColor = LabelColor.White;
            this.alpha = 1f;
            this.fieldBox = fieldBox;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HeyLabelAttribute))]
    public class HeyLabelDrawer : PropertyDrawer
    {
        public static readonly GUIStyle style = new GUIStyle(EditorStyles.helpBox) { normal = new GUIStyleState() { background = Texture2D.grayTexture } };
        public const int boxHeight = 4;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true) + (((HeyLabelAttribute)attribute).fieldBox ? boxHeight: 0); 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HeyLabelAttribute labelAttribute = (HeyLabelAttribute)attribute;
            Color oldBgColor = GUI.backgroundColor;
            Color oldColor = GUI.color;
            Color newColor = GetColorFromEnum(labelAttribute.labelColor);
            float alpha = labelAttribute.alpha;
            if (labelAttribute.fieldBox)
            {
                GUI.backgroundColor = Color.Lerp(oldBgColor, newColor, alpha);
                alpha *= 0.15f;
                GUI.color = Color.white;
                Rect boxRect = position;
                boxRect.height += 2;
                GUI.BeginGroup(boxRect, style);
                position.y += boxHeight / 2 + 1;
                position.height = EditorGUIUtility.singleLineHeight + boxHeight;
                GUI.EndGroup();
            }
            GUI.color = Color.Lerp(Color.white, newColor, alpha);
            string labelText = property.displayName;
            if (!string.IsNullOrEmpty(labelAttribute.getFromField))
            {
                var field = property.serializedObject.FindProperty(labelAttribute.getFromField);
                if (field != null) labelText = field.stringValue;
            }
            else if (!string.IsNullOrEmpty(labelAttribute.labelText)) labelText = labelAttribute.labelText;
            var newLabel = new GUIContent(labelText);
            EditorGUI.PropertyField(position, property, newLabel, true);
            GUI.color = oldColor;
            GUI.backgroundColor = oldBgColor;
        }
        private Color GetColorFromEnum(LabelColor labelColor)
        {
            return labelColor switch
            {
                LabelColor.Red => Color.red,
                LabelColor.Green => Color.green,
                LabelColor.Blue => Color.blue,
                LabelColor.Yellow => Color.yellow,
                LabelColor.Cyan => Color.cyan,
                LabelColor.Magenta => Color.magenta,
                LabelColor.Grey => Color.grey,
                _ => Color.white,
            } * 0.9f;
        }
    }
#endif
}
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace JahnStarGames.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HeyMethodButtonAttribute : PropertyAttribute
    {
        public readonly string buttonName;
        public readonly string header;

        public HeyMethodButtonAttribute(string buttonName = null)
        {
            this.buttonName = buttonName;
            this.header = string.Empty;
        }

        public HeyMethodButtonAttribute(string header, string buttonName)
        {
            this.header = header;
            this.buttonName = buttonName;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class HeyMethodButtonEditor : Editor
    {
        private string currentHeader = null;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var targetObject = target as MonoBehaviour;
            if (targetObject == null) return;

            var methods = targetObject.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            currentHeader = null;

            foreach (var method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<HeyMethodButtonAttribute>();
                if (buttonAttribute == null) continue;
                if (!string.IsNullOrEmpty(buttonAttribute.header))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(buttonAttribute.header, EditorStyles.boldLabel);
                    currentHeader = buttonAttribute.header;
                }
                string buttonName = string.IsNullOrEmpty(buttonAttribute.buttonName) ?  ObjectNames.NicifyVariableName(method.Name) :  buttonAttribute.buttonName;

                if (GUILayout.Button(buttonName)) method.Invoke(targetObject, null);
            }
        }
    }
#endif
}
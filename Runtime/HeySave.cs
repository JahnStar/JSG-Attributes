// Developed by Halil Emre Yildiz - 2023
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Component = UnityEngine.Component;

namespace JahnStarGames.Attributes
{
    /// <summary><strong>
    /// The game object name must be unique and each this attribute must be used only once per game object.
    /// /// </strong></summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class HeySaveAttribute : Attribute
    {
        internal string FileName { get; }
        public HeySaveAttribute(string fileName) => FileName = fileName;
        public override string ToString() => FileName;
    }

    class Wrapper { public List<string> package; public Wrapper(List<string> heydata) => this.package = heydata; }

    public class HeySave : MonoBehaviour
    {
        private static readonly string path = Application.persistentDataPath;
        [Serializable]
        public class FieldData
        {
            public string path;
            public string mono;
            public string comp;
            public string name;
            public string data;
        }
        
        public static void SaveAll()
        {
            GroupFieldsWithFileName(typeof(HeySaveAttribute), out var fileName_fieldsData);
            foreach (var pair in fileName_fieldsData)
            {
                string mergedData = JsonUtility.ToJson(new Wrapper(pair.Value.Select(fieldData => JsonUtility.ToJson(fieldData)).ToList()));
                File.WriteAllText(Path.Combine(path, pair.Key), mergedData);
            }
        }
        public static void Save(string fileName)
        {
            GroupFieldsWithFileName(typeof(HeySaveAttribute), out var fileName_fieldsData);
            foreach (var pair in fileName_fieldsData)
            {
                if (pair.Key != fileName) continue;
                string mergedData = JsonUtility.ToJson(new Wrapper(pair.Value.Select(fieldData => JsonUtility.ToJson(fieldData)).ToList()));
                File.WriteAllText(Path.Combine(path, pair.Key), mergedData);
            }
        }
        /// <summary><remarks><strong>
        /// It should be called from within the Awake()
        /// </strong></remarks></summary>
        public static bool LoadAll()
        {
            GroupFieldsWithFileName(typeof(HeySaveAttribute), out var fileName_fieldsData, true);
            int loadedGroup = 0;
            foreach (var pair in fileName_fieldsData)
            {
                if (!File.Exists(Path.Combine(path, pair.Key))) continue;
                List<string> dataList = JsonUtility.FromJson<Wrapper>(File.ReadAllText(Path.Combine(path, pair.Key))).package;
                List<FieldData> fieldDataList = dataList?.Select(data => JsonUtility.FromJson<FieldData>(data)).ToList();
                foreach (FieldData fieldData in fieldDataList)
                {
                    MonoBehaviour mono = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).ToList().Find(mono => mono.name == fieldData.mono);
                    Component comp = mono.GetComponent(fieldData.comp);
                    if (mono is null || comp is null) Debug.LogError($"Error loading save: {fieldData.comp} not found!");
                    else
                    {
                        FieldInfo field = mono.GetComponent(fieldData.comp).GetType().GetField(fieldData.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (field is null)
                        {
                            Debug.LogError($"Error loading save: {fieldData.name} not found!");
                            continue;
                        }
                        try { field.SetValue(comp, JsonUtility.FromJson(fieldData.data, field.FieldType)); }
                        catch (ArgumentException e)
                        {
                            if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                            {
                                ScriptableObject scriptableObject = ScriptableObject.CreateInstance(field.FieldType);
                                JsonUtility.FromJsonOverwrite(fieldData.data, scriptableObject);
                                field.SetValue(comp, scriptableObject);
                            }
                            else if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
                            {
                                MonoBehaviour monoBehaviour = field.GetValue(comp) as MonoBehaviour;
                                JsonUtility.FromJsonOverwrite(fieldData.data, monoBehaviour);
                                field.SetValue(comp, monoBehaviour);
                            }
                            else Debug.LogError(e); // Type not supported
                        }
                        catch (Exception e) { Debug.LogError(e); }
                    }
                }
                loadedGroup++;
            }
            return loadedGroup == fileName_fieldsData.Count;
        }
        private static void GroupFieldsWithFileName(Type attributeType, out Dictionary<string, List<FieldData>> fileName_fieldsData, bool createInstance = false)
        {
            List<FieldData> saveFields = new();
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).ToList().ForEach(mono => saveFields.AddRange(FindAttributesFields(mono, attributeType)));
            //
            fileName_fieldsData = new();
            foreach (FieldData fieldData in saveFields)
            {
                if (!fileName_fieldsData.ContainsKey(fieldData.path)) fileName_fieldsData.Add(fieldData.path, new List<FieldData>());
                fileName_fieldsData[fieldData.path].Add(fieldData);
                //
                if (createInstance)
                {
                    MonoBehaviour mono = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).ToList().Find(mono => mono.name == fieldData.mono);
                    Component comp = mono.GetComponent(fieldData.comp);
                    FieldInfo field = comp.GetType().GetField(fieldData.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //
                    try { JsonUtility.FromJson(fieldData.data, field.FieldType); }
                    catch (ArgumentException e)
                    {
                        if (field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                        {
                            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(field.FieldType);
                            JsonUtility.FromJsonOverwrite(fieldData.data, scriptableObject);
                            field.SetValue(comp, scriptableObject);
                        }
                        else Debug.LogError(e); // Type not supported
                    }
                    // catch (NullReferenceException e) { }
                    catch (Exception e) { Debug.LogError(fieldData.name + " => " + e); }
                }
            }
        }
        // File Handler Helpers
        public static List<FieldData> FindAttributesFields(MonoBehaviour mono, Type attributeType)
        {
            List<FieldData> matchesFields = new();
            foreach (FieldInfo field in mono.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                object[] attributes = field.GetCustomAttributes(typeof(HeySaveAttribute), false);
                foreach (object attribute in attributes)
                {
                    if (attribute.GetType() == attributeType)
                    {
                        //Debug.Log($"Registered Field: {target.name}.{field.Name}, Path: {heySaveAttribute.path}");
                        matchesFields.Add(new FieldData() { path = attribute.ToString(), mono = mono.name, comp = mono.GetType().FullName, name = field.Name, data = JsonUtility.ToJson(field.GetValue(mono)) });
                    }
                }
            }
            return matchesFields;
        }
        public static void OpenFolder() => Application.OpenURL(path);
        public static void DeleteFiles(string extension) => Directory.GetFiles(path, $"*{extension}").ToList().ForEach(file => File.Delete(file));
    }
}
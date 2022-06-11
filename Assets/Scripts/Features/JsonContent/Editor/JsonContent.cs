using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zekzek.JsonContent
{
    public class JsonContent : EditorWindow
    {
        private MonoScript model;
        private object instance;
        private string json;
        private string filename;

        [MenuItem("Window/Json Content")]
        public static void ShowWindow()
        {
            GetWindow(typeof(JsonContent));
        }

        private void OnGUI()
        {
            // Get MonoScript Model
            model = (MonoScript)EditorGUILayout.ObjectField("Model", model, typeof(MonoScript), false);
            if (model == null) { DrawWarning("No model found"); return; }

            // Generate an instance of the Model if its new
            if (instance == null || model.GetClass() != instance.GetType()) {
                instance = Activator.CreateInstance(model.GetClass());
                ReloadFromInstance();
            }

            // Draw and save current JSON
            json = GUILayout.TextArea(json);

            // Attempt to convert JSON back to the instance
            try {
                object updatedInstance = JsonUtility.FromJson(json, instance.GetType());
                instance = updatedInstance;
                ReloadFromInstance();
            } catch {
                GUILayout.BeginHorizontal();
                DrawWarning("Invalid JSON");
                if (GUILayout.Button("Reload")) { ReloadFromInstance(); }
                GUILayout.EndHorizontal();
            }

            // Draw File Management
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filename");
            filename = GUILayout.TextField(filename);
            if (GUILayout.Button("Save")) { Save(instance.GetType().Name); }
            if (GUILayout.Button("Load")) { Load(instance.GetType().Name); }
            GUILayout.EndHorizontal();
        }

        private void DrawWarning(string text)
        {
            GUILayout.Label(text);
        }

        private void ReloadFromInstance()
        {
            json = JsonUtility.ToJson(instance, true);
        }

        private void Save(string type)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources"));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources", type));
            File.WriteAllText(Path.Combine(Application.dataPath, "Resources", type, filename + ".json"), json);
        }

        private void Load(string type)
        {
            TextAsset content = Resources.Load<TextAsset>(Path.Combine(type, filename));
            if (content != null && !string.IsNullOrEmpty(content.text)) {
                json = content.text;
            }
        }
    }
}
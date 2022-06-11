using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/** Load generic JSON data from within the 'Assets/Resources' folder and convert to a basic object */
namespace Zekzek.JsonContent
{
    public static class ContentUtil
    {
        private static Dictionary<string, Dictionary<string, object>> cachedData = new Dictionary<string, Dictionary<string, object>>();

        public static string GetPath<TYPE>()
        {
            return typeof(TYPE).Name;
        }

        public static TYPE LoadData<TYPE>(string id, bool reload = false)
        {
            string path = GetPath<TYPE>();
            if (path == null) { return default(TYPE); }

            if (!cachedData.ContainsKey(path)) {
                cachedData.Add(path, new Dictionary<string, object>());
            } else if (reload && cachedData[path].ContainsKey(id)) {
                cachedData[path].Remove(id);
            } else if (cachedData[path].ContainsKey(id)) {
                return (TYPE)cachedData[path][id];
            }

            string fullPath = Path.Combine(path, id);

            TextAsset json = Resources.Load<TextAsset>(fullPath);
            if (json != null && !string.IsNullOrEmpty(json.text)) {
                TYPE data = JsonUtility.FromJson<TYPE>(json.text);
                Resources.UnloadAsset(json);
                cachedData[path].Add(id, data);
                Debug.Log(string.Format("Loaded {0} from Resources: {1}", fullPath, data));
                return data;
            }

            Debug.LogError(string.Format("Unable to find content for {0}", fullPath));
            return default(TYPE);
        }

        public static void ClearCachedData()
        {
            cachedData.Clear();
        }

        // Note: This method will fail to find additional data if any has already been loaded
        private static List<TYPE> LoadAllData<TYPE>(bool reload = false)
        {
            string path = GetPath<TYPE>();
            if (path == null) { return new List<TYPE>(); }

            List<TYPE> datas = new List<TYPE>();

            // Check the cache
            if (cachedData.ContainsKey(path)) {
                if (reload) {
                    cachedData.Remove(path);
                } else {
                    var values = cachedData[path].Values.GetEnumerator();
                    while (values.MoveNext()) { datas.Add((TYPE)values.Current); }
                    return datas;
                }
            }

            // Load the files
            cachedData.Add(path, new Dictionary<string, object>());
            TextAsset[] jsons = Resources.LoadAll<TextAsset>(path);
            foreach (TextAsset json in jsons)
                if (json != null && !String.IsNullOrEmpty(json.text)) {
                    Debug.Log(string.Format("Loading {0} from Resources", path));
                    TYPE data = JsonUtility.FromJson<TYPE>(json.text);
                    Resources.UnloadAsset(json);
                    cachedData[path].Add(json.name, data);
                    datas.Add(data);
                }

            return datas;
        }
    }
}
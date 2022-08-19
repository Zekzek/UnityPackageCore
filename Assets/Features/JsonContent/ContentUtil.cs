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

        public static TYPE LoadData<TYPE>(string id, bool reload = false)
        {
            string type = typeof(TYPE).Name;

            if (!cachedData.ContainsKey(type)) {
                cachedData.Add(type, new Dictionary<string, object>());
            } else if (reload && cachedData[type].ContainsKey(id)) {
                cachedData[type].Remove(id);
            } else if (cachedData[type].ContainsKey(id)) {
                return (TYPE)cachedData[type][id];
            }

            string fullPath = Path.Combine(type, id);

            TextAsset json = Resources.Load<TextAsset>(fullPath);
            if (json != null && !string.IsNullOrEmpty(json.text)) {
                TYPE data = JsonUtility.FromJson<TYPE>(json.text);
                Resources.UnloadAsset(json);
                cachedData[type].Add(id, data);
                Debug.Log(string.Format("Loaded {0} from Resources: {1}", fullPath, JsonUtility.ToJson(data, true)));
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
            string type = typeof(TYPE).Name;
            if (type == null) { return new List<TYPE>(); }

            List<TYPE> datas = new List<TYPE>();

            // Check the cache
            if (cachedData.ContainsKey(type)) {
                if (reload) {
                    cachedData.Remove(type);
                } else {
                    var values = cachedData[type].Values.GetEnumerator();
                    while (values.MoveNext()) { datas.Add((TYPE)values.Current); }
                    return datas;
                }
            }

            // Load the files
            cachedData.Add(type, new Dictionary<string, object>());
            TextAsset[] jsons = Resources.LoadAll<TextAsset>(type);
            foreach (TextAsset json in jsons)
                if (json != null && !String.IsNullOrEmpty(json.text)) {
                    Debug.Log(string.Format("Loading {0} from Resources", type));
                    TYPE data = JsonUtility.FromJson<TYPE>(json.text);
                    Resources.UnloadAsset(json);
                    cachedData[type].Add(json.name, data);
                    datas.Add(data);
                }

            return datas;
        }
    }
}
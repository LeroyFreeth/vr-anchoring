using System.IO;
using UnityEngine;

namespace Json
{
    public class JsonFileHelper<T> where T : new()
    {
        private readonly string path;

        public JsonFileHelper(string path, bool useApplicationPath = true)
        {
            this.path = useApplicationPath ? ConvertToApplicationPath(path) : path;
        }

        public void Save(T data)
        {
            if (data == null) return;
            if (!File.Exists(path))
                CreateFile();
            
            string dataString = data.ToString();
#if UNITY_EDITOR
            //Debug.Log($"<color=green>Writing</color> to data file data for path [{path}]:\n{dataString}");
#endif
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        public void CreateFile()
        {
            if (File.Exists(path))
            {
#if UNITY_EDITOR
                Debug.Log($"<color=red>Writing</color> File already created for path [{path}]");
#endif
                return;
            }

            T newData = new T();
            string dataString = newData.ToString();
#if UNITY_EDITOR
            Debug.Log($"<color=orange>Creating</color> new data file data for path [{path}]:\n{dataString}");
#endif
            string json = JsonUtility.ToJson(newData, true);
            File.WriteAllText(path, json);
        }

        public bool Load(out T data)
        {
            if (!File.Exists(path))
            {
                data = new T();
                return false;
            }

            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<T>(json);
            string dataString = data.ToString();
#if UNITY_EDITOR
            Debug.Log($"<color=green>Loaded</color> from data file from path [{path}]:\n{dataString}");
#endif
            return true;
        }

        public void DeleteSaveDataFile()
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"No file exists for path [{path}]");
                return;
            }
#if UNITY_EDITOR
            Debug.Log($"Deleting file for path [{path}]");
#endif
            File.Delete(path);
        }

        public void ResetSaveData() => Save(new T());

        /// <summary>
        /// Does not require extension, as only jsons are allowed
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string ConvertToApplicationPath(string filePath) =>
            Path.Combine(Application.dataPath, $"{filePath}.json");
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [Serializable]
    public class GameData
    {
        public Vector2 playerPosition;

        public List<InventoryItem> playerInventory = new List<InventoryItem>();

        public List<string> intKeys = new List<string>();
        public List<int> intValues = new List<int>();

        public List<string> floatKeys = new List<string>();
        public List<float> floatValues = new List<float>();

        public List<string> boolKeys = new List<string>();
        public List<bool> boolValues = new List<bool>();
        
        public List<string> stringKeys = new List<string>();
        public List<string> stringValues = new List<string>();
    }

    public class PersistentDataManager : MonoBehaviour
    {
        static PersistentDataManager m_Instance;

        GameData m_GameData = new GameData();

        public static PersistentDataManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = FindObjectOfType<PersistentDataManager>();

                    if (m_Instance == null)
                    {
                        var go = new GameObject("PersistentDataManager");
                        m_Instance = go.AddComponent<PersistentDataManager>();
                    }
                }

                return m_Instance;
            }
        }

        public bool Load()
        {
            if (File.Exists(Application.persistentDataPath + "save.json"))
            {
                var json = File.ReadAllText(Application.persistentDataPath + "save.json");
                JsonUtility.FromJsonOverwrite(json, m_GameData);

                SceneManager.Instance.Character.WarpToPosition(m_GameData.playerPosition);
                //TODO Set player inventory

                return true;
            }

            return false;
        }

        public void Save()
        {
            var playerPosition = SceneManager.Instance.Character.transform.position;
            m_GameData.playerPosition = new Vector2(playerPosition.x, playerPosition.y);
            //TODO get items from inventory
            var json = JsonUtility.ToJson(m_GameData);
            File.WriteAllText(Application.persistentDataPath + "save.json", json);
        }

        public int GetValue(string key, int defaultValue = 0)
        {
            int index = m_GameData.intKeys.IndexOf(key);
            if (index > 0)
            {
                return m_GameData.intValues[index];
            }

            return defaultValue;
        }

        public void SetValue(string key, int value)
        {
            int index = m_GameData.intKeys.IndexOf(key);
            if (index > 0)
            {
                m_GameData.intValues[index] = value;
            }
            else
            {
                m_GameData.intKeys.Add(key);
                m_GameData.intValues.Add(value);
            }
        }

        public float GetValue(string key, float defaultValue = 0f)
        {
            int index = m_GameData.floatKeys.IndexOf(key);
            if (index > 0)
            {
                return m_GameData.floatValues[index];
            }

            return defaultValue;
        }

        public void SetValue(string key, float value)
        {
            int index = m_GameData.floatKeys.IndexOf(key);
            if (index > 0)
            {
                m_GameData.floatValues[index] = value;
            }
            else
            {
                m_GameData.floatKeys.Add(key);
                m_GameData.floatValues.Add(value);
            }
        }

        public bool GetValue(string key, bool defaultValue = false)
        {
            int index = m_GameData.boolKeys.IndexOf(key);
            if (index > 0)
            {
                return m_GameData.boolValues[index];
            }

            return defaultValue;
        }
        public void SetValue(string key, bool value)
        {
            int index = m_GameData.boolKeys.IndexOf(key);
            if (index > 0)
            {
                m_GameData.boolValues[index] = value;
            }
            else
            {
                m_GameData.boolKeys.Add(key);
                m_GameData.boolValues.Add(value);
            }
        }

        public string GetValue(string key, string defaultValue = "")
        {
            int index = m_GameData.stringKeys.IndexOf(key);
            if (index > 0)
            {
                return m_GameData.stringValues[index];
            }

            return defaultValue;
        }

        public void SetValue(string key, string value)
        {
            int index = m_GameData.stringKeys.IndexOf(key);
            if (index > 0)
            {
                m_GameData.stringValues[index] = value;
            }
            else
            {
                m_GameData.stringKeys.Add(key);
                m_GameData.stringValues.Add(value);
            }
        }

#if UNITY_EDITOR
        [MenuItem("Adventure Game/Delete Saved Data", false, 1050)]
        static void DeleteSavedData()
        {
            File.Delete(Application.persistentDataPath + "save.json");
        }
#endif
    }
}

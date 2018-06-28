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

        public string[] playerInventory = new string[InventoryManager.INVENTORY_SLOTS];

        public HashSet<string> finishedStoryEvents = new HashSet<string>();
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
                InventoryManager.Instance.ReloadInventoryWithIDs(m_GameData.playerInventory);

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

        public bool IsStoryEventFinished(string storyEvent)
        {
            return m_GameData.finishedStoryEvents.Contains(storyEvent);
        }

        public void AddFinishedStoryEvent(string storyEvent)
        {
            m_GameData.finishedStoryEvents.Add(storyEvent);
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

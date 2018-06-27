using System.Collections.Generic;
using System.IO;
using UnityEditor.AdventureGame;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AdventureGame
{
    public class SceneManager : MonoBehaviour
    {
        [SerializeField]
        Character m_Character;

        public Character Character
        {
            get { return m_Character; }
            set { m_Character = value; }
        }

        static SceneManager s_instance;
        public static SceneManager Instance
        {
            get
            {
#if UNITY_EDITOR
                // deal with script reloading and find scene manager again
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<SceneManager>();
                }
 #endif
                return s_instance;
            }
        }

        [SerializeField]
        Transform m_transitionTransform;

        [SerializeField]
        float m_transitionTime = 3.0f;
        bool m_isTransitioning = false;
        bool m_isGrowing = true;

        // Use this for initialization
        void Start()
        {
            s_instance = this;
#if UNITY_EDITOR
            ReloadScenePrefabs();
#endif
            if (PersistentDataManager.Instance.Load())
            {
                m_Character.WarpToPosition(PersistentDataManager.Instance.GetSavedPosition());
            }

            if (!m_transitionTransform)
                Debug.LogWarning("No star wipe object specified.");
            else {
                m_transitionTransform.gameObject.SetActive(false);
                StartTransition();
            }
        }

        void OnApplicationQuit()
        {
            var playerPosition = m_Character.transform.position;
            PersistentDataManager.Instance.Save(new Vector2(playerPosition.x, playerPosition.y), new List<InventoryItem>());
        }

#if UNITY_EDITOR
        public string m_outputPath = "Assets/ScenePrefabs";
        public int    m_defaultWidth  = 1024;
        public int    m_defaultHeight = 768;

        // replace all children with the associated prefabs
        public void ReloadScenePrefabs()
        {
            List<GameObject> deleteGameObjects = new List<GameObject>();
            List<Object> prefabObjects = new List<Object>();
            foreach (Transform child in transform)
            {
                // create a new prefab
                string scenePrefabName = string.Format("{0}/{1}/{1}.prefab", m_outputPath, child.name);
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(scenePrefabName);
                if (prefab != null)
                {
                    deleteGameObjects.Add(child.gameObject);
                    prefabObjects.Add(prefab);
                }
                else
                {
                    Debug.LogWarningFormat("Could not find sceneprefab at {0} to replace child gameobject!", scenePrefabName);
                }
            }

            for (int i = 0; i < deleteGameObjects.Count; ++i)
            {
                GameObject go = (GameObject)Instantiate(prefabObjects[i], transform);
                go.name = deleteGameObjects[i].name;

                // Relink the prefab
                PrefabUtility.ConnectGameObjectToPrefab(go, (GameObject)prefabObjects[i]);

                deleteGameObjects[i].name = deleteGameObjects[i].name + "_old";

                //deleteGameObjects[i].transform.parent = null;
                DestroyImmediate(deleteGameObjects[i]);
            }
        }
#endif
        void StartTransition() {
            // Make transition object visible
            m_transitionTransform.gameObject.SetActive(true);

            // Begin growing transition object to cover entire screen
            m_isTransitioning = true;
            m_isGrowing = true;
        }

        void Update() {
            if (m_isTransitioning) {
                float transitionIncrement = 1000.0f / (m_transitionTime/2) * Time.fixedDeltaTime;
                if (m_isGrowing) {
                    m_transitionTransform.localScale += new Vector3(transitionIncrement, transitionIncrement, transitionIncrement);
                    // Shrink transition
                    if (m_transitionTransform.localScale.x >= 1000.0f)
                        m_isGrowing = false;
                }
                else {
                    m_transitionTransform.localScale -= new Vector3(transitionIncrement, transitionIncrement, transitionIncrement);
                    // Stop transition
                    if (m_transitionTransform.localScale.x <= 1.0f) {
                        m_isTransitioning = false;
                        m_transitionTransform.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

}
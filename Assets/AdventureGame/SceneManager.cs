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

        string m_scenePrefabCurrent = "Scene";
        string m_scenePrefabToLoad = "Scene";

        Vector2 m_sceneStartingPosition;

        // Use this for initialization
        void Start()
        {
            s_instance = this;
#if UNITY_EDITOR
            ReloadScenePrefabs();
#endif
            PersistentDataManager.Instance.Load();
        }

        public Transform GetLocator(string name)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Scene scene = child.gameObject.GetComponent<Scene>();
                    for (int i = 0; i < scene.m_LocatorRoot.transform.childCount; ++i)
                    {
                        Transform locator = scene.m_LocatorRoot.transform.GetChild(i);
                        if (locator.name == name)
                        {
                            return locator.transform;
                        }
                    }
                }
            }

            return null;
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

                DestroyImmediate(deleteGameObjects[i]);
            }
        }
#endif

        void UnloadScenePrefab() {
            Debug.Log("Unloading : " + m_scenePrefabCurrent);
            Destroy(transform.GetChild(0).gameObject);
            Debug.Log("Unloaded : " + m_scenePrefabCurrent);
        }

        void LoadScenePrefab() {
            m_scenePrefabCurrent = m_scenePrefabToLoad;
            Debug.Log("Loading : " + m_scenePrefabCurrent);
            string scenePrefabName = string.Format("{0}/{1}/{1}.prefab", m_outputPath, m_scenePrefabCurrent);
            Object prefab = AssetDatabase.LoadAssetAtPath<Object>(scenePrefabName);

            GameObject go = (GameObject)Instantiate(prefab, transform);
            go.name = prefab.name;

            // Relink the prefab
            PrefabUtility.ConnectGameObjectToPrefab(go, (GameObject)prefab);

            // Grab starting position from scene prefab
            GameObject startPosObj = GameObject.FindGameObjectWithTag("StartPosition");
            if (startPosObj) {
                m_sceneStartingPosition = new Vector2(startPosObj.transform.localPosition.x, startPosObj.transform.localPosition.y);
                Debug.Log("Moving character to " + m_sceneStartingPosition + ".");
            }
            else {
                Debug.LogError("No start position found for scene " + m_scenePrefabCurrent + ", placing character at (0,0).");
                m_sceneStartingPosition = new Vector2(0,0);
            }
            m_Character.WarpToPosition(m_sceneStartingPosition);
            Debug.Log("Loaded : " + m_scenePrefabToLoad);

            GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
            camera.GetComponent<RegionWindowFollowingCamera>().WarpToTargetPosition();
        }

        void ResetTransition() {
            m_transitionTransform.localScale = new Vector3(1f, 1f, 1f);
            m_transitionTransform.gameObject.SetActive(false);
        }

        void StartTransition() {
            if (!m_transitionTransform) {
                Debug.LogWarning("No transition object set. Will just immediately unload " + m_scenePrefabCurrent + " and load " + m_scenePrefabToLoad);
                UnloadScenePrefab();
                LoadScenePrefab();
                return;
            }
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
                    if (m_transitionTransform.localScale.x >= 1000.0f) {
                        // Transition has covered whole screen; unload current scene and load new scene
                        m_isGrowing = false;
                        UnloadScenePrefab();
                        LoadScenePrefab();
                    }
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


        // Sent by trigger area in scene asking scene manager to load a new scene
        public void TriggerDoorway(string sceneName) {
            Debug.Log("Trigger Scene " + sceneName);
            StartTransition();
        }
    }

}
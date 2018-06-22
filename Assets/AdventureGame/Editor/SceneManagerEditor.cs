using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    public class SceneManagerMenuItems
    {
        [MenuItem("Adventure Game/Reload Scene Prefabs &r")]
        static void MenuItemReloadScenePrefabs()
        {
            SceneManager sceneManager = (SceneManager)Object.FindObjectOfType(typeof(SceneManager));
            if (sceneManager == null)
            {
                Debug.LogWarning("Could not find SceneManager in the scene! Make sure to add a SceneManager component to a game component in your scene.");
            }
            else
            {
                sceneManager.ReloadScenePrefabs();
            }
        }

        [MenuItem("Adventure Game/Save Scene Prefabs &s")]
        static void MenuItemSaveScenePrefabs()
        {
            SceneManager sceneManager = (SceneManager)Object.FindObjectOfType(typeof(SceneManager));
            if (sceneManager == null)
            {
                Debug.LogWarning("Could not find SceneManager in the scene! Make sure to add a SceneManager component to a game component in your scene.");
            }
            else
            {
                SceneManagerEditor.SaveAllScenePrefabs(sceneManager);
            }
        }
    }
    
    // auto-saves the scene prefabs when the scene is saved
    public class SceneManagerAssetPostProcessor : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (path.EndsWith(".unity"))
                {
                    AssetDatabase.LoadAssetAtPath<Object>(path);
                    SceneManager manager = Object.FindObjectOfType<SceneManager>();
                    SceneManagerEditor.SaveAllScenePrefabs(manager);
                }
            }

            return paths;
        }
    }

    [CustomEditor(typeof(SceneManager))]
    public class SceneManagerEditor : Editor
    {
        SceneManager       m_SceneManager;
        SerializedProperty m_OutputPath;
        SerializedProperty m_DefaultWidth;
        SerializedProperty m_DefaultHeight;

        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            // auto-saves the scene prefabs when run is pressed
            EditorApplication.playModeStateChanged += state =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    SceneManager manager = FindObjectOfType<SceneManager>();
                    if (manager != null)
                    {
                        SaveAllScenePrefabs(manager);
                    }
                }
            };

            // Ensures that the project files are named according to the GameObject scene names
            // whenever a game object rename occurs
            EditorApplication.hierarchyChanged += () =>
            {
                SceneManager manager = FindObjectOfType<SceneManager>();
                if (manager != null)
                {
                    EnsureProjectConsistency(manager);
                }
            };

            // handle hiding other scenes when the selection has changed
            Selection.selectionChanged += () =>
            {
                SceneManager manager = FindObjectOfType<SceneManager>();
                if (manager != null && Selection.activeGameObject != null)
                {
                    SetSelectedScenePrefab(manager);
                }
            };
        }

        void OnEnable()
        {
            m_SceneManager = (SceneManager)target;

            m_OutputPath = serializedObject.FindProperty("m_outputPath");
            m_DefaultWidth = serializedObject.FindProperty("m_defaultWidth");
            m_DefaultHeight = serializedObject.FindProperty("m_defaultHeight");
        }

        public override void OnInspectorGUI()
        {
            Color oldColor = GUI.color;

            GUILayout.Space(15);

            EditorGUILayout.PropertyField(m_OutputPath, new GUIContent("Output Path"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Default Resolution", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(m_DefaultWidth, GUIContent.none, GUILayout.MaxWidth(75.0f));
            EditorGUILayout.LabelField(" x ", GUILayout.Width(20.0f));
            EditorGUILayout.PropertyField(m_DefaultHeight, GUIContent.none, GUILayout.MaxWidth(75.0f));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            GUI.color = Color.green;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Add New Scene", GUILayout.Height(50)))
            {
                AddNewScene();
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.color = Color.cyan;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Save Scene Prefabs", GUILayout.Height(50)))
            {
                SaveAllScenePrefabs(m_SceneManager);
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.color = Color.red;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Reload Scene Prefabs", GUILayout.Height(50)))
            {
                m_SceneManager.ReloadScenePrefabs();
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();

            GUI.color = oldColor;

            GUILayout.Space(15);

            serializedObject.ApplyModifiedProperties();
        }

        void AddNewScene()
        {
            float aspectRatio = (float)m_SceneManager.m_defaultWidth / m_SceneManager.m_defaultHeight;

            GameObject sceneRoot = new GameObject();
            sceneRoot.transform.SetParent(m_SceneManager.transform, false);
            sceneRoot.name = GameObjectUtility.GetUniqueNameForSibling(m_SceneManager.transform, "Scene");

            GameObject backgroundObject = new GameObject();
            backgroundObject.transform.SetParent(sceneRoot.transform, false);
            backgroundObject.name = "BackgroundImage";
            SpriteRenderer backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
            backgroundRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            float backgroundHeight = 2.0f * Camera.main.orthographicSize;
            backgroundRenderer.drawMode = SpriteDrawMode.Sliced;
            backgroundRenderer.color = Color.gray;
            backgroundRenderer.size = new Vector2(backgroundHeight * aspectRatio, backgroundHeight);

            GameObject walkableAreaGroup = new GameObject();
            walkableAreaGroup.transform.SetParent(sceneRoot.transform, false);
            walkableAreaGroup.transform.Rotate(-90.0f, 0.0f, -10.0f);

            float walkableScaleHeight = Camera.main.orthographicSize / WalkableAreaEditor.k_SpriteMeshSize;
            walkableAreaGroup.transform.localScale = new Vector3(walkableScaleHeight * aspectRatio, 1.0f, walkableScaleHeight);
            walkableAreaGroup.name = "WalkableAreas";
            walkableAreaGroup.AddComponent<WalkableAreaGroup>();

            SaveScenePrefab(m_SceneManager, sceneRoot.transform);

            Selection.activeGameObject = sceneRoot;
            EditorGUIUtility.PingObject(sceneRoot);
        }

        public static void SaveAllScenePrefabs(SceneManager manager)
        {
            foreach (Transform child in manager.transform)
            {
                PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(child);

                GameObject prefabObj = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

                // look for any modification to the prefab
                List<PropertyModification> listModifications = new List<PropertyModification>();
                if (modifications != null && prefabObj != null)
                {
                    for (int i = 0; i < modifications.Length; ++i)
                    {
                        if (modifications[i].target as Transform != prefabObj.transform ||
                            (!modifications[i].propertyPath.StartsWith("m_LocalPosition.") &&
                             !modifications[i].propertyPath.StartsWith("m_LocalRotation.") &&
                             modifications[i].propertyPath != "m_RootOrder"))
                        {
                            listModifications.Add(modifications[i]);
                        }
                    }
                }

                if (modifications == null ||
                    prefabObj == null ||
                    listModifications.Count > 0 ||
                    !SceneHierarchyEqual(prefabObj, child.gameObject))
                {
                    SaveScenePrefab(manager, child);
                }
            }
        }

        static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').ToUpperInvariant();
        }

        static string GetScenePrefabPath(SceneManager manager, Transform child)
        {
            return string.Format("{0}/{1}/{1}.prefab", manager.m_outputPath, child.name);
        }

        static void SaveScenePrefab(SceneManager manager, Transform child)
        {
            EditorUtility.DisplayProgressBar("Saving Scene Prefab",
                string.Format("Applying Changes to Scene {0}", child.name), 0.5f);
            GameObject prefabObj = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            // check for prefab object rename and rename the source directory to try and organize assets for the user
            if (prefabObj != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefabObj);
                GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, child.gameObject);
                PrefabUtility.ReplacePrefab(child.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                Debug.LogFormat("Saved Scene Prefab: {0}", prefabPath);
            }
            else
            {
                string targetPath = GetScenePrefabPath(manager, child);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                GameObject prefab = PrefabUtility.CreatePrefab(targetPath, child.gameObject);
                PrefabUtility.ReplacePrefab(child.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                Debug.LogFormat("Saved Scene Prefab: {0}", targetPath);
            }
            EditorUtility.ClearProgressBar();
        }

        static void EnsureProjectConsistency(SceneManager manager)
        {
            foreach (Transform child in manager.transform)
            {
                // if the name changed or it is not a prefab yet then save it
                GameObject prefabObj = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefabObj == null)
                {
                    SaveScenePrefab(manager, child);
                }
                else if (prefabObj.name != child.name)
                {
                    string targetPath = GetScenePrefabPath(manager, child);
                    string prefabPath = AssetDatabase.GetAssetPath(prefabObj);

                    // set the filename if it is not the same
                    string prefabFilename = Path.GetFileName(prefabPath);
                    string targetFilename = Path.GetFileName(targetPath);
                    string prefabDirectory = Path.GetDirectoryName(prefabPath);
                    string targetDirectory = Path.GetDirectoryName(targetPath);
                    if (prefabFilename != targetFilename)
                    {
                        EditorUtility.DisplayProgressBar("Renaming Scene Prefab",
                            string.Format("Renaming Scene prefab from {0} to {1}", prefabFilename, targetFilename), 0.5f);
                        string targetRenamePath = Path.Combine(prefabDirectory, targetFilename);
                        if (File.Exists(targetRenamePath))
                        {
                            targetRenamePath = AssetDatabase.GenerateUniqueAssetPath(targetRenamePath);
                        }
                        AssetDatabase.MoveAsset(prefabPath, targetRenamePath);
                        Debug.LogFormat("Renamed Scene prefab from {0} to {1}!", prefabFilename, targetFilename);
                        AssetDatabase.Refresh();
                        EditorUtility.ClearProgressBar();
                    }

                    // set the directory if it is not the same
                    if (NormalizePath(prefabPath) != NormalizePath(targetPath))
                    {
                        EditorUtility.DisplayProgressBar("Moving Scene Path",
                            string.Format("Moving Scene prefab directory from {0} to {1}", prefabDirectory, targetDirectory), 0.5f);
                        if (Directory.Exists(targetDirectory))
                        {
                            targetDirectory = AssetDatabase.GenerateUniqueAssetPath(targetDirectory);
                        }
                        AssetDatabase.MoveAsset(prefabDirectory, targetDirectory);
                        Debug.LogFormat("Moved Scene prefab from {0} to {1}!", prefabDirectory, targetDirectory);
                        AssetDatabase.Refresh();
                        EditorUtility.ClearProgressBar();
                    }
                }

                WalkableArea[] walkableAreas = child.transform.GetComponentsInChildren<WalkableArea>();
                for (int i = 0; i < walkableAreas.Length; ++i)
                {
                    WalkableAreaEditor.EnsureProjectConsistency(manager, walkableAreas[i]);
                }
            }
        }

        static void SetSelectedScenePrefab(SceneManager manager)
        {
            GameObject selectedScenePrefab = null;
            foreach (Transform child in manager.transform)
            {
                GameObject prefabRoot = PrefabUtility.FindPrefabRoot(Selection.activeGameObject);
                if (prefabRoot.GetInstanceID() == child.gameObject.GetInstanceID())
                {
                    selectedScenePrefab = prefabRoot;
                    break;
                }
            }

            if (selectedScenePrefab != null)
            {
                foreach (Transform child in manager.transform)
                {
                    child.gameObject.SetActive(selectedScenePrefab.GetInstanceID() == child.gameObject.GetInstanceID());
                }
            }
        }

        static bool SceneHierarchyEqual(GameObject go1, GameObject go2)
        {
            if (go1.name != go2.name ||
                go1.transform.childCount != go2.transform.childCount)
            {
                return false;
            }

            Component[] comps1 = go1.GetComponents<Component>();
            Component[] comps2 = go2.GetComponents<Component>();
            if (comps1.Length != comps2.Length)
            {
                return false;
            }

            for (int i = 0; i < comps1.Length; ++i)
            {
                if (comps1[i].GetType() != comps2[i].GetType())
                {
                    return false;
                }
            }

            for (int i = 0; i < go1.transform.childCount; ++i)
            {
                if (!SceneHierarchyEqual(go1.transform.GetChild(i).gameObject, go2.transform.GetChild(i).gameObject))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
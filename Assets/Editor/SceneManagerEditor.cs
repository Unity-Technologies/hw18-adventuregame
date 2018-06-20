using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
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
                    SceneManagerEditor.SaveScenePrefabs(manager);
                }
            }

            return paths;
        }
    }

    [InitializeOnLoad]
    public class AutosaveOnRun : ScriptableObject
    {
        static AutosaveOnRun()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    SceneManager manager = Object.FindObjectOfType<SceneManager>();
                    SceneManagerEditor.SaveScenePrefabs(manager);
                }
            };
        }
    }

    [CustomEditor(typeof(SceneManager))]
    public class SceneManagerEditor : Editor
    {
        SceneManager       m_SceneManager;
        SerializedProperty m_OutputPath;
        SerializedProperty m_DefaultWidth;
        SerializedProperty m_DefaultHeight;

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
                SaveScenePrefabs(m_SceneManager);
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
            GameObject sceneRoot = new GameObject();
            sceneRoot.transform.SetParent(m_SceneManager.transform, false);
            sceneRoot.name = GameObjectUtility.GetUniqueNameForSibling(m_SceneManager.transform, "Scene");

            GameObject walkableAreaGroup = new GameObject();
            walkableAreaGroup.transform.SetParent(sceneRoot.transform, false);
            walkableAreaGroup.transform.Rotate(-90.0f, 0.0f, 0.0f);

            float aspectRatio = (float)m_SceneManager.m_defaultWidth / m_SceneManager.m_defaultHeight;
            float walkableWidth = Camera.main.orthographicSize / WalkableAreaEditor.k_SpriteMeshSize;
            walkableAreaGroup.transform.localScale = new Vector3(walkableWidth * aspectRatio, 1.0f, walkableWidth);
            walkableAreaGroup.name = "WalkableAreas";
            walkableAreaGroup.AddComponent<WalkableAreaGroup>();

            SaveScenePrefab(m_SceneManager, sceneRoot.transform);

            Selection.activeGameObject = sceneRoot;
            EditorGUIUtility.PingObject(sceneRoot);
        }

        public static void SaveScenePrefabs(SceneManager manager)
        {
            foreach (Transform child in manager.transform)
            {
                PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(child);

                GameObject prefabObj = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

                List<PropertyModification> listModifications = new List<PropertyModification>();
                if (modifications != null)
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

                if (modifications == null || listModifications.Count > 0 || !SceneHierarchyEqual(prefabObj, child.gameObject))
                {
                    SaveScenePrefab(manager, child);
                }
            }
        }

        static void SaveScenePrefab(SceneManager manager, Transform child)
        {
            string prefabDirectory = string.Format("{0}/{1}", manager.m_outputPath, child.name);
            string prefabPath = string.Format("{0}/{1}.prefab", prefabDirectory, child.name);
            Debug.LogFormat("Saving Scene Prefab: {0}", prefabPath);

            Directory.CreateDirectory(prefabDirectory);
            GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, child.gameObject);
            PrefabUtility.ReplacePrefab(child.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
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
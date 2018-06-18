using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneManager : MonoBehaviour
{
    public string m_outputPath = "Assets/ScenePrefabs";

	// Use this for initialization
	void Start ()
	{
#if UNITY_EDITOR
	    ReloadScenePrefabs();
#endif
	}

#if UNITY_EDITOR
    public void SaveScenePrefabs()
    {
        foreach (Transform child in transform)
        {
            string prefabPath = string.Format("{0}/{1}.prefab", m_outputPath, child.name);
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
                Debug.LogFormat("Saving Scene Prefab: {0}", prefabPath);
                GameObject prefab = PrefabUtility.CreatePrefab(prefabPath, child.gameObject);
                PrefabUtility.ReplacePrefab(child.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
        }
    }

    bool SceneHierarchyEqual(GameObject go1, GameObject go2)
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

    // replace all children with the associated prefabs
    public void ReloadScenePrefabs()
    {
        List<GameObject> deleteGameObjects = new List<GameObject>();
        List<Object> prefabObjects = new List<Object>();
        foreach (Transform child in transform)
        {
            // create a new prefab
            string scenePrefabName = string.Format("{0}/{1}.prefab", m_outputPath, child.name);
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
}

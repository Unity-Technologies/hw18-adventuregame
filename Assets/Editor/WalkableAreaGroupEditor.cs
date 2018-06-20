using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(WalkableAreaGroup))]
    public class WalkableAreaGroupEditor : Editor
    {
        WalkableAreaGroup m_WalkableAreaGroup;

        void OnEnable()
        {
            m_WalkableAreaGroup = (WalkableAreaGroup)target;
        }

        public override void OnInspectorGUI()
        {
            Color oldColor = GUI.color;

            GUILayout.Space(15);
            GUI.color = Color.green;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Add New Walkable Area", GUILayout.Height(50)))
            {
                GameObject gameObject = new GameObject();
                gameObject.transform.SetParent(m_WalkableAreaGroup.transform, false);
                gameObject.name = GameObjectUtility.GetUniqueNameForSibling(m_WalkableAreaGroup.transform, "WalkableArea");
                gameObject.AddComponent<WalkableArea>();
                Selection.activeGameObject = gameObject;
                EditorGUIUtility.PingObject(gameObject);
            }

            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        void OnSceneGUI()
        {
            Color oldColor = Handles.color;

            Handles.DrawAAPolyLine(3, 5, new[]
            {
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-WalkableAreaEditor.k_SpriteMeshSize, -0.1f, -WalkableAreaEditor.k_SpriteMeshSize)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(WalkableAreaEditor.k_SpriteMeshSize, -0.1f, -WalkableAreaEditor.k_SpriteMeshSize)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(WalkableAreaEditor.k_SpriteMeshSize, -0.1f, WalkableAreaEditor.k_SpriteMeshSize)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-WalkableAreaEditor.k_SpriteMeshSize, -0.1f, WalkableAreaEditor.k_SpriteMeshSize)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-WalkableAreaEditor.k_SpriteMeshSize, -0.1f, -WalkableAreaEditor.k_SpriteMeshSize))
            });
        }
    }
}

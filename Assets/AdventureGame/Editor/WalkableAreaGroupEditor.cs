using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(WalkableAreaGroup))]
    public class WalkableAreaGroupEditor : Editor
    {
        WalkableAreaGroup  m_WalkableAreaGroup;
        SerializedProperty m_TextureWidth;
        SerializedProperty m_TextureHeight;

        void OnEnable()
        {
            m_WalkableAreaGroup = (WalkableAreaGroup)target;

            m_TextureWidth = serializedObject.FindProperty("m_textureWidth");
            m_TextureHeight = serializedObject.FindProperty("m_textureHeight");
        }

        public override void OnInspectorGUI()
        {
            Color oldColor = GUI.color;

            GUILayout.Space(15);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Texture Resolution", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.PropertyField(m_TextureWidth, GUIContent.none, GUILayout.MaxWidth(75.0f));
            EditorGUILayout.LabelField(" x ", GUILayout.Width(20.0f));
            EditorGUILayout.PropertyField(m_TextureHeight, GUIContent.none, GUILayout.MaxWidth(75.0f));
            EditorGUILayout.EndHorizontal();

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

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            Color oldColor = Handles.color;

            Handles.DrawAAPolyLine(3, 5, new[]
            {
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-1.0f, -0.1f, -1.0f)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(1.0f, -0.1f, -1.0f)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(1.0f, -0.1f, 1.0f)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-1.0f, -0.1f, 1.0f)),
                m_WalkableAreaGroup.transform.TransformPoint(new Vector3(-1.0f, -0.1f, -1.0f))
            });
        }
    }
}

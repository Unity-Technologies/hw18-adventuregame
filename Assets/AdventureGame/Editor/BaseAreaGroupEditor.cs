using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    public abstract class BaseGroupEditor : Editor
    {
        SerializedProperty m_TextureWidth;
        SerializedProperty m_TextureHeight;

        public abstract string Name { get; }
        public abstract void CreateArea(GameObject parent);

        void OnEnable()
        {
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
            if (GUILayout.Button(string.Format("Add New {0}", Name), GUILayout.Height(50)))
            {
                MonoBehaviour behavior = target as MonoBehaviour;
                CreateArea(behavior.gameObject);
            }

            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            GameObject go = (target as GameObject);
            Handles.DrawAAPolyLine(3, 5, new[]
            {
                go.transform.TransformPoint(new Vector3(-1.0f, -1.0f, -0.1f)),
                go.transform.TransformPoint(new Vector3( 1.0f, -1.0f, -0.1f)),
                go.transform.TransformPoint(new Vector3( 1.0f,  1.0f, -0.1f)),
                go.transform.TransformPoint(new Vector3(-1.0f,  1.0f, -0.1f)),
                go.transform.TransformPoint(new Vector3(-1.0f, -1.0f, -0.1f))
            });
        }
    }
}

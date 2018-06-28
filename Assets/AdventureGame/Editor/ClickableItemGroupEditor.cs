using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(ClickableItemGroup))]
    public class ClickableItemGroupEditor : Editor
    {
        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            Color oldColor = GUI.color;

            GUI.color = Color.green;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Add New Clickable Item", GUILayout.Height(50)))
            {

            }

            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {

        }
    }
}

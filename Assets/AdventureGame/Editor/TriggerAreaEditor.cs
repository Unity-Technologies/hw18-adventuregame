using System.IO;
using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(TriggerArea))]
    public class TriggerAreaEditor : BaseAreaEditor
    {
        SerializedProperty m_GameLogicData;

        public override void OnEnable()
        {
            m_GameLogicData = serializedObject.FindProperty("m_gameLogicData");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            Color oldColor = GUI.color;
            GUI.color = Color.yellow;
            if (GUILayout.Button("Edit Game Logic", GUILayout.Height(50)))
            {
                if (m_GameLogicData.objectReferenceValue == null)
                {
                    GameObject root = PrefabUtility.FindValidUploadPrefabInstanceRoot(m_Behavior.transform.parent.gameObject);
                    string gameLogicPath = Path.Combine(Path.Combine(SceneManager.Instance.m_outputPath, root.name), "GameLogic");
                    Directory.CreateDirectory(gameLogicPath);
                    string gameLogicAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(gameLogicPath, string.Format("{0}.asset", m_Behavior.name)));

                    AssetDatabase.CreateAsset(CreateInstance<GameLogicData>(), gameLogicAssetPath);
                    AssetDatabase.SaveAssets();

                    m_GameLogicData.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameLogicData>(gameLogicAssetPath);
                }

                GameLogicGraphViewWindow graphViewWindow = EditorWindow.GetWindow<GameLogicGraphViewWindow>();
                if (graphViewWindow != null)
                {
                    graphViewWindow.Focus();
                    graphViewWindow.ShowScript((GameLogicData)m_GameLogicData.objectReferenceValue);
                }
            }
            GUI.color = oldColor;

            GUILayout.Space(10);
            base.OnInspectorGUI();
        }
    }
}
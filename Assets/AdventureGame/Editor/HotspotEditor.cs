using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(Hotspot))]
    public class HotspotEditor : BaseAreaEditor
    {
        SerializedProperty m_GameLogicData;

        public override void OnEnable()
        {
            base.OnEnable();
            m_GameLogicData = serializedObject.FindProperty("m_gameLogicData");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_GameLogicData);

            base.OnInspectorGUI();
        }
    }
}
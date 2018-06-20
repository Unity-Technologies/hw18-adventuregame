using UnityEditor;

namespace UnityEngine.AdventureGame
{
    [CustomEditor(typeof(WalkableAreaGroup))]
    public class WalkableAreaGroupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WalkableAreaGroup walkableAreaGroup = (WalkableAreaGroup)target;
            Color oldColor = GUI.color;
            
            GUILayout.Space(15);
            GUI.color = Color.green;
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            if (GUILayout.Button("Add New Walkable Area", GUILayout.Height(50)))
            {
                GameObject gameObject = new GameObject();
                gameObject.transform.SetParent(walkableAreaGroup.transform, false);
                gameObject.name = GameObjectUtility.GetUniqueNameForSibling(walkableAreaGroup.transform, "WalkableArea");
                gameObject.AddComponent<WalkableArea>();
                Selection.activeGameObject = gameObject;
            }
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }
    }
}

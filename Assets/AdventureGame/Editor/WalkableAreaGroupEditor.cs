using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(WalkableAreaGroup))]
    public class WalkableAreaGroupEditor : BaseGroupEditor
    {
        public override string Name { get { return "Walkable Area"; } }

        public override void CreateArea(GameObject parent)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(parent.transform, false);
            gameObject.name = GameObjectUtility.GetUniqueNameForSibling(parent.transform, "WalkableArea");
            gameObject.AddComponent<WalkableArea>();
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}

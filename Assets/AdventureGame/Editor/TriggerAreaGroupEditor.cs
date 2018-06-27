using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(TriggerAreaGroup))]
    public class TriggerAreaGroupEditor : BaseGroupEditor
    {
        public override string Name { get {return "Trigger Area";} }

        public override void CreateArea(GameObject parent)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(parent.transform, false);
            gameObject.name = GameObjectUtility.GetUniqueNameForSibling(parent.transform, "TriggerArea");

            PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.pathCount = 0;
            gameObject.AddComponent<TriggerArea>();
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}

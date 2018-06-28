using UnityEngine;
using UnityEngine.AdventureGame;

namespace UnityEditor.AdventureGame
{
    [CustomEditor(typeof(HotspotGroup))]
    public class HotspotGroupEditor : BaseGroupEditor
    {
        public override string Name { get {return "Hotspot";} }

        public override void CreateArea(GameObject parent)
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(parent.transform, false);
            gameObject.name = GameObjectUtility.GetUniqueNameForSibling(parent.transform, "Hotspot");

            PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.pathCount = 0;
            gameObject.AddComponent<Hotspot>();
            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine.AI;

namespace UnityEngine.AdventureGame
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class Hotspot : Interactable
    {
#if UNITY_EDITOR
        public Sprite m_sprite;
        public Color m_color = new Color(1.0f, 1.0f, 0.0f, 0.25f);

        [Range(0.0f, 1.0f)]
        public float m_detail = 0.5f;
#endif
    }
}
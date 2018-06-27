namespace UnityEngine.AdventureGame
{
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(Interactable))]
    public class Hotspot : Interactable, IBaseArea
    {
#if UNITY_EDITOR
        public Sprite m_sprite;
        public Color m_color = new Color(1.0f, 1.0f, 0.0f, 0.25f);

        [Range(0.0f, 1.0f)]
        public float m_detail = 0.5f;

        public Sprite Sprite
        {
            get { return m_sprite; }
            set { m_sprite = value; }
        }

        public Color Color
        {
            get { return m_color; }
            set { m_color = value; }
        }

        public float Detail
        {
            get { return m_detail; }
            set { m_detail = value; }
        }
#endif
    }
}
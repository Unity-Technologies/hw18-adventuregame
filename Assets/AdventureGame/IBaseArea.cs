using UnityEngine;

namespace UnityEngine.AdventureGame
{
    public interface IBaseArea
    {
#if UNITY_EDITOR
        Sprite Sprite { get; set; }
        Color Color { get; set; }
        float Detail { get; set; }
#endif
    }
}
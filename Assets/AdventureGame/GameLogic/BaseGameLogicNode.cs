using System;
using UnityEditor.Experimental.UIElements.GraphView;

namespace UnityEngine.AdventureGame
{
    [Serializable]
    public abstract class BaseGameLogicNode
    {
        public abstract void Execute();
    }
}
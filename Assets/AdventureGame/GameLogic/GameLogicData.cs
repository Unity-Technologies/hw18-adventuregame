using System;
using System.Collections.Generic;

namespace UnityEngine.AdventureGame
{
    [CreateAssetMenu(fileName = "GameLogic", menuName = "GameLogic", order = 1)]
    public class GameLogicData : ScriptableObject
    {
        [Serializable]
        public class GameLogicGraphNode
        {
            public string m_type;
            public string m_typeData;
            public string m_title;
            public Vector2 m_position;
            public List<GameLogicGraphEdge> m_outputs;
        }

        [Serializable]
        public class GameLogicGraphEdge
        {
            public int m_sourcePort;
            public int m_targetNode;
            public int m_targetPort;
        }

        public List<GameLogicGraphNode> m_graphNodes = new List<GameLogicGraphNode>();
    }
}

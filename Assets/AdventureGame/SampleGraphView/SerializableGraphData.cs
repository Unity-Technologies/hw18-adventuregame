using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class SerializableGraphData : ScriptableObject
{
    [Serializable]
    public class SerializableGraphNode
    {
        public string                      m_title;
        public Vector2                     m_position;
        public List<SerializableGraphEdge> m_outputs;
    }

    [Serializable]
    public class SerializableGraphEdge
    {
        public int m_sourcePort;
        public int m_targetNode;
        public int m_targetPort;
    }
    
    public List<SerializableGraphNode> m_graphNodes;
}

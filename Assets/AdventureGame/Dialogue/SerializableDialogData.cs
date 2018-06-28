using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

public class SerializableDialogData : ScriptableObject
{
    [Serializable]
    public class SerializableDialogNode
    {
        public string m_title;
        public string m_speakingCharacterName;
        public string m_characterDialogue;
        public Vector2 m_position;
        public int inputNodeCount;
        public int outputNodeCount;
        public List<SerializableDialogEdge> m_outputs;
        public List<string> m_outputDialogs;
    }

    [Serializable]
    public class SerializableDialogEdge
    {
        public int m_sourcePort;
        public int m_targetNode;
        public int m_targetPort;
    }

    public List<SerializableDialogNode> m_dialogNodes;
}

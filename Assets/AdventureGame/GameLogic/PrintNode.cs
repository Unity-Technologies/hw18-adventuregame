using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityEngine.AdventureGame
{
    public static class PrintNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            Debug.LogFormat("Output is: {0}", currentNode.m_typeData);
            yield break;
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Print";

            node.mainContainer.style.backgroundColor = Color.magenta;

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            Port outputPort1 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort1.portName = "";
            outputPort1.userData = null;
            node.outputContainer.Add(outputPort1);

            var characterName = new TextField()
            {
                multiline = false,
                value = typeData
            };

            node.mainContainer.Insert(1, characterName);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is TextField)
                {
                    return (ele as TextField).value;
                }
            }

            return null;
        }
#endif
    }
}
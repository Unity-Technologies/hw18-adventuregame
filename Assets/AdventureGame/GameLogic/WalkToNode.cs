using System;
using System.Collections;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class WalkToNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            Transform trans = SceneManager.Instance.GetLocator(currentNode.m_typeData);
            if (trans == null)
            {
                Debug.LogErrorFormat("Could not find locator: {0}", currentNode.m_typeData);
                yield break;
            }

            SceneManager.Instance.Character.WalkToPosition(trans.position);

            while (!SceneManager.Instance.Character.IsAtDestination())
            {
                yield return null;
            }

            yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Walk To";

            node.mainContainer.style.backgroundColor = Color.blue;

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
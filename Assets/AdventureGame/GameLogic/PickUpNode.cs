using System;
using System.Collections;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class PickUpNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            InventoryItem inventoryItem = SceneManager.Instance.GetInventoryItem(currentNode.m_typeData);
            if (inventoryItem == null)
            {
                Debug.LogErrorFormat("Could not find inventory item: {0}", currentNode.m_typeData);
                yield break;
            }

            inventoryItem.PickedUp();

            yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Pick Up";

            node.mainContainer.style.backgroundColor = Color.cyan;

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            Port outputPort1 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort1.portName = "";
            outputPort1.userData = null;
            node.outputContainer.Add(outputPort1);

            var targetId = new TextField()
            {
                multiline = false,
                value = typeData
            };

            node.mainContainer.Insert(1, targetId);

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
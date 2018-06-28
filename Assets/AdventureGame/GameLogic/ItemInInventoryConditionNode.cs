using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AdventureGame;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class ItemInInventoryConditionNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            bool itemInInventory = InventoryManager.Instance.GetItemWithId(currentNode.m_typeData) != null;
	        yield return currentNode.GetReturnValue(itemInInventory ? 0 : 1);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Item In Inventory Condition";

	        node.mainContainer.style.backgroundColor = Color.cyan;

			node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            var targetId = new TextField()
            {
                multiline = false,
                value = typeData
            };

            node.mainContainer.Insert(1, targetId);

            Port outputPort1 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort1.portName = "true";
            outputPort1.userData = null;
            node.outputContainer.Add(outputPort1);

            Port outputPort2 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort2.portName = "false";
            outputPort2.userData = null;
            node.outputContainer.Add(outputPort2);

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
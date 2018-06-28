using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AdventureGame;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    //returns true if an inventory item with the given id is selected
    public static class SelectedInventoryItemConditionNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            InventoryItem selected = InventoryManager.Instance.Selected;
	        yield return currentNode.GetReturnValue(selected && selected.Id == currentNode.m_typeData ? 0 : 1);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Selected Inventory Item Condition";

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
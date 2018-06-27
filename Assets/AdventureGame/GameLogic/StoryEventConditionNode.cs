using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class StoryEventConditionNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            yield return null;
            yield return null;

            //if true
            yield return currentNode.m_outputs[0].m_targetNode;

            //if false
            yield return currentNode.m_outputs[1].m_targetNode;
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "StoryEventCondition";

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            Port outputPort1 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort1.portName = "true";
            outputPort1.userData = null;
            node.outputContainer.Add(outputPort1);

            Port outputPort2 = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort2.portName = "false";
            outputPort2.userData = null;
            node.outputContainer.Add(outputPort2);

	        List<string> storyEvents = StoryEventsDatabase.StoryEventDatabase != null ? StoryEventsDatabase.StoryEventDatabase.events
										: new List<string>();

	        var storyEventsDropdown =
		        new PopupField<string>(storyEvents, string.IsNullOrEmpty(typeData) ? 0 : storyEvents.FindIndex((x) => string.Equals(x, typeData)));
			node.mainContainer.Insert(1, storyEventsDropdown);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is PopupField<string>)
                {
                    return (ele as PopupField<string>).value;
                }
            }

            return null;
        }
#endif
    }
}
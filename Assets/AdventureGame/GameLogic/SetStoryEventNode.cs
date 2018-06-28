using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AdventureGame;
#if UNITY_EDITOR
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityEngine.AdventureGame
{
    public static class SetStoryEventNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            PersistentDataManager.Instance.AddFinishedStoryEvent(currentNode.m_typeData);

            //if true return first return value if false return second
	        yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Set Story Event";

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

	        List<string> storyEvents = StoryEventsDatabase.StoryEventDatabase != null ? StoryEventsDatabase.StoryEventDatabase.events
										: new List<string>();

	        var storyEventsDropdown =
		        new PopupField<string>(storyEvents,
										string.IsNullOrEmpty(typeData) || !storyEvents.Exists((x) => string.Equals(x, typeData)) ? 0
											: storyEvents.FindIndex((x) => string.Equals(x, typeData)));
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AdventureGame;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class WaitSecondsNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            int waitSeconds;
            if (!int.TryParse(currentNode.m_typeData, out waitSeconds))
            {
                yield break;
            }
            float elapsedTime = 0.0f;
            while (elapsedTime < waitSeconds)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
	        yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Wait Seconds";

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

            int waitSeconds = 0;
            int.TryParse(typeData, out waitSeconds);

            var secondsField = new IntegerField()
	        {
                value = waitSeconds
	        };
			node.mainContainer.Insert(1, secondsField);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is IntegerField)
                {
                    return (ele as IntegerField).value.ToString();
                }
            }

            return null;
        }
#endif
    }
}
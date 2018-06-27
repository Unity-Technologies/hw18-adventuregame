﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AdventureGame;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class TriggerScene
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            SceneManager.Instance.TriggerDoorway();
            yield break;
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData, Action changeCallback)
        {
            Node node = new Node();
            node.title = "Trigger Scene";

	        node.mainContainer.style.backgroundColor = Color.blue;

			node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

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
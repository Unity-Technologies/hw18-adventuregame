using System;
using System.Collections;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class TriggerSingleLineNode
    {
        [Serializable]
        struct SingleLineNodeData
        {
            public string character;
            public string dialogue;
        }

        static bool s_WaitingOnDialogue;

        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            var data = JsonUtility.FromJson<SingleLineNodeData>(currentNode.m_typeData);
            AdventureGameOverlayManager.Instance.DisplayCharacterDialogue(data.dialogue, data.character, OnDialogueEnd);

            s_WaitingOnDialogue = true;

            while (s_WaitingOnDialogue)
            {
                yield return null;
            }
        }

        static void OnDialogueEnd()
        {
            s_WaitingOnDialogue = false;
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Trigger Single Line";

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

            SingleLineNodeData data;

            try
            {
                data = JsonUtility.FromJson<SingleLineNodeData>(typeData);
            }
            catch (Exception)
            {
                Debug.LogError(e);
                data = new SingleLineNodeData();
            }

            var characterLabel = new Label
            {
                text = "Character"
            };

            node.mainContainer.Insert(1, characterLabel);

            var character = new TextField
            {
                multiline = false,
                value = data.character,
                name = "character"
            };

            node.mainContainer.Insert(2, character);

            var dialogueLabel = new Label
            {
                text = "Dialogue"
            };

            node.mainContainer.Insert(3, dialogueLabel);

            var dialogue = new TextField
            {
                multiline = false,
                value = data.dialogue,
                name = "dialogue"
            };

            node.mainContainer.Insert(4, dialogue);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            SingleLineNodeData data;
            data.character = node.mainContainer.Q<TextField>("character").value;
            data.dialogue = node.mainContainer.Q<TextField>("dialogue").value;

            return JsonUtility.ToJson(data);
        }
#endif
    }
}
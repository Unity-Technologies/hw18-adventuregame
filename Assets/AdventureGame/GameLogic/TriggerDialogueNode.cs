using System;
using System.Collections;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AdventureGame
{
    public static class TriggerDialogNode
    {
        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            string path = AssetDatabase.GUIDToAssetPath(currentNode.m_typeData);
            DialogueManager.Instance.StartDialogue(AssetDatabase.LoadAssetAtPath<SerializableDialogData>(path));

            //if true return first return value if false return second
	        yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData, Action changeCallback)
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

            var dialogOptions = new ObjectField()
            {
                objectType = typeof(SerializableDialogData),
                allowSceneObjects = false
            };
            string path = AssetDatabase.GUIDToAssetPath(typeData);
            dialogOptions.value = AssetDatabase.LoadAssetAtPath<SerializableDialogData>(path);
			node.mainContainer.Insert(1, dialogOptions);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is ObjectField)
                {
                    ObjectField field = (ele as ObjectField);
                    string path = AssetDatabase.GetAssetPath(field.value);
                    return AssetDatabase.AssetPathToGUID(path);
                }
            }

            return null;
        }
#endif
    }
}
using System;
using System.Collections;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class SetBoolAnimParameterNode
    {
        [Serializable]
        public class SetBoolAnimParameterSettings
        {
            public string m_AnimParameterName;
            public bool   m_Value;
        }

        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            SetBoolAnimParameterSettings settings = JsonUtility.FromJson<SetBoolAnimParameterSettings>(currentNode.m_typeData);
            SceneManager.Instance.Character.Animator.SetBool(settings.m_AnimParameterName, settings.m_Value);
            yield return currentNode.GetReturnValue(0);
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Set Bool Anim Parameter";

	        node.mainContainer.style.backgroundColor = Color.blue;

			node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            SetBoolAnimParameterSettings settings = JsonUtility.FromJson<SetBoolAnimParameterSettings>(typeData);
            if (settings == null)
            {
                settings = new SetBoolAnimParameterSettings();
            }

            var animParameterTextField = new TextField()
            {
                multiline = false,
                value = settings.m_AnimParameterName
            };
            node.mainContainer.Insert(1, animParameterTextField);

            var animParameterValueToggle = new Toggle()
            {
                value = settings.m_Value
            };
            node.mainContainer.Insert(2, animParameterValueToggle);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            SetBoolAnimParameterSettings settings = new SetBoolAnimParameterSettings();
            
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is TextField)
                {
                    settings.m_AnimParameterName = (ele as TextField).value;
                }
                else if (ele is Toggle)
                {
                    settings.m_Value = (ele as Toggle).value;
                }
            }

            return JsonUtility.ToJson(settings);
        }
#endif
    }
}
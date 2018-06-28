using System;
using System.Collections;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEngine.AdventureGame
{
    public static class SetGameObjectActiveNode
    {
        [Serializable]
        public class SetGameObjectActiveNodeSettings
        {
            public string m_GameObjectName;
            public bool   m_Value;
        }

        public static IEnumerator Execute(GameLogicData.GameLogicGraphNode currentNode)
        {
            SetGameObjectActiveNodeSettings settings = JsonUtility.FromJson<SetGameObjectActiveNodeSettings>(currentNode.m_typeData);
            GameObject[] objects = SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < objects.Length; ++i)
            {
                RecursiveSetActive(objects[i], settings);
            }
            yield return currentNode.GetReturnValue(0);
        }

        static void RecursiveSetActive(GameObject go, SetGameObjectActiveNodeSettings setting)
        {
            if (go.name == setting.m_GameObjectName)
            {
                go.SetActive(setting.m_Value);
                return;
            }

            foreach (Transform t in go.transform)
            {
                RecursiveSetActive(t.gameObject, setting);
            }
        }

#if UNITY_EDITOR
        public static Node CreateNode(string typeData)
        {
            Node node = new Node();
            node.title = "Set GameObject Active";

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

            SetGameObjectActiveNodeSettings settings = JsonUtility.FromJson<SetGameObjectActiveNodeSettings>(typeData);
            if (settings == null)
            {
                settings = new SetGameObjectActiveNodeSettings();
            }

            var gameObjectPath = new TextField()
            {
                multiline = false,
                value = settings.m_GameObjectName
            };
            node.mainContainer.Insert(1, gameObjectPath);

            var activeToggle = new Toggle()
            {
                value = settings.m_Value
            };
            node.mainContainer.Insert(2, activeToggle);

            return node;
        }

        public static string ExtractExtraData(Node node)
        {
            SetGameObjectActiveNodeSettings settings = new SetGameObjectActiveNodeSettings();
            
            foreach (VisualElement ele in node.mainContainer)
            {
                if (ele is TextField)
                {
                    settings.m_GameObjectName = (ele as TextField).value;
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
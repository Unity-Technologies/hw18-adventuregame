using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.AdventureGame
{
    [CreateAssetMenu(fileName = "GameLogic", menuName = "GameLogic", order = 1)]
    public class GameLogicData : ScriptableObject
    {
        [Serializable]
        public class GameLogicGraphNode
        {
            public string m_type;
            public string m_typeData;
            public string m_title;
            public Vector2 m_position;
            public List<GameLogicGraphEdge> m_outputs;


            public int GetReturnValue(int index)
            {
                return (index < m_outputs.Count) ? m_outputs[index].m_targetNode : -1;
            }
        }

        [Serializable]
        public class GameLogicGraphEdge
        {
            public int m_sourcePort;
            public int m_targetNode;
            public int m_targetPort;
        }

        public List<GameLogicGraphNode> m_graphNodes = new List<GameLogicGraphNode>();

        public IEnumerator Execute()
        {
            // find the start node and execute it
            for (int i = 0; i < m_graphNodes.Count; ++i)
            {
                if (string.IsNullOrEmpty(m_graphNodes[i].m_type) && m_graphNodes[i].m_outputs.Count > 0)
                {
                    yield return ExecuteNode(m_graphNodes[m_graphNodes[i].m_outputs[0].m_targetNode]);
                    yield break;
                }
            }
        }

        IEnumerator ExecuteNode(GameLogicGraphNode node)
        {
            Type type = Type.GetType(string.Format("{0}, Assembly-CSharp", node.m_type));
            if (type == null)
            {
                Debug.LogErrorFormat("Failed to find type: {0}!", node.m_type);
                yield break;
            }

            MethodInfo method = type.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                Debug.LogError("Failed to find method Execute()!");
                yield break;
            }
            
            SceneManager.Instance.Character.Controllable = false;
            int next = -1;
            IEnumerator enumerator = (IEnumerator)method.Invoke(null, new object[] { node });
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    yield return null;
                }
                else if (enumerator.Current is int)
                {
                    next = (int)enumerator.Current;
                    break;
                }
            }
            if (next > 0)
            {
                yield return ExecuteNode(m_graphNodes[next]);
            }
            SceneManager.Instance.Character.Controllable = true;
        }
    }
}

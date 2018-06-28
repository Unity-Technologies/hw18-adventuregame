using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.AdventureGame;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.AdventureGame
{
    public class GameLogicGraphViewWindow : EditorWindow, ISearchWindowProvider
    {
        GameLogicGraphView m_GraphView;
        GameLogicData m_GameLogicData;

        [OnOpenAsset(1)]
        public static bool OpenGameLogicFromAsset(int instanceID, int line)
        {
            GameLogicData data = EditorUtility.InstanceIDToObject(instanceID) as GameLogicData;
            OpenWindow(data);
            return data != null; // we did not handle the open
        }

        [MenuItem("Adventure Game/Game Logic Window &g")]
        public static void OpenWindow()
        {
            GetWindow<GameLogicGraphViewWindow>("Game Logic", true, typeof(SceneView));
        }

        public static void OpenWindow(GameLogicData data)
        {
            if (data != null)
            {
                GameLogicGraphViewWindow view = GetWindow<GameLogicGraphViewWindow>("Game Logic", true, typeof(SceneView));
                view.ShowScript(data);
            }
        }

        public void ShowScript(GameLogicData data)
        {
            m_GameLogicData = data;

            if (!LoadGraphData())
            {
                Node node = CreateRootNode();
                m_GraphView.AddElement(node);
            }
        }

        // Use this for initialization
        void OnEnable()
        {
            var sampleGraphView = new GameLogicGraphView();
            m_GraphView = sampleGraphView;

            m_GraphView.name = "theView";
            m_GraphView.persistenceKey = "theView";
            m_GraphView.StretchToParentSize();

            this.GetRootVisualContainer().Add(m_GraphView);

            OnSelectionChanged();

            m_GraphView.graphViewChanged += OnGraphViewChanged;
            m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        void OnLostFocus()
        {
            SaveGraphData();
        }

        void OnSelectionChanged()
        {
            GameLogicData[] data = Selection.GetFiltered<GameLogicData>(SelectionMode.Assets);
            if (data.Length != 1)
            {
                m_GameLogicData = null;
                return;
            }
            m_GameLogicData = data[0];

            if (!LoadGraphData())
            {
                Node node = CreateRootNode();
                m_GraphView.AddElement(node);
            }
        }

        protected void OnRequestNodeCreation(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
        }

        public GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            EditorApplication.update += DelayedSaveGraphData;
            return change;
        }

        void DelayedSaveGraphData()
        {
            EditorApplication.update -= DelayedSaveGraphData;
            SaveGraphData();
        }

        Node CreateRootNode()
        {
            Node node = new Node();
            node.title = "Start";
            node.capabilities &= ~(Capabilities.Movable | Capabilities.Deletable);
            node.mainContainer.style.backgroundColor = Color.green;
            node.SetPosition(new Rect(new Vector2(-50, -50), new Vector2(100, 100)));

            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "";
            outputPort.userData = null;

            node.outputContainer.Add(outputPort);
            return node;
        }

        Node DeserializeNode(GameLogicData.GameLogicGraphNode graphNode)
        {
            Node node;
            if (string.IsNullOrEmpty(graphNode.m_type))
            {
                node = CreateRootNode();
                node.userData = null;
            }
            else
            {
                Type type = Type.GetType(string.Format("{0}, Assembly-CSharp", graphNode.m_type));
                if (type == null)
                {
                    Debug.LogErrorFormat("Failed to find type: {0}", graphNode.m_type);
                    return null;
                }
                node = CreateNodeFromType(type, graphNode.m_typeData);
                node.userData = type;
            }

            node.title = graphNode.m_title;
            node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
            return node;
        }

        Node CreateNodeFromType(Type type, string typedata)
        {
            MethodInfo method = type.GetMethod("CreateNode", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                Debug.LogError("Failed to find method CreateNode()!");
                return null;
            }
            
            Node node = method.Invoke(null, new object[] {typedata}) as Node;
            if (node == null)
            {
                Debug.LogError("Failed to create node!");
                return null;
            }

            return node;
        }

        SearchTreeEntry CreateSearchTreeEntry(Texture2D icon, int level, Type type)
        {
            return new SearchTreeEntry(new GUIContent(type.Name.Substring(0, type.Name.LastIndexOf("Node")), icon)) { level = level, userData = type };
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Conditionals"), 1));
            tree.Add(CreateSearchTreeEntry(icon, 2, typeof(StoryEventConditionNode)));
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Actions"), 1));
            tree.Add(CreateSearchTreeEntry(icon, 2, typeof(PrintNode)));
            tree.Add(CreateSearchTreeEntry(icon, 2, typeof(WalkToNode)));
			tree.Add(CreateSearchTreeEntry(icon, 2, typeof(PickUpNode)));
			tree.Add(CreateSearchTreeEntry(icon, 2, typeof(SetStoryEventNode)));
            tree.Add(CreateSearchTreeEntry(icon, 2, typeof(TriggerSceneNode)));
            tree.Add(CreateSearchTreeEntry(icon, 2, typeof(TriggerDialogNode)));

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (!(entry is SearchTreeGroupEntry))
            {
                Type createType = entry.userData as Type;
                Node node = CreateNodeFromType(createType, null);
                if (node == null)
                {
                    return false;
                }

                node.userData = createType;
                m_GraphView.AddElement(node);

                Vector2 pointInWindow = context.screenMousePosition - position.position;
                Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);
                node.SetPosition(new Rect(pointInGraph, Vector2.zero));
                node.Select(m_GraphView, false);

                SaveGraphData();

                return true;
            }

            return false;
        }

        public void SaveGraphData()
        {
            if (m_GameLogicData == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(m_GameLogicData);

            List<Node> nodes = m_GraphView.nodes.ToList();
            GameLogicData graphData = ScriptableObject.CreateInstance<GameLogicData>();
            graphData.m_graphNodes = new List<GameLogicData.GameLogicGraphNode>();
            for (int i = 0; i < nodes.Count; ++i)
            {
                Node currentNode = nodes[i];
                Type nodeType = currentNode.userData as Type;
                GameLogicData.GameLogicGraphNode graphNode = new GameLogicData.GameLogicGraphNode();
                if (nodeType == null)
                {
                    graphNode.m_type = null;
                    graphNode.m_typeData = null;
                }
                else
                {
                    MethodInfo method = nodeType.GetMethod("ExtractExtraData", BindingFlags.Static | BindingFlags.Public);
                    if (method == null)
                    {
                        Debug.LogError("Failed to find method ExtractExtraData()!");
                        return;
                    }

                    graphNode.m_type = nodeType.FullName;
                    graphNode.m_typeData = (string)method.Invoke(null, new object[] { currentNode });
                }
                graphNode.m_title = currentNode.title;
                graphNode.m_position = currentNode.GetPosition().position;
                graphNode.m_outputs = new List<GameLogicData.GameLogicGraphEdge>();

                foreach (Port port in currentNode.outputContainer)
                {
                    foreach (Edge edge in port.connections)
                    {
                        GameLogicData.GameLogicGraphEdge serializedEdge = new GameLogicData.GameLogicGraphEdge();
                        serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                        serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                        serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);
                        graphNode.m_outputs.Add(serializedEdge);
                    }
                }
                
                graphData.m_graphNodes.Add(graphNode);
            }

            EditorUtility.CopySerialized(graphData, m_GameLogicData);
            AssetDatabase.SaveAssets();

            m_GameLogicData = AssetDatabase.LoadAssetAtPath<GameLogicData>(assetPath);
        }

        public bool LoadGraphData()
        {
	        List<Node> removeNodes = m_GraphView.nodes.ToList();
	        foreach (Node node in removeNodes)
	        {
		        m_GraphView.RemoveElement(node);
	        }

	        List<Edge> removeEdges = m_GraphView.edges.ToList();
	        foreach (Edge edge in removeEdges)
	        {
		        m_GraphView.RemoveElement(edge);
	        }

			if (m_GameLogicData.m_graphNodes.Count == 0)
            {
                return false;
            }

            // create the nodes
            List<Node> createdNodes = new List<Node>();
            for (int i = 0; i < m_GameLogicData.m_graphNodes.Count; ++i)
            {
                Node node = DeserializeNode(m_GameLogicData.m_graphNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);
            }

            //connect the nodes
            for (int i = 0; i < m_GameLogicData.m_graphNodes.Count; ++i)
            {
                for (int iEdge = 0; iEdge < m_GameLogicData.m_graphNodes[i].m_outputs.Count; ++iEdge)
                {
                    GameLogicData.GameLogicGraphEdge edge = m_GameLogicData.m_graphNodes[i].m_outputs[iEdge];
                    Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort] as Port;
                    Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                    m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
                }
            }

            return true;
        }
    }
}
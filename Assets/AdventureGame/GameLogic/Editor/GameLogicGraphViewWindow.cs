using System;
using System.Collections.Generic;
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

        [MenuItem("Adventure Game/Game Logic Open")]
        public static void OpenWindow()
        {
            GetWindow<GameLogicGraphViewWindow>();
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
            outputPort.portName = "start";
            outputPort.userData = null;

            node.outputContainer.Add(outputPort);
            return node;
        }

        Node DeserializeNode(GameLogicData.GameLogicGraphNode graphNode)
        {
            Node node = graphNode.m_title == "Start" ? CreateRootNode() : CreateNode(graphNode.m_title);
            node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
            return node;
        }

        Node CreateNode(string title)
        {
            Node node = new Node();
            node.title = title;

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "in port";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "out port";
            outputPort.userData = null;
            node.outputContainer.Add(outputPort);

            return node;
        }

        Node CreateEndNode(string title)
        {
            Node node = new Node();
            node.title = title;

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "action";
            inputPort.userData = null;
            node.inputContainer.Add(inputPort);

            return node;
        }

        Node CreateConditionNode(string title)
        {
            Node node = new Node();
            node.title = title;

            node.capabilities |= Capabilities.Movable;
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            inputPort.portName = "in port";
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

            return node;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Category"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Condition", icon)) { level = 2, userData = typeof(GameLogicConditionNode) });
            tree.Add(new SearchTreeEntry(new GUIContent("End node", icon)) { level = 2, userData = typeof(GameLogicEndNode) });

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (!(entry is SearchTreeGroupEntry))
            {
                GameLogicNode gameLogicNode = ScriptableObject.CreateInstance(entry.userData as Type) as GameLogicNode;
                Node node = null;

                if (gameLogicNode is GameLogicConditionNode)
                {
                    node = CreateConditionNode("Default");
                }
                else if (gameLogicNode is GameLogicEndNode)
                {
                    node = CreateEndNode("Default");
                }

                if (node != null)
                {
                    m_GraphView.AddElement(node);

                    Vector2 pointInWindow = context.screenMousePosition - position.position;
                    Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);
                    node.SetPosition(new Rect(pointInGraph, Vector2.zero));
                    node.Select(m_GraphView, false);

                    SaveGraphData();
                }

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
                GameLogicData.GameLogicGraphNode graphNode = new GameLogicData.GameLogicGraphNode();
                Node currentNode = nodes[i];

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

            AssetDatabase.CreateAsset(graphData, assetPath);
            AssetDatabase.SaveAssets();
        }

        public bool LoadGraphData()
        {
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
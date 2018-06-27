using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Unity.Adventuregame {
    public class DialogueGraphViewWindow : EditorWindow, ISearchWindowProvider
    {

        [MenuItem("Dialogue/Open Window")]
        public static void OpenWindow()
        {
            GetWindow<DialogueGraphViewWindow>();
        }

        const string k_TestGraphDataPath = "Assets/AdventureGame/Dialogue/Test.asset";

        protected SampleGraphView m_GraphView;


        // Use this for initialization
        void OnEnable()
        {
            var sampleGraphView = new SampleGraphView();
            m_GraphView = sampleGraphView;

            m_GraphView.name = "theView";
            m_GraphView.persistenceKey = "theView";
            m_GraphView.StretchToParentSize();

            this.GetRootVisualContainer().Add(m_GraphView);

            if (!LoadGraphData(k_TestGraphDataPath))
            {
                Node node = CreateRootNode();
                m_GraphView.AddElement(node);
            }

            m_GraphView.graphViewChanged += OnGraphViewChanged;
            m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
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
            SaveGraphData(m_GraphView.nodes.ToList(), k_TestGraphDataPath);
        }

        Node CreateRootNode()
        {
            Node node = new Node();
            node.title = "Start";
            node.capabilities &= ~(Capabilities.Movable | Capabilities.Deletable);
            node.style.backgroundColor = Color.green;
            node.SetPosition(new Rect(new Vector2(-50, -50), new Vector2(100, 100)));

            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                typeof(bool));
            outputPort.portName = "start";
            outputPort.userData = null;

            node.outputContainer.Add(outputPort);

            return node;
        }

        Node DeserializeNode(SerializableGraphData.SerializableGraphNode graphNode)
        {
            Node node = graphNode.m_title == "Start" ? CreateRootNode() : CreateNode(graphNode.m_title, 1, 1);
            node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
            return node;
        }

        protected Node CreateNode(string title, int inNodes, int outNodes)
        {
            Node node = new Node();
            node.title = title;

            node.capabilities |= Capabilities.Movable;
            for (int i = 0; i < inNodes; i++)
            {
                Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                    typeof(bool));
                node.inputContainer.Add(inputPort);
            }

            for (int i = 0; i < outNodes; i++)
            {
                Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                    typeof(bool));
                node.outputContainer.Add(outputPort);
            }

            return node;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Test Category"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Test Item1", icon))
            {
                level = 2,
                userData = typeof(SampleGraphView)
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Test Item2", icon))
            {
                level = 2,
                userData = typeof(SampleGraphView)
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Test Item3", icon))
            {
                level = 2,
                userData = typeof(SampleGraphView)
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Test Item4", icon))
            {
                level = 2,
                userData = typeof(SampleGraphView)
            });
            tree.Add(new SearchTreeEntry(new GUIContent("Create Dialogue", icon)) {level = 2});

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.name == "Create Dialogue")
            {
                var node = new DialogueNode();

                var characterName = new TextField()
                {
                    multiline = false,
                    value = "<CharacerName Here>"
                };

                node.mainContainer.Insert(1, characterName);
                m_GraphView.AddElement(node);
                node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));

                m_GraphView.AddElement(node);

                Vector2 pointInWindow = context.screenMousePosition - position.position;
                Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

                node.SetPosition(new Rect(pointInGraph,
                    Vector2.zero)); // it's ok to pass zero here because width/height is dynamic

                node.Select(m_GraphView, false);
                return true;
            }

            return OnSelectEntry(entry, context);
        }

        public void SaveGraphData(List<Node> nodes, string outputPath)
        {
            SerializableDialogData dialogGraphData = ScriptableObject.CreateInstance<SerializableDialogData>();
            dialogGraphData.m_dialogNodes = new List<SerializableDialogData.SerializableDialogNode>();

            string inputString = string.Empty;
            string outPutString = string.Empty;

            for (int i = 0; i < nodes.Count; ++i)
            {
                SerializableDialogData.SerializableDialogNode dialogGraphNode =
                    new SerializableDialogData.SerializableDialogNode();
                Node currentNode = nodes[i];

                dialogGraphNode.m_title = currentNode.title;
                dialogGraphNode.m_position = currentNode.GetPosition().position;
                dialogGraphNode.m_outputs = new List<SerializableDialogData.SerializableDialogEdge>();

                foreach (VisualElement element in currentNode.mainContainer)
                {
                    if (element is TextField)
                    {
                        dialogGraphNode.m_speakingCharacterName = ((TextField) element).value;
                    }
                }

                foreach (VisualElement element in currentNode.inputContainer)
                {
                    if (element is TextField)
                    {
                        inputString = ((TextField) element).value;
                    }
                }

                foreach (VisualElement element in currentNode.outputContainer)
                {
                    if (element is TextField)
                    {

                        outPutString = ((TextField) element).value;
                    }

                    if (element is Port)
                    {
                        foreach (Edge edge in ((Port) element).connections)
                        {
                            SerializableDialogData.SerializableDialogEdge serializedEdge =
                                new SerializableDialogData.SerializableDialogEdge();
                            if (dialogGraphNode.m_title == "Start")
                            {
                                serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                                serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                            }
                            else
                            {
                                serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                                serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);

                                serializedEdge.m_outputDialog = outPutString;
                                if (outPutString != string.Empty)
                                {
                                    outPutString = string.Empty;
                                }

                                serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);

                                serializedEdge.m_inputDialog = inputString;
                                if (inputString != string.Empty)
                                {
                                    inputString = string.Empty;
                                }
                            }

                            dialogGraphNode.m_outputs.Add(serializedEdge);
                        }
                    }
                }

                dialogGraphData.m_dialogNodes.Add(dialogGraphNode);
            }

            AssetDatabase.CreateAsset(dialogGraphData, outputPath);
            AssetDatabase.SaveAssets();
        }

        public virtual bool LoadGraphData(string inputPath)
        {
            SerializableGraphData graphData = AssetDatabase.LoadAssetAtPath<SerializableGraphData>(inputPath);
            if (graphData == null)
            {
                return false;
            }

            // create the nodes
            List<Node> createdNodes = new List<Node>();
            for (int i = 0; i < graphData.m_graphNodes.Count; ++i)
            {
                Node node = DeserializeNode(graphData.m_graphNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);
            }

            //connect the nodes
            for (int i = 0; i < graphData.m_graphNodes.Count; ++i)
            {
                for (int iEdge = 0; iEdge < graphData.m_graphNodes[i].m_outputs.Count; ++iEdge)
                {
                    SerializableGraphData.SerializableGraphEdge edge = graphData.m_graphNodes[i].m_outputs[iEdge];
                    Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort] as Port;
                    Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                    m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
                }
            }

            return true;
        }
    }
}
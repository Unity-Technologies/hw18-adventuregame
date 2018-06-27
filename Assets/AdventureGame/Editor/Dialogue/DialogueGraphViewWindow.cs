using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Unity.Adventuregame {
    public class DialogueGraphViewWindow : EditorWindow, ISearchWindowProvider
    {
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
                DialogueNode node = CreateRootNode();
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

        DialogueNode CreateRootNode()
        {
            DialogueNode node = new DialogueNode();
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

        DialogueNode DeserializeNode(SerializableDialogData.SerializableDialogNode graphNode)
        {
            DialogueNode node = graphNode.m_title == "Start" ? CreateRootNode() : CreateDialogueNode(graphNode.m_title, 1, 1);
            node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
            return node;
        }

        protected DialogueNode CreateDialogueNode(string title, int inNodes, int outNodes)
        {
            DialogueNode node = new DialogueNode();
            node.title = title;

            var characterName = new TextField()
            {
                multiline = false,
                value = "<CharacterName Here>",
                name = "characterName"
            };
            node.mainContainer.Insert(1, characterName);

            var characterDialogue = new TextField()
            {
                multiline = true,
                value = "<Character Dialogue Area>",
                name = "characterDialogue"
            };
            node.mainContainer.Insert(2, characterDialogue);

            return node;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Test Category"), 1));
            tree.Add(new SearchTreeEntry(new GUIContent("Create Dialogue", icon)) {level = 2});

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.name == "Create Dialogue")
            {
                var node = CreateDialogueNode(string.Empty, 0, 0);

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
                        if (((TextField) element).name == "characterName")
                        {
                            dialogGraphNode.m_speakingCharacterName = ((TextField) element).value;
                        }
                        else if (((TextField) element).name == "characterDialogue")
                        {
                            dialogGraphNode.m_characterDialogue = ((TextField)element).value;
                        }
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
            SerializableDialogData dialogData = AssetDatabase.LoadAssetAtPath<SerializableDialogData>(inputPath);
            if (dialogData == null)
            {
                return false;
            }

            // create the nodes
            List<DialogueNode> createdNodes = new List<DialogueNode>();
            for (int i = 0; i < dialogData.m_dialogNodes.Count; ++i)
            {
                DialogueNode node = DeserializeNode(dialogData.m_dialogNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);
            }

            //connect the nodes
            for (int i = 0; i < dialogData.m_dialogNodes.Count; ++i)
            {
                for (int iEdge = 0; iEdge < dialogData.m_dialogNodes[i].m_outputs.Count; ++iEdge)
                {
                    SerializableDialogData.SerializableDialogEdge edge = dialogData.m_dialogNodes[i].m_outputs[iEdge];
                    Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort] as Port;
                    Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                    m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
                }
            }

            return true;
        }
    }
}
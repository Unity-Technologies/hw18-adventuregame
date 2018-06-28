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
                DialogueNode node = CreateStartNode();
                m_GraphView.AddElement(node);
            }
            m_GraphView.graphViewChanged += OnGraphViewChanged;
            m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
        }

        private DialogueNode CreateStartNode()
        {
            var node = CreateDialogueNode("START", 0, 0);
            m_GraphView.AddElement(node);
            node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));
            node.addOutput();

            node.capabilities &= ~(Capabilities.Movable | Capabilities.Deletable);
            node.style.backgroundColor = Color.green;
            node.SetPosition(new Rect(new Vector2(-50, -50), new Vector2(100, 100)));

            return node;
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

        DialogueNode DeserializeNode(SerializableDialogData.SerializableDialogNode graphNode)
        {
            DialogueNode node = CreateDialogueNode(graphNode.m_title, 0, 0);
            Rect temp = new Rect(graphNode.m_position, new Vector2(1, 1));
            node.SetPosition(temp);
            return node;
        }

        protected DialogueNode CreateDialogueNode(string title, int inNodes, int outNodes)
        {
            DialogueNode node = new DialogueNode();
            node.style.backgroundColor = Color.magenta;
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
            tree.Add(new SearchTreeEntry(new GUIContent("Create Dialogue End", icon)) { level = 2 });

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.name == "Create Dialogue")
            {
                var node = CreateDialogueNode(string.Empty, 0, 0);

                m_GraphView.AddElement(node);
                node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));

                Vector2 pointInWindow = context.screenMousePosition - position.position;
                Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

                node.SetPosition(new Rect(pointInGraph,
                    Vector2.zero)); // it's ok to pass zero here because width/height is dynamic

                node.Select(m_GraphView, false);
                return true;
            }     

            if (entry.name == "Create Dialogue End")
            {
                var node = CreateDialogueNode("END", 0, 0);
                m_GraphView.AddElement(node);
                node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));
                node.style.backgroundColor = Color.magenta;
                node.addInput();

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
                
                    dialogGraphNode.inputNodeCount = currentNode.inputContainer.childCount;
                    dialogGraphNode.outputNodeCount = currentNode.outputContainer.childCount;

                dialogGraphNode.m_title = currentNode.title;
                dialogGraphNode.m_position = currentNode.GetPosition().position;
                dialogGraphNode.m_outputs = new List<SerializableDialogData.SerializableDialogEdge>();
                dialogGraphNode.m_outputDialogs = new List<string>();
                dialogGraphNode.m_nodeColor = currentNode.style.backgroundColor;
                    
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
                    if (element.childCount > 1 && element[0] is TextField)
                    {

                        outPutString = ((TextField) element[0]).value;
                    }

                    if (element.childCount > 1 && element[1] is Port)
                    {
                        foreach (Edge edge in ((Port) element[1]).connections)
                        {
                            SerializableDialogData.SerializableDialogEdge serializedEdge =
                                new SerializableDialogData.SerializableDialogEdge();

                            var sourceContainer = currentNode.outputContainer.First(e => e[1] == edge.output);
                            serializedEdge.m_sourcePort = sourceContainer == null ? -1 : currentNode.outputContainer.IndexOf(sourceContainer);
                            serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);
                            serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                            if (serializedEdge.m_sourcePort == -1 || serializedEdge.m_targetPort == -1)
                            {
                                continue;
                            }

                            dialogGraphNode.m_outputDialogs.Add(outPutString);
                            if (outPutString != string.Empty)
                            {
                                outPutString = string.Empty;
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
                int index = 0;
                
                DialogueNode node = DeserializeNode(dialogData.m_dialogNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);
                node.title = dialogData.m_dialogNodes[i].m_title;
                node.style.backgroundColor = dialogData.m_dialogNodes[i].m_nodeColor;

                for (int j = 0; j < dialogData.m_dialogNodes[i].inputNodeCount; j++)
                {
                    node.addInput();
                }

                for (int j = 0; j < dialogData.m_dialogNodes[i].outputNodeCount; j++)
                {
                    node.addOutput();                
                }

                foreach (VisualElement element in node.outputContainer)
                {
                    if (element.childCount > 1 && element[0] is TextField)
                    {
                        ((TextField) element[0]).value =
                            dialogData.m_dialogNodes[i].m_outputDialogs[index++];
                    }
                }

                foreach (VisualElement element in node.mainContainer)
                {
                    if (element is TextField)
                    {
                        if (((TextField)element).name == "characterName")
                        {
                            ((TextField) element).value = dialogData.m_dialogNodes[i].m_speakingCharacterName;
                        }
                        else if (((TextField)element).name == "characterDialogue")
                        {
                            ((TextField)element).value = dialogData.m_dialogNodes[i].m_characterDialogue;
                        }
                    }
                }
            }

            //connect the nodes
            for (int i = 0; i < dialogData.m_dialogNodes.Count; ++i)
            {
                for (int iEdge = 0; iEdge < dialogData.m_dialogNodes[i].m_outputs.Count; ++iEdge)
                {
                    SerializableDialogData.SerializableDialogEdge edge = dialogData.m_dialogNodes[i].m_outputs[iEdge];
                    Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort][1] as Port;
                    Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                    m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
                }
            }

            return true;
        }
    }
}
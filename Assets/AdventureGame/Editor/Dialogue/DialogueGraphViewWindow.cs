using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.AdventureGame;
using UnityEngine.Experimental.UIElements;

namespace Unity.Adventuregame {
    public class DialogueGraphViewWindow : EditorWindow, ISearchWindowProvider
    {
        protected SampleGraphView m_GraphView;
        SerializableDialogData m_DialogData;
        Label m_NoSelectLabel;

        [OnOpenAsset(1)]
        public static bool OpenGameLogicFromAsset(int instanceID, int line)
        {
            SerializableDialogData data = EditorUtility.InstanceIDToObject(instanceID) as SerializableDialogData;
            OpenWindow(data);
            return data != null; // we did not handle the open
        }

        [MenuItem("Adventure Game/Dialogue Window &g")]
        public static void OpenWindow()
        {
            GetWindow<DialogueGraphViewWindow>("Dialogue", true, typeof(SceneView));
        }

        public static void OpenWindow(SerializableDialogData data)
        {
            if (data != null)
            {
                DialogueGraphViewWindow view = GetWindow<DialogueGraphViewWindow>("Dialogue", true, typeof(SceneView));
                view.ShowScript(data);
            }
        }

        public void ShowScript(SerializableDialogData data)
        {
            m_DialogData = data;

            if (!LoadGraphData())
            {
                DialogueNode node = CreateStartNode();
                node.addOutput();
                m_GraphView.AddElement(node);
            }
        }

        // Use this for initialization
        void OnEnable()
        {
            var sampleGraphView = new SampleGraphView();
            m_GraphView = sampleGraphView;

            m_GraphView.name = "theView";
            m_GraphView.persistenceKey = "theView";
            m_GraphView.StretchToParentSize();

            m_NoSelectLabel = new Label("Select a Dialogue Asset in order to view");
            m_NoSelectLabel.style.textAlignment = TextAnchor.MiddleCenter;
            m_NoSelectLabel.StretchToParentSize();
            m_NoSelectLabel.visible = false;

            this.GetRootVisualContainer().Add(m_GraphView);
            this.GetRootVisualContainer().Add(m_NoSelectLabel);
            OnSelectionChanged();

            if (!LoadGraphData())
            {
                DialogueNode node = CreateStartNode();
                node.addOutput();
                m_GraphView.AddElement(node);
            }
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
            SaveGraphData();

            ClearGraphData();

            SerializableDialogData[] data = Selection.GetFiltered<SerializableDialogData>(SelectionMode.Assets);
            if (data.Length != 1)
            {
                m_DialogData = null;
                return;
            }
            m_DialogData = data[0];

            if (!LoadGraphData())
            {
                DialogueNode node = CreateStartNode();
                node.addOutput();
                m_GraphView.AddElement(node);
            }
        }

        private DialogueNode CreateStartNode()
        {
            var node = CreateDialogueNode("START", 0, 0);

            node.mainContainer.style.backgroundColor = Color.green;
            m_GraphView.AddElement(node);
            node.SetPosition(new Rect(new Vector2(10, 100), Vector2.zero));

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
            SaveGraphData();
        }

        DialogueNode DeserializeNode(SerializableDialogData.SerializableDialogNode graphNode)
        {
            DialogueNode node = graphNode.m_title == "START" ? CreateStartNode() : CreateDialogueNode(graphNode.m_title, 0, 0);
            Rect temp = new Rect(graphNode.m_position, new Vector2(1, 1));
            node.SetPosition(temp);
            return node;
        }

        protected DialogueNode CreateDialogueNode(string title, int inNodes, int outNodes)
        {
            DialogueNode node = new DialogueNode();
            node.Initialize(this);
            node.mainContainer.style.backgroundColor = Color.magenta;
            node.title = title;

            var characterName = new TextField()
            {
                multiline = false,
                value = "<CharacterName Here>",
                name = "characterName",
                isDelayed = true
            };
            characterName.OnValueChanged(val => SaveGraphData());
            node.mainContainer.Insert(1, characterName);

            var characterDialogue = new TextField()
            {
                multiline = true,
                value = "<Character Dialogue Area>",
                name = "characterDialogue",
                isDelayed = true
            };
            characterDialogue.OnValueChanged(val => SaveGraphData());
            node.mainContainer.Insert(2, characterDialogue);

            return node;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
            tree.Add(new SearchTreeEntry(new GUIContent("Create Dialogue", icon)) {level = 1});
            tree.Add(new SearchTreeEntry(new GUIContent("Create Dialogue End", icon)) { level = 1 });

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
                SaveGraphData();
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
                SaveGraphData();
                return true;
            }
            return OnSelectEntry(entry, context);
        }

        public void SaveGraphData()
        {
            if (m_DialogData == null)
        {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(m_DialogData);

            List<Node> nodes = m_GraphView.nodes.ToList();

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
                        dialogGraphNode.m_outputDialogs.Add(outPutString);
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

                            dialogGraphNode.m_outputs.Add(serializedEdge);
                        }
                    }
                }

                dialogGraphData.m_dialogNodes.Add(dialogGraphNode);
            }

            EditorUtility.CopySerialized(dialogGraphData, m_DialogData);
            AssetDatabase.SaveAssets();
        }

        void ClearGraphData()
        {
            m_NoSelectLabel.visible = true;
            m_GraphView.visible = false;

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
        }

        public virtual bool LoadGraphData()
        {
            ClearGraphData();
            m_NoSelectLabel.visible = false;
            m_GraphView.visible = true;

            if (m_DialogData == null || m_DialogData.m_dialogNodes.Count == 0)
            {
                return false;
            }

            // create the nodes
            List<DialogueNode> createdNodes = new List<DialogueNode>();
            for (int i = 0; i < m_DialogData.m_dialogNodes.Count; ++i)
            {
                int index = 0;
                
                DialogueNode node = DeserializeNode(m_DialogData.m_dialogNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);
                node.title = m_DialogData.m_dialogNodes[i].m_title;
                node.style.backgroundColor = m_DialogData.m_dialogNodes[i].m_nodeColor;

                for (int j = 0; j < m_DialogData.m_dialogNodes[i].inputNodeCount; j++)
                {
                    node.addInput();
                }

                for (int j = 0; j < m_DialogData.m_dialogNodes[i].outputNodeCount; j++)
                {
                    node.addOutput();                
                }

                foreach (VisualElement element in node.outputContainer)
                {
                    if (element.childCount > 1 && element[0] is TextField)
                    {
                        ((TextField) element[0]).value =
                            m_DialogData.m_dialogNodes[i].m_outputDialogs[index++];
                    }
                }

                foreach (VisualElement element in node.mainContainer)
                {
                    if (element is TextField)
                    {
                        if (((TextField)element).name == "characterName")
                        {
                            ((TextField) element).value = m_DialogData.m_dialogNodes[i].m_speakingCharacterName;
                        }
                        else if (((TextField)element).name == "characterDialogue")
                        {
                            ((TextField)element).value = m_DialogData.m_dialogNodes[i].m_characterDialogue;
                        }
                    }
                }
            }

            //connect the nodes
            for (int i = 0; i < m_DialogData.m_dialogNodes.Count; ++i)
            {
                for (int iEdge = 0; iEdge < m_DialogData.m_dialogNodes[i].m_outputs.Count; ++iEdge)
                {
                    SerializableDialogData.SerializableDialogEdge edge = m_DialogData.m_dialogNodes[i].m_outputs[iEdge];
                    Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort][1] as Port;
                    Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                    m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
                }
            }

            return true;
        }
    }
}
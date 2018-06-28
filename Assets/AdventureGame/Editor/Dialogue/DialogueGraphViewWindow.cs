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

            LoadGraphData();
        }

        // Use this for initialization
        void OnEnable()
        {
            var sampleGraphView = new SampleGraphView();
            m_GraphView = sampleGraphView;

            m_GraphView.name = "theView";
            m_GraphView.persistenceKey = "theView";
            m_GraphView.StretchToParentSize();

            this.GetRootVisualContainer().Add(m_GraphView);
            
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

            SerializableDialogData[] data = Selection.GetFiltered<SerializableDialogData>(SelectionMode.Assets);
            if (data.Length != 1)
            {
                m_DialogData = null;
                return;
            }
            m_DialogData = data[0];

            LoadGraphData();
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
            DialogueNode node = CreateDialogueNode(graphNode.m_title, 0, 0);
            Rect temp = new Rect(graphNode.m_position, new Vector2(1, 1));
            node.SetPosition(temp);
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

                            serializedEdge.m_outputDialog = outPutString;
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

            AssetDatabase.CreateAsset(dialogGraphData, assetPath);
            AssetDatabase.SaveAssets();
        }

        public virtual bool LoadGraphData()
        {
            if (m_DialogData == null || m_DialogData.m_dialogNodes.Count == 0)
            {
                return false;
            }

            // create the nodes
            List<DialogueNode> createdNodes = new List<DialogueNode>();
            for (int i = 0; i < m_DialogData.m_dialogNodes.Count; ++i)
            {
                DialogueNode node = DeserializeNode(m_DialogData.m_dialogNodes[i]);
                createdNodes.Add(node);
                m_GraphView.AddElement(node);

                for (int j = 0; j < m_DialogData.m_dialogNodes[i].inputNodeCount; j++)
                {
                    node.addInput();
                }

                for (int j = 0; j < m_DialogData.m_dialogNodes[i].outputNodeCount; j++)
                {
                    node.addOutput();
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
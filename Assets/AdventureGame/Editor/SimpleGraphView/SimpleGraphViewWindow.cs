using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class SimpleGraphViewWindow : EditorWindow
{
    class MyDataPort
    {
        public int data;
    }

    SimpleGraphView graphView;

    [MenuItem("SimpleGraphView/Open Window")]
    public static void OpenWindow()
    {
        GetWindow<SimpleGraphViewWindow>();
    }

	// Use this for initialization
	void OnEnable()
	{
        var simpleGraphView = new SimpleGraphView();
        graphView = simpleGraphView;

        graphView.name = "theView";
        graphView.persistenceKey = "theView";
        graphView.StretchToParentSize();

	    graphView.graphViewChanged = GraphViewChanged;

        this.GetRootVisualContainer().Add(graphView);

	    CreateNode(8, new Vector2(10, 100));
        CreateNode(12, new Vector2(100, 20));
	}

    public GraphViewChange GraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                Debug.Log("input: " +
                    (edge.input.userData as MyDataPort).data +
                    " output: " +
                    (edge.output.userData as MyDataPort).data);
            }
        }

        return change;
    }

    void CreateNode(int num, Vector2 pos)
    {
        Node node = new Node();
        node.title = "hello";
        node.capabilities |= Capabilities.Movable;
        graphView.AddElement(node);
        node.SetPosition(new Rect(pos, Vector2.zero));

        var dataPort = new MyDataPort() { data = num + 4 };
        var dataPort1 = new MyDataPort() { data = num + 8 };

        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        inputPort.portName = "in port";
        inputPort.userData = dataPort;
        
        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "out port";
        outputPort.userData = dataPort1;

        node.inputContainer.Add(inputPort);
        node.outputContainer.Add(outputPort);
    }

    // Update is called once per frame
    void OnDisable()
	{
		
	}
}

using System.Text;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace Unity.Adventuregame {
    public class DialogueNode : Node {
        void AddNoteMenuItems(ContextualMenu menu)
        {
            menu.AppendAction("Add input", (a) => addInput(), ContextualMenu.MenuAction.AlwaysEnabled);
            menu.AppendAction("Add output", (a) => addOutput(), ContextualMenu.MenuAction.AlwaysEnabled);
        }

        public void addOutput()
        {
            style.backgroundColor = new Color(0, 0, 0, 0);
            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            outputPort.portName = string.Empty;
            VisualElement test = new VisualElement();
            test.style.flexDirection = FlexDirection.Row;

            TextField outText = new TextField
            {
                multiline = true
            };
            outText.style.flexGrow = 1;
            test.Add(outText);
            test.Add(outputPort);
            outputContainer.Add(test);
        }

        public void addInput()
        {
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPort.portName = string.Empty;
            inputContainer.Add(inputPort);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is DialogueNode)
            {
                AddNoteMenuItems(evt.menu);
            }

            base.BuildContextualMenu(evt);
        }
    }
}
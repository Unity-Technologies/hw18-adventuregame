using System.Text;
using UnityEditor.Experimental.UIElements.GraphView;
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

        void addOutput()
        {
            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            outputPort.portName = string.Empty;
            outputContainer.Add(outputPort);

            TextField outText = new TextField
            {
                multiline = true
            };
            outputContainer.Add(outText);
            outText.PlaceBehind(outputPort);

        }

        void addInput()
        {
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(string));
            inputPort.portName = string.Empty;
            inputContainer.Add(inputPort);

            TextField inText = new TextField
            {
                multiline = true
            };
            inputContainer.Add(inText);
            inText.PlaceBehind(inputPort);
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
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace Unity.Adventuregame {
    public class DialogueNode : Node {
        void AddNoteMenuItems(ContextualMenu menu)
        {
            menu.AppendAction("Add input", (a) => addInput(), ContextualMenu.MenuAction.AlwaysEnabled);
            menu.AppendAction("Add output", (a) => addOutput(), ContextualMenu.MenuAction.AlwaysEnabled);
        }

        void addOutput()
        {
            Port output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputContainer.Add(output);
        }

        void addInput()
        {
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
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
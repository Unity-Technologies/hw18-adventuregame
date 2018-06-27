using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.AdventureGame
{
    public class GameLogicGraphView : GraphView
    {
        public GameLogicGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // FIXME: add a coordinator so that ContentDragger and SelectionDragger cannot be active at the same time.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            Insert(0, new GridBackground());

            focusIndex = 0;
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(nap =>
                    nap.direction != startPort.direction &&
                    nap.node != startPort.node)
                .ToList();
        }
    }
}
using System.Diagnostics;

namespace Aardwolf
{
    internal class pathNode
    {
        public readonly int heightPosition;
        public readonly int widthPosition;

        private Dictionary<pathNode, int> connectedNodes;

        public void connectNode (ref pathNode node)
        {
            // Calculate the distance between this node and the connecting node.
            int distance = Math.Abs(heightPosition - node.heightPosition) + Math.Abs(widthPosition - node.widthPosition);

            // Add the connecting node to the list of connected nodes.
            connectedNodes.Add(node, distance);

            Debug.WriteLine("Node at " + heightPosition + ", " + widthPosition + " connected to node at " + node.heightPosition + ", " + node.widthPosition + " with distance " + distance);
        }
        public pathNode (int heightPosition, int widthPosition)
        {
            this.heightPosition = heightPosition;
            this.widthPosition = widthPosition;

            connectedNodes = new Dictionary<pathNode, int>();

            Debug.WriteLine("Node created at " + heightPosition + ", " + widthPosition);
        }
    }

    internal class pathfinder
    {
        private maphandler _mapdata;
        private List<pathNode> _nodes;

        public bool tileNodeWorthy(int heightPosition, int widthPosition)
        {
            // We're checking to see if a tile is node worthy. To make the check it first needs to be a floor tile.
            if (_mapdata.getTileData(heightPosition, widthPosition) != 0)
                return false;

            // We will run a sanitation check to ensure that the tile is not already a node.
            foreach (pathNode node in _nodes)
            {
                if (node.heightPosition == heightPosition && node.widthPosition == widthPosition)
                    return false;
            }

            // The tile needs to be blocked on at least one of it's NE/SE/SW/NW sides.
            bool cornerBlocked = false;
            if (_mapdata.getTileData(heightPosition - 1, widthPosition - 1) != 0)
                cornerBlocked = true;
            if (_mapdata.getTileData(heightPosition - 1, widthPosition + 1) != 0)
                cornerBlocked = true;
            if (_mapdata.getTileData(heightPosition + 1, widthPosition + 1) != 0)
                cornerBlocked = true;
            if (_mapdata.getTileData(heightPosition + 1, widthPosition - 1) != 0)
                cornerBlocked = true;

            // Now do the same check but for blocking objects, providing that the corner is not blocked.
            if (!cornerBlocked)
            {
                if (_mapdata.isTileBlocked(heightPosition - 1, widthPosition - 1) == true)
                    cornerBlocked = true;
                if (_mapdata.isTileBlocked(heightPosition - 1, widthPosition + 1) == true)
                    cornerBlocked = true;
                if (_mapdata.isTileBlocked(heightPosition + 1, widthPosition + 1) == true)
                    cornerBlocked = true;
                if (_mapdata.isTileBlocked(heightPosition + 1, widthPosition - 1) == true)
                    cornerBlocked = true;
            }

            if (!cornerBlocked)
                return false;

            // Now it also needs to blocked on A N/S side and a E/W side.
            bool sideBlocked = false;
            if (_mapdata.getTileData(heightPosition - 1, widthPosition) != 0 || _mapdata.isTileBlocked(heightPosition - 1, widthPosition))
                sideBlocked = !sideBlocked;
            if (_mapdata.getTileData(heightPosition + 1, widthPosition) != 0 || _mapdata.isTileBlocked(heightPosition + 1, widthPosition))
                sideBlocked = !sideBlocked;

            bool sideBlocked2 = false;

            if (_mapdata.getTileData(heightPosition, widthPosition - 1) != 0 || _mapdata.isTileBlocked(heightPosition, widthPosition - 1))
                sideBlocked2 = !sideBlocked2;
            if (_mapdata.getTileData(heightPosition, widthPosition + 1) != 0 || _mapdata.isTileBlocked(heightPosition, widthPosition + 1))
                sideBlocked2 = !sideBlocked2;

            if (!sideBlocked || !sideBlocked2)
                return false;

            return true;
        }

        public void preparePathFinder()
        {
            
        }
        public pathfinder (ref maphandler mapdata)
        {
            _mapdata = mapdata;
            _nodes = new List<pathNode>();
        }
    }
}

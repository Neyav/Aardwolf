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

        private bool tileBlocked(int heightPosition, int widthPosition)
        {
            if (_mapdata.getTileData(heightPosition, widthPosition) != 0)
                return true;

            if (_mapdata.isTileBlocked(heightPosition, widthPosition))
                return true;

            return false;
        }

        // Returns a unique code for the blocked status of the corners of a tile.
        private int cornerBlocked(int heightPosition, int widthPosition)
        {
            int code = 0;
            // Check NE corner.
            if (tileBlocked(heightPosition - 1, widthPosition - 1))
                code = code + 1;
            // Check SE corner.
            if (tileBlocked(heightPosition + 1, widthPosition - 1))
                code = code + 10;
            // Check SW corner.
            if (tileBlocked(heightPosition + 1, widthPosition + 1))
                code = code + 100;
            // Check NW corner.
            if (tileBlocked(heightPosition - 1, widthPosition + 1))
                code = code + 1000;

            return code;
        }
        public bool tileNodeWorthy(int heightPosition, int widthPosition)
        {
            int cornerBlockedcode = 0;

            // If the tile is blocked it cannot be a node.
            if (tileBlocked(heightPosition, widthPosition))
                return false;

            // If the tile is already a node it cannot be a node.
            foreach (pathNode node in _nodes)
            {
                if (node.heightPosition == heightPosition && node.widthPosition == widthPosition)
                    return false;
            }

            // If the tile is not blocked on at least one of it's NE/SE/SW/NW sides it cannot be a node.
            cornerBlockedcode = cornerBlocked(heightPosition, widthPosition);

            if (cornerBlockedcode == 0)
                return false;

            // If two tiles adjacent to this tile have the same cornerBlocked code, this tile cannot be a node.
            // Check E/W
            if (cornerBlocked(heightPosition, widthPosition - 1) == cornerBlockedcode && cornerBlocked(heightPosition, widthPosition + 1) == cornerBlockedcode)
                return false;

            // Check N/S
            if (cornerBlocked(heightPosition - 1, widthPosition) == cornerBlockedcode && cornerBlocked(heightPosition + 1, widthPosition) == cornerBlockedcode)
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

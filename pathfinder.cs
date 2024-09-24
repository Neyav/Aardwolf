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

    internal class pathfinderFloor
    {
        private int[][] _floortiles;
        private bool _tileGenerated;
        private maphandler _mapdata;
        private List<pathNode> _nodes;


        private bool tileBlocked(int heightPosition, int widthPosition)
        {
            if (heightPosition < 0 || heightPosition >= _mapdata.getMapHeight() || widthPosition < 0 || widthPosition >= _mapdata.getMapWidth())
                return true;

            if (_tileGenerated)
                if (_floortiles[heightPosition][widthPosition] == 0)
                    return true;

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
            {
                if (!(tileBlocked(heightPosition - 1, widthPosition) || tileBlocked(heightPosition, widthPosition - 1)))
                    code = code + 1;
            }
            // Check SE corner.
            if (tileBlocked(heightPosition + 1, widthPosition - 1))
                if (!(tileBlocked(heightPosition + 1, widthPosition) || tileBlocked(heightPosition, widthPosition - 1)))
                    code = code + 10;
            // Check SW corner.
            if (tileBlocked(heightPosition + 1, widthPosition + 1))
                if (!(tileBlocked(heightPosition + 1, widthPosition) || tileBlocked(heightPosition, widthPosition + 1)))
                    code = code + 100;
            // Check NW corner.
            if (tileBlocked(heightPosition - 1, widthPosition + 1))
                if (!(tileBlocked(heightPosition - 1, widthPosition) || tileBlocked(heightPosition, widthPosition + 1)))
                    code = code + 1000;

            return code;
        }
        private bool tileNodeWorthy(int heightPosition, int widthPosition)
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

            return true;
        }
        public pathNode returnNode(int heightPosition, int widthPosition)
        {
            foreach (pathNode node in _nodes)
            {
                if (node.heightPosition == heightPosition && node.widthPosition == widthPosition)
                    return node;
            }

            return null;
        }
        public void generateFloorNodes()
        {
            // Anything that might be node worthy gets a node.
            for (int heightPosition = 0; heightPosition < _mapdata.getMapHeight(); heightPosition++)
            {
                for (int widthPosition = 0; widthPosition < _mapdata.getMapWidth(); widthPosition++)
                {
                    if (tileNodeWorthy(heightPosition, widthPosition))
                    {
                        _nodes.Add(new pathNode(heightPosition, widthPosition));
                    }
                }
            }
        }

        public pathfinderFloor (ref maphandler mapdata)
        {
            _mapdata = mapdata;
            _tileGenerated = false;
            _nodes = new List<pathNode>();

            _floortiles = new int[_mapdata.getMapHeight()][];
            for (int i = 0; i < _mapdata.getMapHeight(); i++)
            {
                _floortiles[i] = new int[_mapdata.getMapWidth()];

                for (int j = 0; j < _mapdata.getMapWidth(); j++)
                {
                    if (tileBlocked(i,j))
                        _floortiles[i][j] = 0;
                    else
                        _floortiles[i][j] = 1;
                }
            }

            _tileGenerated = true;
        }
    }

    internal class pathfinder
    {
        private maphandler _mapdata;
        private List<pathfinderFloor> _pathfinderFloors;

        public pathNode returnNode(int heightPosition, int widthPosition)
        {
            return _pathfinderFloors[0].returnNode(heightPosition, widthPosition);
        }

        public void preparePathFinder()
        {
            // Add the base floor.
            _pathfinderFloors.Add(new pathfinderFloor(ref _mapdata));
            _pathfinderFloors[0].generateFloorNodes();
        }
        public pathfinder (ref maphandler mapdata)
        {
            _mapdata = mapdata;
            _pathfinderFloors = new List<pathfinderFloor>();
        }
    }
}

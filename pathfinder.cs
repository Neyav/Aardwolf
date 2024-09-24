using System.Diagnostics;

namespace Aardwolf
{
    internal class pathNode
    {
        public readonly int heightPosition;
        public readonly int widthPosition;

        public bool traveled;
        public float travelDistance;
        pathNode traveledNode;

        private Dictionary<pathNode, float> _connectedNodes;
        private Dictionary<pathNode, int> _nodeBlockStatus;

        public void connectNode(pathNode node, int blockStatus, float distance)
        {
            // Validate that these nodes are not already connected.
            if (_connectedNodes.ContainsKey(node))
                return;

            _connectedNodes.Add(node, distance);            
            _nodeBlockStatus.Add(node, blockStatus);
        }
        public void calculateAndConnectNode (pathNode node, int blockedStatus)
        {
            // Calculate the distance between this node and the connecting node.            
            float distance = (float)Math.Sqrt(Math.Pow(heightPosition - node.heightPosition, 2) + Math.Pow(widthPosition - node.widthPosition, 2));

            // Add the connecting node to the list of connected nodes.
            connectNode(this, blockedStatus, distance);
            connectNode(node, blockedStatus, distance);

            Debug.WriteLine("Node at " + heightPosition + ", " + widthPosition + " connected to node at " + node.heightPosition + ", " + node.widthPosition + " with distance " + distance);
        }

        public List<pathNode> returnConnectedNodes()
        {
            return _connectedNodes.Keys.ToList();
        }
        public pathNode (int heightPosition, int widthPosition)
        {
            this.heightPosition = heightPosition;
            this.widthPosition = widthPosition;

            _connectedNodes = new Dictionary<pathNode, float>();
            _nodeBlockStatus = new Dictionary<pathNode, int>();

            travelDistance = -1;
            traveled = false;
            traveledNode = null;

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

        private int nodeConnectObstruction(pathNode startNode, pathNode endNode)
        {
            int dx = Math.Abs(startNode.heightPosition - endNode.heightPosition);
            int dy = Math.Abs(startNode.widthPosition - endNode.widthPosition);
            int sx = startNode.heightPosition < endNode.heightPosition ? 1 : -1;
            int sy = startNode.widthPosition < endNode.widthPosition ? 1 : -1;
            int err = dx - dy;
            int moveX = startNode.heightPosition;
            int moveY = startNode.widthPosition;
            int keyRequired = 0;

            while (true)
            {
                if (tileBlocked(moveX, moveY))
                    return 1;

                if (moveX == endNode.heightPosition && moveY == endNode.widthPosition)
                    break;

                if (_mapdata.isDoorOpenable(moveX, moveY, true, false))
                    keyRequired = 2;  // Gold key required for this path
                else if (_mapdata.isDoorOpenable(moveX, moveY, false, true))
                    keyRequired = 3;  // Silver key required for this path

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    moveX = moveX + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    moveY = moveY + sy;
                }
            }

            return keyRequired;
        }

        private void connectNodes()
        {
            // Go through the list of nodes.
            foreach (pathNode basenode in _nodes)
            {
                foreach (pathNode trynode in _nodes)
                {                    
                    // Don't test the same node against itself.
                    if (basenode == trynode)
                        continue;

                    // We need to figure out if the nodes are connected without obstructions.
                    int blockStatus = nodeConnectObstruction(basenode, trynode);

                    if (blockStatus != 1)
                    {
                        basenode.calculateAndConnectNode(trynode, blockStatus);
                    }
                }
            }
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

            // Add the player spawn point as a node.
            _nodes.Add(new pathNode(_mapdata.playerSpawnHeight, _mapdata.playerSpawnWidth));

            connectNodes();
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

        public List<pathNode> returnConnectedNodes(int heightPosition, int widthPosition)
        {
            return _pathfinderFloors[0].returnNode(heightPosition, widthPosition).returnConnectedNodes();
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

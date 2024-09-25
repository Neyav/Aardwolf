using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Aardwolf
{

    enum nodeStatus
    {
        none = 0,
        goldKey = 1,
        silverKey = 2,
        bothKeys = 3
    };

    internal class pathNode
    {
        public readonly int heightPosition;
        public readonly int widthPosition;

        public bool startPoint;
        public bool endPoint;

        public nodeStatus importantNodeStatus;
        public pathfinderFloor floor;

        public bool traveled;
        public float travelDistance;
        public pathNode traveledNode;

        private Dictionary<pathNode, float> _connectedNodes;
        private Dictionary<pathNode, int> _nodeBlockStatus;

        public float returnDistance(pathNode node)
        {
            return _connectedNodes[node];
        }

        public int returnBlockStatus(pathNode node)
        {
            return _nodeBlockStatus[node];
        }

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

            //Debug.WriteLine("Node at " + heightPosition + ", " + widthPosition + " connected to node at " + node.heightPosition + ", " + node.widthPosition + " with distance " + distance);
        }

        public List<pathNode> returnConnectedNodes()
        {
            return _connectedNodes.Keys.ToList();
        }
        public pathNode (int heightPosition, int widthPosition, pathfinderFloor spawnFloor)
        {
            this.heightPosition = heightPosition;
            this.widthPosition = widthPosition;

            floor = spawnFloor;

            _connectedNodes = new Dictionary<pathNode, float>();
            _nodeBlockStatus = new Dictionary<pathNode, int>();

            importantNodeStatus = nodeStatus.none;

            travelDistance = -1;
            traveled = false;
            traveledNode = null;

            //Debug.WriteLine("Node created at " + heightPosition + ", " + widthPosition);
        }
    }

    internal class pathfinderFloor
    {
        private int[][] _floortiles;
        private bool _tileGenerated;
        private maphandler _mapdata;
        private List<pathNode> _nodes;

        private bool _ignorePushWalls;
        private bool _allSecretsTreasures;

        public bool tileBlocked(int heightPosition, int widthPosition)
        {
            if (heightPosition < 0 || heightPosition >= _mapdata.getMapHeight() || widthPosition < 0 || widthPosition >= _mapdata.getMapWidth())
                return true;

            // If we have a reference tile map use it before testing the original map data.
            if (_tileGenerated)
                if (_floortiles[heightPosition][widthPosition] == 1)
                    return true;
                else
                    return false;

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

        public List<pathNode> returnUntraveledNodes()
        {
            List<pathNode> pathNodes = new List<pathNode>();

            foreach (pathNode node in _nodes)
            {
                if (!node.traveled && node.travelDistance >= 0)
                    pathNodes.Add(node);
            }

            return pathNodes;
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

                if (!_mapdata.isDoorOpenable(moveX, moveY, false, false))
                {
                    if (_mapdata.isDoorOpenable(moveX, moveY, true, false) && keyRequired != 2 && keyRequired != 5)
                        keyRequired += 2;  // Gold key required for this path
                    else if (_mapdata.isDoorOpenable(moveX, moveY, false, true) && keyRequired != 3 && keyRequired != 5)
                        keyRequired += 3;  // Silver key required for this path
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    moveX = moveX + sx;
                }

                if (tileBlocked(moveX, moveY))
                    return 1;

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

        private bool insertUniqueNode(pathNode pathNode)
        {
            // If this tile is blocked, don't add it.
            if (tileBlocked(pathNode.heightPosition, pathNode.widthPosition))
                return false;

            // If a node at this location exists, delete it.
            foreach (pathNode node in _nodes)
            {
                if (node.heightPosition == pathNode.heightPosition && node.widthPosition == pathNode.widthPosition)
                {
                    _nodes.Remove(node);
                    break;
                }
            }

            _nodes.Add(pathNode);

            return true;
        }

        public void overrideMapTile(int heightPosition, int widthPosition, int tileData)
        {
            _floortiles[heightPosition][widthPosition] = tileData;
        }

        public int returnMapTile(int heightPosition, int widthPosition)
        {
            return _floortiles[heightPosition][widthPosition];
        }

        public void generateFloorNodes(pathNode carryOverNode)
        {
            // Anything that might be node worthy gets a node.
            for (int heightPosition = 0; heightPosition < _mapdata.getMapHeight(); heightPosition++)
            {
                for (int widthPosition = 0; widthPosition < _mapdata.getMapWidth(); widthPosition++)
                {
                    if (tileNodeWorthy(heightPosition, widthPosition))
                    {
                        insertUniqueNode(new pathNode(heightPosition, widthPosition, this));
                    } // Are we beside an exit tile?
                    else if (_mapdata.isTileAnExit(heightPosition, widthPosition - 1) || _mapdata.isTileAnExit(heightPosition, widthPosition + 1))
                    {
                        if (!tileBlocked(heightPosition, widthPosition))
                        {
                            insertUniqueNode(new pathNode(heightPosition, widthPosition, this));

                            _nodes[_nodes.Count - 1].endPoint = true;
                        }
                    }

                    if (!_ignorePushWalls)
                    {
                        if (_mapdata.isTilePushable(heightPosition - 1, widthPosition))
                        {
                            insertUniqueNode(new pathNode(heightPosition, widthPosition, this));
                        }
                        else if (_mapdata.isTilePushable(heightPosition + 1, widthPosition))
                        {
                            insertUniqueNode(new pathNode(heightPosition, widthPosition, this));
                        }
                        else if (_mapdata.isTilePushable(heightPosition, widthPosition - 1))
                        {
                            insertUniqueNode(new pathNode(heightPosition, widthPosition, this));
                        }
                        else if (_mapdata.isTilePushable(heightPosition, widthPosition + 1))
                        {
                            insertUniqueNode(new pathNode(heightPosition, widthPosition, this));
                        }
                    }

                    if (_mapdata.getStaticObjectID(heightPosition, widthPosition) == 43)
                    {
                        if (insertUniqueNode(new pathNode(heightPosition, widthPosition, this)))
                            _nodes[_nodes.Count - 1].importantNodeStatus = nodeStatus.goldKey;
                    }
                    else if (_mapdata.getStaticObjectID(heightPosition, widthPosition) == 44)
                    {
                        if (insertUniqueNode(new pathNode(heightPosition, widthPosition, this)))
                            _nodes[_nodes.Count - 1].importantNodeStatus = nodeStatus.silverKey;
                    }
                }
            }

            if (carryOverNode != null)
            {
                insertUniqueNode(carryOverNode); // Copy this node to the new graph.
            }

            // Add the player spawn point as a node.
            if (insertUniqueNode(new pathNode(_mapdata.playerSpawnHeight, _mapdata.playerSpawnWidth, this)))
                _nodes[_nodes.Count - 1].startPoint = true;

            connectNodes();
        }

        public pathNode returnStartNode()
        {
            foreach (pathNode node in _nodes)
            {
                if (node.startPoint)
                    return node;
            }

            return null;
        }

        public pathfinderFloor (ref maphandler mapdata, pathfinderFloor sourceFloor, bool ignorePushWalls, bool allSecretsTreasures)
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
                    if (sourceFloor == null)
                    {
                        if (tileBlocked(i, j))
                            _floortiles[i][j] = 1;
                        else
                            _floortiles[i][j] = 0;
                    }
                    else
                    {
                        _floortiles[i][j] = sourceFloor.returnMapTile(i,j);
                    }
                }
            }

            _tileGenerated = true;

            _ignorePushWalls = ignorePushWalls;
            _allSecretsTreasures = allSecretsTreasures;
        }
    }

    internal class pathfinder
    {
        private maphandler _mapdata;
        private List<pathfinderFloor> _pathfinderFloors;
        private pathNode _startNode;
        private pathNode _endNode;

        public bool ignorePushWalls;
        public bool allSecretsTreasures;    

        public pathNode returnNode(int heightPosition, int widthPosition)
        {
            return _pathfinderFloors[_pathfinderFloors.Count() - 1].returnNode(heightPosition, widthPosition);
        }

        public List<pathNode> returnConnectedNodes(int heightPosition, int widthPosition)
        {
            return _pathfinderFloors[_pathfinderFloors.Count() - 1].returnNode(heightPosition, widthPosition).returnConnectedNodes();
        }

        public List<pathNode> returnRoute()
        {
            List<pathNode> route = new List<pathNode>();
            pathNode currentNode = _endNode;

            while (currentNode != null)
            {
                Debug.WriteLine("Route node at " + currentNode.heightPosition + ", " + currentNode.widthPosition);
                route.Add(currentNode);
                currentNode = currentNode.traveledNode;
            }

            // Reverse route.
            route.Reverse();

            return route;
        }

        public bool solveMaze()
        {
            pathNode currentNode = _startNode;
            _startNode.travelDistance = 0;            

            while (true)
            {
                currentNode.traveled = true;

                Debug.WriteLine("Traveling to " + currentNode.heightPosition + ", " + currentNode.widthPosition + " " + currentNode.travelDistance + " " + currentNode.importantNodeStatus);

                if (currentNode.endPoint)
                {
                    Debug.WriteLine("Exit found at " + currentNode.heightPosition + ", " + currentNode.widthPosition + " " + currentNode.travelDistance + " " + currentNode.importantNodeStatus);
                    _endNode = currentNode;
                    
                    return true;
                }

                // If this picks us up a key, we need  to move to a new floor.
                if (currentNode.importantNodeStatus != nodeStatus.none && currentNode.importantNodeStatus != currentNode.traveledNode.importantNodeStatus)
                {
                    if (currentNode.importantNodeStatus == nodeStatus.goldKey && currentNode.traveledNode.importantNodeStatus == nodeStatus.silverKey)
                        currentNode.importantNodeStatus = nodeStatus.bothKeys;
                    if (currentNode.importantNodeStatus == nodeStatus.silverKey && currentNode.traveledNode.importantNodeStatus == nodeStatus.goldKey)
                        currentNode.importantNodeStatus = nodeStatus.bothKeys;

                    pathfinderFloor newFloor = new pathfinderFloor(ref _mapdata, currentNode.floor, ignorePushWalls, allSecretsTreasures);
                    newFloor.generateFloorNodes(currentNode);
                    _pathfinderFloors.Add(newFloor);
                }

                if (!ignorePushWalls)
                {
                    int moveHeight = 0;
                    int moveWidth = 0;
                    int testHeight = currentNode.heightPosition;
                    int testWidth = currentNode.widthPosition;
                    bool moveable = false;

                    Debug.WriteLine("Checking for pushable tiles at " + currentNode.heightPosition + ", " + currentNode.widthPosition);

                    if (_mapdata.isTilePushable(currentNode.heightPosition - 1, currentNode.widthPosition) && currentNode.floor.tileBlocked(currentNode.heightPosition - 1, currentNode.widthPosition))
                    {
                        moveHeight = -1;
                    }
                    else if (_mapdata.isTilePushable(currentNode.heightPosition + 1, currentNode.widthPosition) && currentNode.floor.tileBlocked(currentNode.heightPosition + 1, currentNode.widthPosition))
                    {
                        moveHeight = 1;
                    }
                    else if (_mapdata.isTilePushable(currentNode.heightPosition, currentNode.widthPosition - 1) && currentNode.floor.tileBlocked(currentNode.heightPosition, currentNode.widthPosition - 1))
                    {
                        moveWidth = -1;
                    }
                    else if (_mapdata.isTilePushable(currentNode.heightPosition, currentNode.widthPosition + 1) && currentNode.floor.tileBlocked(currentNode.heightPosition, currentNode.widthPosition + 1))
                    {
                        moveWidth = 1;
                    }

                    if (moveHeight != 0 || moveWidth != 0)
                    {
                        testHeight = testHeight + moveHeight;
                        testWidth = testWidth + moveWidth;

                        Debug.WriteLine("Pushable tile at " + currentNode.heightPosition + ", " + currentNode.widthPosition + " " + testHeight + ", " + testWidth);

                        // See if we can move the pushable tile.
                        if (!currentNode.floor.tileBlocked(testHeight + moveHeight, testWidth + moveWidth))
                        {
                            testHeight = testHeight + moveHeight;
                            testWidth = testWidth + moveWidth;                            
                        }

                        // Can we move it two blocks?
                        if (!currentNode.floor.tileBlocked(testHeight + moveHeight, testWidth + moveWidth))
                        {
                            testHeight = testHeight + moveHeight;
                            testWidth = testWidth + moveWidth;
                            moveable = true;
                        }
                        
                        Debug.WriteLine("Pushable tile at " + currentNode.heightPosition + ", " + currentNode.widthPosition + " " + testHeight + ", " + testWidth);

                        if (moveable)
                        {
                            Debug.WriteLine("Tile pushed at " + currentNode.heightPosition + ", " + currentNode.widthPosition + " " + testHeight + ", " + testWidth + " " + moveHeight + ", " + moveWidth);

                            // We can move the tile, so generate a new floor and move it.
                            pathfinderFloor newFloor = new pathfinderFloor(ref _mapdata, currentNode.floor, ignorePushWalls, allSecretsTreasures);
                            newFloor.overrideMapTile(currentNode.heightPosition + moveHeight, currentNode.widthPosition + moveWidth, 0);
                            newFloor.overrideMapTile(testHeight, testWidth, 1);
                            newFloor.generateFloorNodes(currentNode);
                            _pathfinderFloors.Add(newFloor);
                        }

                    }

                }

                // Get the list of connected nodes.
                List<pathNode> connectedNodes = currentNode.returnConnectedNodes();

                foreach (pathNode node in connectedNodes)
                {
                    if (node.traveled)
                        continue;

                    if (node.travelDistance == -1 || node.travelDistance > currentNode.travelDistance + currentNode.returnDistance(node))
                    {
                        if (currentNode.importantNodeStatus != nodeStatus.bothKeys)
                        {
                            int nodeBlock = currentNode.returnBlockStatus(node);

                            if (nodeBlock == 5)
                                continue; // Route needs both keys, you don't have them.

                            // In both these cases the door you need is blocked by a key you don't have.
                            if (currentNode.importantNodeStatus != nodeStatus.goldKey && nodeBlock == 2)
                                continue;
                            if (currentNode.importantNodeStatus != nodeStatus.silverKey && nodeBlock == 3)
                                continue;
                        }                        

                        node.travelDistance = currentNode.travelDistance + currentNode.returnDistance(node);
                        node.traveledNode = currentNode;
                        if (node.importantNodeStatus == nodeStatus.none)
                            node.importantNodeStatus = currentNode.importantNodeStatus;
                    }
                }

                // Make a master node list of all nodes from all floors that haven't been traveled to yet..
                List<pathNode> allNodes = new List<pathNode>();
                foreach (pathfinderFloor floor in _pathfinderFloors)
                {
                    List<pathNode> aggrigate = floor.returnUntraveledNodes();
                    foreach (pathNode node in aggrigate)
                    {
                        allNodes.Add(node);
                    }
                }

                // Find the lowest travel distance node that hasn't been traveled, if there aren't any return false.
                pathNode lowestNode = null;
                foreach (pathNode node in allNodes)
                {
                    if (node.traveled)
                        continue;

                    if (node.travelDistance == -1)
                        continue;

                    if (lowestNode == null)
                        lowestNode = node;
                    else if (node.travelDistance < lowestNode.travelDistance)
                        lowestNode = node;
                }

                if (lowestNode == null)
                    return false;

                currentNode = lowestNode;
            }
        }
        
        public void preparePathFinder()
        {
            // Add the base floor.
            _pathfinderFloors.Add(new pathfinderFloor(ref _mapdata, null, ignorePushWalls, allSecretsTreasures));
            _pathfinderFloors[0].generateFloorNodes(null);
            _startNode = _pathfinderFloors[0].returnStartNode();

        }
        public pathfinder (ref maphandler mapdata)
        {
            _mapdata = mapdata;
            _pathfinderFloors = new List<pathfinderFloor>();
            _startNode = null;
            _endNode = null;

            allSecretsTreasures = false;
            ignorePushWalls = false;
        }
    }
}

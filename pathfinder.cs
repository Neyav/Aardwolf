using System.Diagnostics;

namespace Aardwolf
{
    internal class pathfindingGraph
    {
        public List<int[][]> graphMap;
        public List<int> floorMap;
        public List<int> HeightCeiling;
        public List<int> WidthCeiling;
        public List<int> HeightFloor;
        public List<int> WidthFloor;
        public List<bool> floorCleared;
        public List<bool[][]> visited;
        public List<int[][]> travelDistance;
        public List<int[][]> travelDirection;
        private int _Width;
        private int _Height;

        public void setGraphValue(int floor, int height, int width, int value)
        {
            graphMap[floor][height][width] = value;
        }

        public int getGraphValue(int floor, int height, int width)
        {
            return graphMap[floor][height][width];
        }

        public int addFloor(int baseFloor)
        {
            int[][] newFloor = new int[_Height][];

            for (int i = 0; i < _Height; i++)
            {
                newFloor[i] = new int[_Width];
            }

            // If we have a base floor, copy it over.
            if (baseFloor != -1)
            {
                for (int i = 0; i < _Height; i++)
                {
                    for (int j = 0; j < _Width; j++)
                    {
                        newFloor[i][j] = graphMap[baseFloor][i][j];
                    }
                }
            }

            graphMap.Add(newFloor);
            floorMap.Add(baseFloor); // Keep track of the floor we came from.
            floorCleared.Add(false); // Keep track of if the floor has been cleared or not.

            // Debug print out a floor was created and from where.
            Debug.WriteLine("Floor " + (graphMap.Count - 1) + " created from floor " + baseFloor);

            // Create a new blank visited template.
            bool[][] newVisited = new bool[_Height][];
            for (int i = 0; i < _Height; i++)
            {
                newVisited[i] = new bool[_Width];
                newVisited[i] = Enumerable.Repeat(false, _Width).ToArray();
            }

            visited.Add(newVisited);

            // Create a new blank travel distance template.
            int[][] newTravelDistance = new int[_Height][];
            for (int i = 0; i < _Height; i++)
            {
                newTravelDistance[i] = new int[_Width];
                newTravelDistance[i] = Enumerable.Repeat(-1, _Width).ToArray();
            }

            travelDistance.Add(newTravelDistance);

            // Create a new blank travel direction template.
            int[][] newTravelDirection = new int[_Height][];
            for (int i = 0; i < _Height; i++)
            {
                newTravelDirection[i] = new int[_Width];
                newTravelDirection[i] = Enumerable.Repeat(0, _Width).ToArray();
            }

            travelDirection.Add(newTravelDirection);

            return graphMap.Count - 1;
        }
        public pathfindingGraph(int graphWidth, int graphHeight)
        {
            _Width = graphWidth;
            _Height = graphHeight;

            // Initialize the graph with a single floor.
            graphMap = new List<int[][]>();
            floorMap = new List<int>();
            floorCleared = new List<bool>();
            visited = new List<bool[][]>();
            travelDistance = new List<int[][]>();
            travelDirection = new List<int[][]>();
            HeightCeiling = new List<int>();
            WidthCeiling = new List<int>();
            HeightFloor = new List<int>();
            WidthFloor = new List<int>();

            this.addFloor(-1);
        }
    }

    struct pathfindingNode
    {
        public int height;
        public int width;
        public int floor;

        public pathfindingNode(int height, int width, int floor)
        {
            this.height = height;
            this.width = width;
            this.floor = floor;
        }
    }

    internal class pathfinder
    {
        private maphandler _mapdata;        
        private pathfindingGraph _graph;
        private List<pathfindingNode> _path;
        int firstNodeWidth;
        int firstNodeHeight;
        bool mazeSolved;

        pathfindingNode findsmallestunexploredNode()
        {
            pathfindingNode smallestNode = new pathfindingNode(-1, -1, -1);

            // Find the smallest travel distance node that hasn't been visited yet across all floors.
            for (int i = 0; i < _graph.graphMap.Count; i++)
            {
                if (_graph.floorCleared[i] == true)
                {
                    continue;
                }
                bool floorCleared = true; // Assume the floor is cleared until proven otherwise.

                // If there is no heightfloor/heightceiling/etc for this floor, scan it quickly for nodes, and set the floor bounds.
                if (_graph.HeightFloor.Count < i + 1)
                {
                    _graph.HeightFloor.Add(_mapdata.getMapHeight());
                    _graph.WidthFloor.Add(_mapdata.getMapWidth());
                    _graph.HeightCeiling.Add(0);
                    _graph.WidthCeiling.Add(0);

                    for (int j = 0; j < _mapdata.getMapHeight(); j++)
                    {
                        for (int k = 0; k < _mapdata.getMapWidth(); k++)
                        {
                            if (_graph.travelDistance[i][j][k] > -1)
                            {
                                if (j <= _graph.HeightFloor[i])
                                {
                                    _graph.HeightFloor[i] = j - 1;
                                }
                                if (j >= _graph.HeightCeiling[i])
                                {
                                    _graph.HeightCeiling[i] = j + 1;
                                }
                                if (k <= _graph.WidthFloor[i])
                                {
                                    _graph.WidthFloor[i] = k - 1;
                                }
                                if (k >= _graph.WidthCeiling[i])
                                {
                                    _graph.WidthCeiling[i] = k + 1;
                                }
                            }
                        }
                    }
                }

                // Debug out heightfloor/ceiling/etc
                //Debug.WriteLine("Floor " + i + " HeightFloor: " + _graph.HeightFloor[i] + " HeightCeiling: " + _graph.HeightCeiling[i] + " WidthFloor: " + _graph.WidthFloor[i] + " WidthCeiling: " + _graph.WidthCeiling[i]);


                for (int j = _graph.HeightFloor[i]; j <= _graph.HeightCeiling[i]; j++)
                {
                    for (int k = _graph.WidthFloor[i]; k <=_graph.WidthCeiling[i]; k++)
                    {
                        if (_graph.visited[i][j][k] == false && _graph.travelDistance[i][j][k] != -1)
                        {
                            floorCleared = false;

                            // If this = the ceiling/floor bounds, we need to expand that bound.
                            if (j == _graph.HeightFloor[i])
                            {
                                _graph.HeightFloor[i] = j - 1;
                            }
                            if (j == _graph.HeightCeiling[i])
                            {
                                _graph.HeightCeiling[i] = j + 1;
                            }
                            if (k == _graph.WidthFloor[i])
                            {
                                _graph.WidthFloor[i] = k - 1;
                            }
                            if (k == _graph.WidthCeiling[i])
                            {
                                _graph.WidthCeiling[i] = k + 1;
                            }

                            // if our smallest node is set to -1, then any valid node will be the smallest.
                            if (smallestNode.height == -1)
                            {
                                smallestNode.height = j;
                                smallestNode.width = k;
                                smallestNode.floor = i;            
                            }
                            else
                            {
                                if (_graph.travelDistance[i][j][k] < _graph.travelDistance[smallestNode.floor][smallestNode.height][smallestNode.width])
                                {
                                    smallestNode.height = j;
                                    smallestNode.width = k;
                                    smallestNode.floor = i;
                                }
                            }
                        }
                    }
                }

                if (floorCleared)
                {
                    _graph.floorCleared[i] = true;
                    // We didn't find any unvisited nodes on this floor, consider it cleared and never look at it again.
                    // Display to debug
                    Debug.WriteLine("Floor " + i + " has been cleared.");
                }
            }

            return smallestNode;

        }

        private bool canPushWallMove(pathfindingNode Node)
        {
            if (_graph.getGraphValue(Node.floor, Node.height, Node.width) == 1)
            {
                return true;
            }
            else if (_graph.getGraphValue(Node.floor, Node.height, Node.width) == -2)
            {
                return true;
            }
            else if (_graph.getGraphValue(Node.floor, Node.height, Node.width) == -3)
            {
                return true;
            }

            return false;
        }

        private bool detectNearPushwall(pathfindingNode currentNode, int direction, bool activate)
        {
            // Check to see if there's a pushwall in that direction.
            pathfindingNode updateNode = currentNode;
            bool pushwallCanMove = false;

            switch (direction)
            {
                case 1:
                    updateNode.height -= 1;
                    break;
                case 2:
                    updateNode.width += 1;
                    break;
                case 3:
                    updateNode.height += 1;
                    break;
                case 4:
                    updateNode.width -= 1;
                    break;
            }
            // Is updateNode a pushwall?
            if (_graph.getGraphValue(updateNode.floor, updateNode.height, updateNode.width) == -5)
            {
                pathfindingNode pushWallNode = updateNode;
                pathfindingNode pushWallTest = updateNode;

                // If we push it in this direction can it move, and if so would it be able to move one block or two blocks?
                switch (direction)
                {
                    case 1:
                        pushWallTest.height -= 1;
                        break;
                    case 2:
                        pushWallTest.width += 1;
                        break;
                    case 3:
                        pushWallTest.height += 1;
                        break;
                    case 4:
                        pushWallTest.width -= 1;
                        break;
                }
                // Check to see if the pushwall can move.
                if (canPushWallMove(pushWallTest))
                {
                    pushwallCanMove = true;
                    pushWallNode = pushWallTest;
                    // Can we move a second time?
                    switch (direction)
                    {
                        case 1:
                            pushWallTest.height -= 1;
                            break;
                        case 2:
                            pushWallTest.width += 1;
                            break;
                        case 3:
                            pushWallTest.height += 1;
                            break;
                        case 4:
                            pushWallTest.width -= 1;
                            break;
                    }

                    if (canPushWallMove(pushWallTest))
                    {
                        pushWallNode = pushWallTest;
                    }
                }

                updateNode = pushWallNode;
            }

            if (activate)
            {
                if (pushwallCanMove)
                {
                    // Debug text the pushwall location that lead to this new floor.
                    Debug.WriteLine("Pushwall found at: " + updateNode.height + " " + updateNode.width + " " + updateNode.floor);

                    // Clone a new floor and move us to that floor.
                    int newFloor = _graph.addFloor(currentNode.floor);
                    _graph.setGraphValue(newFloor, updateNode.height, updateNode.width, -1);
                    _graph.setGraphValue(newFloor, currentNode.height, currentNode.width, 1);

                    // Set the travel distance and to be the same on the new floor, but the direction to be 5, to indicate we're going down to the floor that spawned this floor.
                    _graph.travelDistance[newFloor][currentNode.height][currentNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 5;
                    _graph.travelDirection[newFloor][currentNode.height][currentNode.width] = 5;

                    return pushwallCanMove;
                }
            }

            return pushwallCanMove;
        }

        private void updateTravelDistance(pathfindingNode currentNode, int direction)
        {
            pathfindingNode updateNode = currentNode;

            // Modify updateNode to point to the new node.
            switch (direction)
            {
                case 1:
                    updateNode.height -= 1;
                    break;
                case 2:
                    updateNode.width += 1;
                    break;
                case 3:
                    updateNode.height += 1;
                    break;
                case 4:
                    updateNode.width -= 1;
                    break;
            }

            // Check to see if the new node is within the bounds of the map.
            if (updateNode.height < 0 || updateNode.height >= _mapdata.getMapHeight() || updateNode.width < 0 || updateNode.width >= _mapdata.getMapWidth())
            {
                return;
            }

            // Check to see if we've already visited this node.
            if (_graph.visited[updateNode.floor][updateNode.height][updateNode.width] == true)
            {
                return;
            }

            int travelDistance = _graph.getGraphValue(updateNode.floor, updateNode.height, updateNode.width);

            if (travelDistance == -10 || travelDistance == -11) // Found a key. Elevate to a new node floor.
            {
                // Set the graph value to 1 to remove significance from this node, then add a new floor, and on that new floor set the gold door to 2.
                _graph.setGraphValue(updateNode.floor, updateNode.height, updateNode.width, 1);

                // Set the travel direction and travel distance of the key node.
                _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 1;
                _graph.travelDirection[updateNode.floor][updateNode.height][updateNode.width] = direction;

                int newFloor = _graph.addFloor(updateNode.floor);

                // Set the travel distance and to be the same on the new floor, but the direction to be 5, to indicate we're going down to the floor that spawned this floor.
                _graph.travelDistance[newFloor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 1;
                _graph.travelDirection[newFloor][updateNode.height][updateNode.width] = 5;

                if (travelDistance == -10)
                {
                    // Look for the gold door and set it to 2.
                    for (int i = 0; i < _mapdata.getMapHeight(); i++)
                    {
                        for (int j = 0; j < _mapdata.getMapWidth(); j++)
                        {
                            if (_graph.getGraphValue(updateNode.floor, i, j) == -2)
                            {
                                _graph.setGraphValue(newFloor, i, j, 2);
                            }
                        }
                    }
                }
                else 
                {
                    // Look for the silver door and set it to 2.
                    for (int i = 0; i < _mapdata.getMapHeight(); i++)
                    {
                        for (int j = 0; j < _mapdata.getMapWidth(); j++)
                        {
                            if (_graph.getGraphValue(updateNode.floor, i, j) == -3)
                            {
                                _graph.setGraphValue(newFloor, i, j, 2);
                            }
                        }
                    }
                }
            }
            else if (travelDistance > -1) // Found a floor tile. Update the travel distance and direction.
            {

                // If the new node has a smaller travel distance than what we're going to update it to, don't update it.
                if (_graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] != -1 && _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] < _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance)
                {
                    return;
                }

                // Update the travel distance of the new node along with the direction we traveled to get there.
                _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance;
                _graph.travelDirection[updateNode.floor][updateNode.height][updateNode.width] = direction;
            }
        }

        public bool isTileOnPath(int height, int width)
        {
            for (int i = 0; i < _path.Count; i++)
            {
                if (_path[i].height == height && _path[i].width == width)
                {
                    return true;
                }
            }

            return false;
        }

        public void solveMaze()
        {
            pathfindingNode exitNode = new pathfindingNode(-1, -1, -1);
            // Set the first position to be explored with a travel distance of 0.
            _graph.travelDistance[0][firstNodeHeight][firstNodeWidth] = 0;
            // Travel direction of -1 is the start point. 1-4 are the directions of N, E, S, W. 4 + floor number is the travel floor.
            _graph.travelDirection[0][firstNodeHeight][firstNodeWidth] = -1;            

            while (true)
            {
                pathfindingNode currentNode = findsmallestunexploredNode();

                if (currentNode.height == -1)
                {
                    // If we can't find any more nodes to explore, we're done.
                    Debug.WriteLine("No more nodes to explore. -- Failed to solve -- ");
                    break;
                }

                // Dump to debug
                //Debug.WriteLine("Current Node: " + currentNode.height + " " + currentNode.width + " " + currentNode.floor + " " + _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width]);


                // If we've reached the exit, we're done.
                if (_graph.getGraphValue(currentNode.floor, currentNode.height, currentNode.width) == 0)
                {
                    Debug.WriteLine("Exit found. -- Solved -- ");
                    exitNode = currentNode;
                    mazeSolved = true;
                    break;
                }

                // Check to see if we can move north.
                updateTravelDistance(currentNode, 1);
                // Check to see if we can move east.
                updateTravelDistance(currentNode, 2);
                // Check to see if we can move south.
                updateTravelDistance(currentNode, 3);
                // Check to see if we can move west.
                updateTravelDistance(currentNode, 4);

                // Mark the current node as visited.
                _graph.visited[currentNode.floor][currentNode.height][currentNode.width] = true;

                // Check to see if there is a pushwall nearby.
                detectNearPushwall(currentNode, 1, true);
                detectNearPushwall(currentNode, 2, true);
                detectNearPushwall(currentNode, 3, true);
                detectNearPushwall(currentNode, 4, true);
            }

           if (mazeSolved)
            {
                // If we've solved the maze, backtrack from the exit to the start to find the path.
                pathfindingNode currentNode = exitNode;
                while (true)
                {
                    _path.Add(currentNode);

                    if (_graph.travelDirection[currentNode.floor][currentNode.height][currentNode.width] == -1)
                    {
                        break;
                    }

                    switch (_graph.travelDirection[currentNode.floor][currentNode.height][currentNode.width])
                    {
                        case 1:
                            currentNode.height += 1;
                            break;
                        case 2:
                            currentNode.width -= 1;
                            break;
                        case 3:
                            currentNode.height -= 1;
                            break;
                        case 4:
                            currentNode.width += 1;
                            break;
                        case 5: // Going down to the floor that spawned this floor.
                            currentNode.floor = _graph.floorMap[currentNode.floor];
                            break;
                    }
                }

                // Print out the path to debug.
                for (int i = _path.Count - 1; i >= 0; i--)
                {
                    Debug.WriteLine("Path: " + _path[i].height + " " + _path[i].width + " " + _path[i].floor);
                }
            }
            else
            {
                Debug.WriteLine("Failed to solve maze.");
            }

        }

        public void setStart(int height, int width)
        {
            firstNodeHeight = height;
            firstNodeWidth = width;
        }
        public void prepareBaseFloor()
        {
            // Check tile by tile of the mapdata. If it's a wall, set the graph value to -1. If it's a floor set it to 1, and if it's a door set it to 2.
            // If it's a locked door set it to -2. If it's a pushwall set it to 4.
            for (int i = 0; i < _mapdata.getMapHeight(); i++)
            {
                for (int j = 0; j < _mapdata.getMapWidth(); j++)
                {
                    if (_mapdata.getTileData(i, j) != 0)
                    {
                        _graph.setGraphValue(0, i, j, -1);
                    }
                    else if (_mapdata.getTileData(i, j) == 0)
                    {
                        _graph.setGraphValue(0, i, j, 1);
                    }

                    // If the tile is a -1, check to see if it's a door or a pushwall.
                    if (_graph.getGraphValue(0, i, j) == -1)
                    {
                        // Check to see if it's a door.
                        // [TODO] : x and y criss cross. I need to fix this and establish some consistency.
                        bool isDoor = _mapdata.isDoorOpenable(i, j, false, false);
                        bool isGoldDoor = _mapdata.isDoorOpenable(i, j, true, false);
                        bool isSilverDoor = _mapdata.isDoorOpenable(i, j, false, true);
                        bool isPushwall = _mapdata.isTilePushable(i, j);

                        if (isDoor)
                        {
                            _graph.setGraphValue(0, i, j, 2);
                        }
                        else if (isGoldDoor)
                        {
                            _graph.setGraphValue(0, i, j, -2);
                        }
                        else if (isSilverDoor)
                        {
                            _graph.setGraphValue(0, i, j, -3);
                        }
                        else if (isPushwall)
                        {
                            _graph.setGraphValue(0, i, j, -5);
                        }
                    }
                    else
                    {
                        // If an static map object is blocking the path, also set it to -1.
                        if (_mapdata.isTileBlocked(i, j))
                        {
                            _graph.setGraphValue(0, i, j, -1);
                        }
                        else
                        {
                            // If an exit tile is found to the left or right set it to 0 to signify the exit of the path.
                            if (_mapdata.getTileData(i, j - 1) == 21 || _mapdata.getTileData(i, j + 1) == 21)
                            {
                                _graph.setGraphValue(0, i, j, 0);
                            }
                        }

                        // Check for keys and set the graph value to -4 if a key is found.
                        if (_mapdata.getStaticObjectID(i, j) == 43)
                        {   // This is a gold key.
                            _graph.setGraphValue(0, i, j, -10);
                        }
                        else if (_mapdata.getStaticObjectID(i, j) == 44)
                        {   // This is a silver key.
                            _graph.setGraphValue(0, i, j, -11);
                        }
                    }
                }
            }

            // Print out the base floor map with even spacing between each tile to debug.
            /*for (int i = 0; i < _mapdata.getMapHeight(); i++)
            {
                for (int j = 0; j < _mapdata.getMapWidth(); j++)
                {
                    if (_graph.getGraphValue(0, i, j) == -1)
                    {
                        Debug.Write("X ");
                    }
                    else
                    {
                        Debug.Write(_graph.getGraphValue(0, i, j) + " ");
                    }                    
                }
                Debug.WriteLine("");
            }*/

        }

        public pathfinder(ref maphandler mapdata)
        {
            _mapdata = mapdata;

            firstNodeHeight = -1;
            firstNodeWidth = -1;

            mazeSolved = false;

            // Initialize our pathfinding graph.
            _graph = new pathfindingGraph(_mapdata.getMapWidth(), _mapdata.getMapHeight());
            _path = new List<pathfindingNode>();
        }
    }
}

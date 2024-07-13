using System.Diagnostics;

namespace Aardwolf
{
    internal class pathfindingGraph
    {
        public List<int[][]> graphMap;
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
            visited = new List<bool[][]>();
            travelDistance = new List<int[][]>();
            travelDirection = new List<int[][]>();

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
                for (int j = 0; j < _mapdata.getMapHeight(); j++)
                {
                    for (int k = 0; k < _mapdata.getMapWidth(); k++)
                    {
                        if (_graph.visited[i][j][k] == false && _graph.travelDistance[i][j][k] != -1)
                        {
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
            }

            return smallestNode;

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

            // Check to see if we can travel to the new node.
            if (travelDistance < 0)
            {
                return;
            }

            // If the new node has a smaller travel distance than what we're going to update it to, don't update it.
            if (_graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] != -1 && _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] < _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance)
            {
                return;
            }

            // Update the travel distance of the new node along with the direction we traveled to get there.
            _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance;
            _graph.travelDirection[updateNode.floor][updateNode.height][updateNode.width] = direction;
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
                Debug.WriteLine("Current Node: " + currentNode.height + " " + currentNode.width + " " + currentNode.floor + " " + _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width]);


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
                            _graph.setGraphValue(0, i, j, 5);
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

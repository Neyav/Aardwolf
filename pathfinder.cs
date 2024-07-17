using System.Diagnostics;

namespace Aardwolf
{
    internal class pathfindingGraph
    {
        private Dictionary<pathfindingNode, Int16> graphMap;
        public List<int> floorMap;
        public List<int> treasuresRemaining;
        public List<int> secretsRemaining;
        public List<bool[][]> visited;
        public List<int[][]> travelDistance;
        public List<int[][]> travelDirection;
        public Queue<pathfindingNode> pathfindingQueue;
        private int _Width;
        private int _Height;

        public void setGraphValue(int floor, int height, int width, Int16 value)
        {
            pathfindingNode node = new pathfindingNode(height, width, floor);

            graphMap[node] = value;
        }

        public Int16 getGraphValue(int floor, int height, int width)
        {
            pathfindingNode searchNode = new pathfindingNode(height, width, floor);
            Int16 value = -1;

            while (!graphMap.TryGetValue(searchNode, out value))
                searchNode.floor = floorMap[searchNode.floor]; // Descend floors till we find where this block changed.

            return value;
        }

        public int addFloor(int baseFloor)
        {
            
            floorMap.Add(baseFloor); // Keep track of the floor we came from.

            treasuresRemaining.Add(0);
            secretsRemaining.Add(0);

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

            return floorMap.Count - 1;
        }

        public int returnScore(pathfindingNode node)
        {
            // The score is how average steps between each treasure and secret found.
            int optionals = treasuresRemaining[0] + secretsRemaining[0] - treasuresRemaining[node.floor] - secretsRemaining[node.floor];
            int steps = travelDistance[node.floor][node.height][node.width];

            return steps / (optionals + 1); 
        }
        public pathfindingGraph(int graphWidth, int graphHeight)
        {
            _Width = graphWidth;
            _Height = graphHeight;

            // Initialize the graph with a single floor.
            graphMap = new Dictionary<pathfindingNode, Int16>();
            floorMap = new List<int>();
            treasuresRemaining = new List<int>();
            secretsRemaining = new List<int>();
            visited = new List<bool[][]>();
            travelDistance = new List<int[][]>();
            travelDirection = new List<int[][]>();
            pathfindingQueue = new Queue<pathfindingNode>();

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
        public bool mazeFailed;
        public bool ignorePushWalls;
        public bool allSecrets;
        int biggestQueue;

        pathfindingNode findsmallestunexploredNode()
        {
            pathfindingNode smallestNode = new pathfindingNode(-1, -1, -1);

            // Sort the queue by travelDistance
            //_graph.pathfindingQueue = new Queue<pathfindingNode>(_graph.pathfindingQueue.OrderBy(x => _graph.travelDistance[x.floor][x.height][x.width]));

            //if (_graph.pathfindingQueue.Count > biggestQueue)
            //{
            //    biggestQueue = _graph.pathfindingQueue.Count;
            //}

            if (_graph.pathfindingQueue.Count == 0)
            {
                return smallestNode;
            }
            else if (allSecrets)
            {
                // If biggestqueue is divisable by 50, sort it based on returnScore
                if (biggestQueue > 200000)
                {
                    biggestQueue = 0;
                    Debug.WriteLine("Sorting queue by return score.");
                    // sort it so the highest score is the first item in the queue.



                    _graph.pathfindingQueue = new Queue<pathfindingNode>(_graph.pathfindingQueue.OrderByDescending(x => _graph.returnScore(x)));

                    Queue<pathfindingNode> tempQueue = new Queue<pathfindingNode>();
                    // Move pathfindingQueue to tempQueue, but only items that have a returnScore of at least half of the first entry.
                    // Dump the treasure and secrets and travel distance of each node, along with whether it gets nuked or not?
                    int biggestScore = 0;

                    biggestScore = _graph.returnScore(_graph.pathfindingQueue.Peek());
                    Debug.WriteLine("Biggest Score: " +  biggestScore);


                    /*for (int i = 0; i < _graph.pathfindingQueue.Count; i++)
                    {
                        // If the score of this entry is less than quarter of biggestScore, break, otherwise add it to tempqueue

                        int localScore = _graph.returnScore(_graph.pathfindingQueue.Peek());

                        if (localScore < biggestScore / 4)
                        {
                            // output how many nodes we're nuking.
                            Debug.WriteLine("Nuking " + _graph.pathfindingQueue.Count + " nodes.");
                            break;
                        }

                        pathfindingNode node = _graph.pathfindingQueue.Dequeue();
                        tempQueue.Enqueue(node);
                    }
                    
                    // Swap them.
                    _graph.pathfindingQueue = tempQueue;*/

                }
                else
                    biggestQueue++;

                smallestNode = _graph.pathfindingQueue.Dequeue();
            }
            else
            {
                smallestNode = _graph.pathfindingQueue.Dequeue();
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
            pathfindingNode pushWallBlock = currentNode;
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
                pushWallBlock = updateNode;

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
                    // Clone a new floor and move us to that floor.
                    int newFloor = _graph.addFloor(currentNode.floor);
                    _graph.setGraphValue(newFloor, updateNode.height, updateNode.width, -1);
                    _graph.setGraphValue(newFloor, pushWallBlock.height, pushWallBlock.width, 1);

                    // Set the travel distance and to be the same on the new floor, but the direction to be 5, to indicate we're going down to the floor that spawned this floor.
                    _graph.travelDistance[newFloor][currentNode.height][currentNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 5;
                    _graph.travelDirection[newFloor][currentNode.height][currentNode.width] = 5;

                    _graph.treasuresRemaining[newFloor] = _graph.treasuresRemaining[updateNode.floor];
                    _graph.secretsRemaining[newFloor] = _graph.secretsRemaining[updateNode.floor];

                    if (allSecrets)
                    {
                        _graph.secretsRemaining[newFloor]--;

                        // Dump it to debug.
                        Debug.WriteLine("Secrets remaining: " + _graph.secretsRemaining[newFloor] + " on floor " + newFloor);

                        // If we're looking for all secrets, and we've found all secrets and items, let the exit return.
                        if (_graph.treasuresRemaining[newFloor] == 0 && _graph.secretsRemaining[newFloor] == 0)
                        {
                            reinitalizeExits(newFloor);
                        }
                    }

                    // add this node to the queue
                    _graph.pathfindingQueue.Enqueue(new pathfindingNode(currentNode.height, currentNode.width, newFloor));

                    return pushwallCanMove;
                }
            }

            return pushwallCanMove;
        }

        private void reinitalizeExits(int floor)
        {
            // Go through the floor map and find any -15 exits and set them to 0.
            for (int i = 0; i < _mapdata.getMapHeight(); i++)
            {
                for (int j = 0; j < _mapdata.getMapWidth(); j++)
                {
                    if (_graph.getGraphValue(floor, i, j) == -15)
                    {
                        _graph.setGraphValue(floor, i, j, 0);
                    }
                }
            }
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

            if (travelDistance == -10 || travelDistance == -11 || travelDistance == -16) // Found a key or a treasure.. Elevate to a new node floor.
            {
                // Set the graph value to 1 to remove significance from this node, then add a new floor, and on that new floor set the gold door to 2.
                _graph.setGraphValue(updateNode.floor, updateNode.height, updateNode.width, 1);

                // Set the travel direction and travel distance of the key node.
                _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 1;
                _graph.travelDirection[updateNode.floor][updateNode.height][updateNode.width] = direction;

                int newFloor = _graph.addFloor(updateNode.floor);

                _graph.treasuresRemaining[newFloor] = _graph.treasuresRemaining[updateNode.floor];
                _graph.secretsRemaining[newFloor] = _graph.secretsRemaining[updateNode.floor];

                if (allSecrets)
                {
                    if (travelDistance == -16)
                    {
                        _graph.treasuresRemaining[newFloor]--;
                        // Dump it to debug.
                        Debug.WriteLine("[" + _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] + "]: Treasures remaining: " + _graph.treasuresRemaining[newFloor] + "/" + _graph.treasuresRemaining[0] + " Secrets: " + _graph.secretsRemaining[newFloor] + "/" + _graph.secretsRemaining[0] + " on floor " + newFloor);
                    }
                    // If we're looking for all secrets, and we've found all secrets and items, let the exit return.
                    if (_graph.treasuresRemaining[newFloor] == 0 && _graph.secretsRemaining[newFloor] == 0)
                    {
                        reinitalizeExits(newFloor);
                    }
                }

                // Set the travel distance and to be the same on the new floor, but the direction to be 5, to indicate we're going down to the floor that spawned this floor.
                _graph.travelDistance[newFloor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + 1;
                _graph.travelDirection[newFloor][updateNode.height][updateNode.width] = 5;

                // Add this node to the queue.
                _graph.pathfindingQueue.Enqueue(new pathfindingNode(updateNode.height, updateNode.width, newFloor));

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
                if (_graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] != -1 && _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] <= _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance)
                {
                    return;
                }

                // Update the travel distance of the new node along with the direction we traveled to get there.
                _graph.travelDistance[updateNode.floor][updateNode.height][updateNode.width] = _graph.travelDistance[currentNode.floor][currentNode.height][currentNode.width] + travelDistance;
                _graph.travelDirection[updateNode.floor][updateNode.height][updateNode.width] = direction;

                // Add the new node to the queue.
                _graph.pathfindingQueue.Enqueue(new pathfindingNode(updateNode.height, updateNode.width, updateNode.floor));
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

            if (allSecrets)
            {
                // Scan the ground floor map. We need to count the number of secrets and treasures on the map.
                // Replace the 0 floor exit with -15, and set all treasures to -16.

                for (int i = 0; i < _mapdata.getMapHeight(); i++)
                {
                    for (int j = 0; j < _mapdata.getMapWidth(); j++)
                    {
                        if (_graph.getGraphValue(0,i,j) == 0)
                        {
                            _graph.setGraphValue(0, i, j, -15);
                        }
                        else if (_mapdata.getStaticObjectID(i, j) >= 52 && _mapdata.getStaticObjectID(i,j) <= 55)
                        {
                            _graph.setGraphValue(0, i, j, -16);
                            _graph.treasuresRemaining[0]++;
                        }

                        if (_mapdata.isTilePushable(i, j))
                        {
                            _graph.secretsRemaining[0]++;
                        }
                    }
                }

                // Debug how many secrets and treasures we have.
                Debug.WriteLine("Treasures remaining: " + _graph.treasuresRemaining[0]);
                Debug.WriteLine("Secrets remaining: " + _graph.secretsRemaining[0]);
            }

            // Add this node to the queue.
            _graph.pathfindingQueue.Enqueue(new pathfindingNode(firstNodeHeight, firstNodeWidth, 0));

            while (true)
            {
                pathfindingNode currentNode = findsmallestunexploredNode();

                if (currentNode.height == -1)
                {
                    // If we can't find any more nodes to explore, we're done.
                    Debug.WriteLine("No more nodes to explore. -- Failed to solve -- ");
                    break;
                }

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

                if (ignorePushWalls)
                {
                    continue;
                }
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
            }
            else
            {
                Debug.WriteLine("Failed to solve maze.");
                mazeFailed = true;
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
            biggestQueue = 0;

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
        }

        public pathfinder(ref maphandler mapdata)
        {
            _mapdata = mapdata;

            firstNodeHeight = -1;
            firstNodeWidth = -1;

            biggestQueue = 0;

            mazeSolved = false;
            mazeFailed = false;
            ignorePushWalls = false;
            allSecrets = false;

            // Initialize our pathfinding graph.
            _graph = new pathfindingGraph(_mapdata.getMapWidth(), _mapdata.getMapHeight());
            _path = new List<pathfindingNode>();
        }
    }
}

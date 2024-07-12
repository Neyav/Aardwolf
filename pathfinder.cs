using System.Diagnostics;

namespace Aardwolf
{
    internal class pathfindingGraph
    {
        private List<int[][]> graphMap;
        private List<bool[][]> visited;
        private List<int[][]> travelDistance;
        private List<int[][]> travelDirection;
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
            }

            visited.Add(newVisited);

            // Create a new blank travel distance template.
            int[][] newTravelDistance = new int[_Height][];
            for (int i = 0; i < _Height; i++)
            {
                newTravelDistance[i] = new int[_Width];
            }

            travelDistance.Add(newTravelDistance);

            // Create a new blank travel direction template.
            int[][] newTravelDirection = new int[_Height][];
            for (int i = 0; i < _Height; i++)
            {
                newTravelDirection[i] = new int[_Width];
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

    internal class pathfinder
    {
        private maphandler _mapdata;
        private int _currentNode;
        private pathfindingGraph _graph;

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
            for (int i = 0; i < _mapdata.getMapHeight(); i++)
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
            }

        }

        public pathfinder(ref maphandler mapdata)
        {
            _mapdata = mapdata;            

            _currentNode = 0;

            // Initialize our pathfinding graph.
            _graph = new pathfindingGraph(_mapdata.getMapWidth(), _mapdata.getMapHeight());
        }
    }
}

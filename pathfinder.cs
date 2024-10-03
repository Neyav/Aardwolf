using System.Diagnostics;
using System.Numerics;

namespace Aardwolf
{
    enum nodeStatus
    {
        none = 0,
        goldKey = 1,
        silverKey = 2,
        bothKeys = 3
    };

    enum direction
    {
        north = 0,
        east = 1,
        south = 2,
        west = 3
    };

    internal class lineofSightOffset
    {
        public readonly float[] calculatedOffset;

        static private Complex Normalize(Complex c)
        {
            double magnitude = c.Magnitude;
            return new Complex(c.Real / magnitude, c.Imaginary / magnitude);
        }
        public lineofSightOffset()
        {
            calculatedOffset = new float[64];

            for (int i = 1; i < 65; i++)
            {
                Complex lineStart = new Complex(0.5, 0.5);
                Complex lineEnd = new Complex(1, i + 0);

                Complex direction = lineEnd - lineStart;

                // Normalize the direction vector
                Complex normalizedDirection = Normalize(direction);

                normalizedDirection *= 1 / normalizedDirection.Imaginary;

                calculatedOffset[i - 1] = (float)normalizedDirection.Real;
            }
        }
    }

    internal class coord2D
    {
        public int heightPosition;
        public int widthPosition;

        public void move(direction dir)
        {
            switch (dir)
            {
                case direction.north:
                    heightPosition--;
                    break;
                case direction.east:
                    widthPosition++;
                    break;
                case direction.south:
                    heightPosition++;
                    break;
                case direction.west:
                    widthPosition--;
                    break;
            }
        }
        public coord2D(int heightPosition, int widthPosition)
        {
            this.heightPosition = heightPosition;
            this.widthPosition = widthPosition;
        }
    }

    internal class mapTile
    {
        public readonly coord2D position;

        public mapTile East;
        public mapTile West;
        public mapTile North;
        public mapTile South;

        public bool blocked;
        public bool isDoor;
        public nodeStatus locked;

        public nodeStatus hasKey;

        public mapTile getAdjacentTile(direction dir)
        {
            if (dir == direction.north)
            {
                return North;
            }
            else if (dir == direction.east)
            {
                return East;
            }
            else if (dir == direction.south)
            {
                return South;
            }
            else if (dir == direction.west)
            {
                return West;
            }

            return null;
        }

        public mapTile(int heightPosition, int widthPosition)
        {
            this.position = new coord2D(heightPosition, widthPosition);

            East = West = North = South = null;
        }
    }
    internal class pathNode
    {
        public readonly int heightPosition;
        public readonly int widthPosition;

        public bool endPoint;
    }

    internal class pathfinder
    {
        private maphandler _mapdata;

        public bool ignorePushWalls;

        private mapTile[,] _mapTiles;

        private void generateMapTileset()
        {
            for (int heightPos = 0; heightPos < _mapdata.getMapHeight(); heightPos++)
            {
                for (int widthPos = 0; widthPos < _mapdata.getMapWidth(); widthPos++)
                {
                    _mapTiles[heightPos, widthPos] = new mapTile(heightPos, widthPos);

                    _mapTiles[heightPos, widthPos].North = heightPos > 0 ? _mapTiles[heightPos - 1, widthPos] : null;
                    _mapTiles[heightPos, widthPos].West = widthPos > 0 ? _mapTiles[heightPos, widthPos - 1] : null;

                    if (heightPos > 0)
                    {
                        _mapTiles[heightPos - 1, widthPos].South = _mapTiles[heightPos, widthPos];
                    }

                    if (widthPos > 0)
                    {
                        _mapTiles[heightPos, widthPos - 1].East = _mapTiles[heightPos, widthPos];
                    }

                    if (_mapdata.isTileBlocked(heightPos, widthPos))
                    {
                        _mapTiles[heightPos, widthPos].blocked = true;
                    }
                    else if (_mapdata.getTileData(heightPos, widthPos) != 0)
                    {
                        _mapTiles[heightPos, widthPos].blocked = true;
                    }
                    else
                    {
                        _mapTiles[heightPos, widthPos].blocked = false;
                    }

                    if (_mapdata.isDoorOpenable(heightPos, widthPos, false, false))
                    {
                        _mapTiles[heightPos, widthPos].isDoor = true;
                    }
                    else if (_mapdata.isDoorOpenable(heightPos, widthPos, true, false))
                    {
                        _mapTiles[heightPos, widthPos].isDoor = true;
                        _mapTiles[heightPos, widthPos].locked = nodeStatus.goldKey;
                    }
                    else if (_mapdata.isDoorOpenable(heightPos, widthPos, false, true))
                    {
                        _mapTiles[heightPos, widthPos].isDoor = true;
                        _mapTiles[heightPos, widthPos].locked = nodeStatus.silverKey;
                    }
                    else
                    {
                        _mapTiles[heightPos, widthPos].isDoor = false;
                    }

                    if (_mapdata.getStaticObjectID(heightPos, widthPos) == 43)
                    {
                        _mapTiles[heightPos, widthPos].hasKey = nodeStatus.goldKey;
                    }
                    else if (_mapdata.getStaticObjectID(heightPos, widthPos) == 44)
                    {
                        _mapTiles[heightPos, widthPos].hasKey = nodeStatus.silverKey;
                    }
                    else
                    {
                        _mapTiles[heightPos, widthPos].hasKey = nodeStatus.none;
                    }
                }
            }
        }

        private direction returnClockwiseAdjacent(direction dir)
        {
            if (dir == direction.north)
            {
                return direction.east;
            }
            else if (dir == direction.east)
            {
                return direction.south;
            }
            else if (dir == direction.south)
            {
                return direction.west;
            }
            else if (dir == direction.west)
            {
                return direction.north;
            }

            return direction.north;
        }

        private direction returnCounterClockwiseAdjacent(direction dir)
        {
            if (dir == direction.north)
            {
                return direction.west;
            }
            else if (dir == direction.east)
            {
                return direction.north;
            }
            else if (dir == direction.south)
            {
                return direction.east;
            }
            else if (dir == direction.west)
            {
                return direction.south;
            }

            return direction.north;
        }


        private List<mapTile> calculateVisibility(mapTile startTile)
        {
            List<mapTile> visibilityMap = new List<mapTile>();

            return visibilityMap;
        }

        public pathNode returnNode(int heightPosition, int widthPosition)
        {
            if (_mapTiles[heightPosition, widthPosition].blocked)
            {
                return null;
            }
            else return new pathNode();

            return null;
        }

        public List<pathNode> returnConnectedNodes(int heightPosition, int widthPosition)
        {
            List<pathNode> nodes = new List<pathNode>();

            return nodes;
        }

        public List<pathNode> returnRoute()
        {
            List<pathNode> nodes = new List<pathNode>();

            return nodes;
        }

        public bool solveMaze()
        {
            return false;
        }

        public List<pathNode> returnTraversableNodes()
        {
            List<pathNode> nodes = new List<pathNode>();

            return nodes;
        }

        public void preparePathFinder()
        {
            lineofSightOffset precalculatedLosOffset = new lineofSightOffset();

            generateMapTileset();
        }
        public pathfinder(ref maphandler mapdata)
        {
            _mapdata = mapdata;

            _mapTiles = new mapTile[_mapdata.getMapHeight(), _mapdata.getMapWidth()];

        }
    }   

}

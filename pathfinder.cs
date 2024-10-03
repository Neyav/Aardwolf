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

        public pathNode returnNode(int heightPosition, int widthPosition)
        {
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
            lineofSightOffset los = new lineofSightOffset();

        }
        public pathfinder(ref maphandler mapdata)
        {
            _mapdata = mapdata;

        }
    }   

}

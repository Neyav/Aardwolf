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


        }
        public pathfinder (ref maphandler mapdata)
        {
            _mapdata = mapdata;

        }
    }
}

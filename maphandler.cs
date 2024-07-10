// maphandler -- It is the estimated design that this will be responsible for clipping and collision detection, doors, item pickups, and pushwalls.
//               Enemy AI is probable best left as part of the actor class, with access to a pathfinding class.
//               This is all rough planning so far.

using System.Diagnostics;

namespace Aardwolf
{
    public enum mapObjectTypes
    {
        MAPOBJECT_NONE = 0,
        MAPOBJECT_DOOR = 1,
        MAPOBJECT_PUSHWALL = 2
    }
    public enum mapDirection
    {
        DIR_NORTH = 0,
        DIR_EAST = 1,
        DIR_SOUTH = 2,
        DIR_WEST = 3
    }

    // We don't have any kind of game ticrate setup yet, so things are either open or closed.
    // [Dash|RD] TODO: When we get a ticrate setup we'll need to add a tick method to this class to automatically handle all of the map objects.
    struct dynamicMapObject
    {
        public mapObjectTypes type;
        public int spawnx;
        public int spawny;
        public int x;
        public int y;

        public int keyNumber;
        public bool activated;
        public mapDirection activatedDirection;
        public int progress;

        public dynamicMapObject()
        {
            this.type = mapObjectTypes.MAPOBJECT_NONE;
            this.spawnx = 0;
            this.spawny = 0;
            this.x = 0;
            this.y = 0;
            this.keyNumber = 0;
            this.activated = false;
            this.activatedDirection = mapDirection.DIR_NORTH;
            this.progress = 0;
        }
    }

    struct staticMapObject
    {
        public bool blocking;
        public bool obtainable;
        public int x;
        public int y;

        public staticMapObject()
        {
            this.blocking = false;
            this.obtainable = false;
            this.x = 0;
            this.y = 0;
        }
    }

    internal class maphandler
    {
        private byte[][] levelTileMap;
        List<dynamicMapObject> dynamicMapObjects;
        List<staticMapObject> staticMapObjects;
        private bool _isLoaded = false;
        private int _mapHeight;
        private int _mapWidth;

        public void importMapData(byte[] rawMapData, int mapHeight, int mapWidth)
        {
            _mapHeight = mapHeight;
            _mapWidth = mapWidth;

            // Reset the dynamic map objects list.
            dynamicMapObjects.Clear();

            // Original map data is stored as a 16 bit word, we need to convert it to a byte array.
            levelTileMap = new byte[mapHeight][];
            for (int i = 0; i < mapHeight; i++)
            {
                levelTileMap[i] = new byte[mapWidth];
                for (int j = 0; j < mapWidth; j++)
                {
                    byte tilebyte = rawMapData[(i * mapHeight + j) * 2];

                    // We're only storing tiles that are floors. If we decide to use the sound prop tiles we'll store them elsewhere.
                    if (tilebyte <= 101)
                        levelTileMap[i][j] = tilebyte;

                    if (tilebyte >= 90 && tilebyte <= 101)
                    {   // It's a door, spawn a dynamic object for it so we can track it.
                        spawnDoorObject(tilebyte, j, i);
                    }                    
                }
            }

            _isLoaded = true;
        }

        public void spawnDoorObject(int tileNumber, int x, int y)
        {
            if (tileNumber >= 90 && tileNumber <= 101) // It's a door.
            {
                // Determine which type of door.
                byte doorType = 0;

                switch (tileNumber)
                {
                    case 90:
                    case 92:
                    case 94:
                    case 96:
                    case 98:
                    case 100:
                        doorType = (byte)((tileNumber - 90) / 2);
                        break;
                    case 91:
                    case 93:
                    case 95:
                    case 97:
                    case 99:
                    case 101:
                        doorType = (byte)((tileNumber - 91) / 2);
                        break;
                }

                dynamicMapObject newObject = new dynamicMapObject();
                newObject.type = mapObjectTypes.MAPOBJECT_DOOR;
                newObject.spawnx = x;
                newObject.spawny = y;
                newObject.x = x;
                newObject.y = y;

                // It's a locked door, set the appropriate key type.
                if (doorType > 0 && doorType < 5)
                {
                    newObject.keyNumber = doorType;

                    // Print to debug that we require a key.
                    Debug.WriteLine("Door at " + x + ", " + y + " requires key " + doorType);
                }

                dynamicMapObjects.Add(newObject);
            }
            else
            {
                Debug.WriteLine("Error: Attempted to spawn a door object with a non-door tile number.");
            }
        }

        public void spawnMapObject(int objNumber, int x, int y)
        {
            if (objNumber == 98) // It's a pushwall.
            {
                dynamicMapObject newObject = new dynamicMapObject();
                newObject.type = mapObjectTypes.MAPOBJECT_PUSHWALL;
                newObject.spawnx = x;
                newObject.spawny = y;
                newObject.x = x;
                newObject.y = y;
                dynamicMapObjects.Add(newObject);
            }
            
        }

        public bool isTilePushable(int x, int y)
        {
            foreach (dynamicMapObject obj in dynamicMapObjects)
            {
                if (obj.x == x && obj.y == y)
                {
                    if (obj.type == mapObjectTypes.MAPOBJECT_PUSHWALL && !obj.activated)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool isDoorOpenable(int x, int y, bool goldKey, bool silverKey)
        {
            foreach (dynamicMapObject obj in dynamicMapObjects)
            {
                if (obj.x == x && obj.y == y)
                {
                    if (obj.type == mapObjectTypes.MAPOBJECT_DOOR)
                    {
                        if (obj.keyNumber == 0)
                        {
                            return true;
                        }
                        else if (obj.keyNumber == 1 && goldKey)
                        {
                            return true;
                        }
                        else if (obj.keyNumber == 2 && silverKey)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public byte getTileData(int Height, int Width)
        {
            if (_isLoaded)
            {
                return levelTileMap[Height][Width];
            }
            else
            {
                return 0;
            }
        }

        public int getMapHeight()
        {
            return _mapHeight;
        }

        public int getMapWidth()
        {
            return _mapWidth;
        }

        public maphandler()
        {
            dynamicMapObjects = new List<dynamicMapObject>();
            staticMapObjects = new List<staticMapObject>();

            _mapHeight = 0;
            _mapWidth = 0;
        }
    }
}

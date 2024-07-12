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

    public enum staticObjectInteraction
    {
        OBJ_NONE = 0,
        OBJ_BLOCKING = 1,
        OBJ_OBTAINABLE = 2
    }

    // We don't have any kind of game ticrate setup yet, so things are either open or closed.
    // [Dash|RD] TODO: When we get a ticrate setup we'll need to add a tick method to this class to automatically handle all of the map objects.
    struct dynamicMapObject
    {
        public mapObjectTypes type;
        public int spawnwidth;
        public int spawnheight;
        public int poswidth;
        public int posheight;

        public int keyNumber;
        public bool activated;
        public mapDirection activatedDirection;
        public int progress;

        public dynamicMapObject()
        {
            this.type = mapObjectTypes.MAPOBJECT_NONE;
            this.spawnwidth = 0;
            this.spawnheight = 0;
            this.poswidth = 0;
            this.posheight = 0;
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
        public int objectID;
        public int poswidth;
        public int posheight;

        public staticMapObject()
        {
            this.blocking = false;
            this.obtainable = false;
            this.objectID = 0;
            this.poswidth = 0;
            this.posheight = 0;
        }
    }

    internal class maphandler
    {
        private byte[][] levelTileMap;
        List<dynamicMapObject> dynamicMapObjects;
        List<staticMapObject> staticMapObjects;
        private bool _isLoaded = false;
        private bool _isSoD = false;
        private int _mapHeight;
        private int _mapWidth;

        Dictionary<byte, staticObjectInteraction> staticObjectClassification;        

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
                        spawnDoorObject(tilebyte, i, j);
                    }                    
                }
            }

            _isLoaded = true;
        }

        public void spawnDoorObject(int tileNumber, int height, int width)
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
                newObject.spawnwidth = width;
                newObject.spawnheight = height;
                newObject.poswidth = width;
                newObject.posheight = height;

                // It's a locked door, set the appropriate key type.
                if (doorType > 0 && doorType < 5)
                {
                    newObject.keyNumber = doorType;
                }

                dynamicMapObjects.Add(newObject);
            }
            else
            {
                Debug.WriteLine("Error: Attempted to spawn a door object with a non-door tile number.");
            }
        }

        public void spawnMapObject(int objNumber, int height, int width)
        {
            if (objNumber == 98) // It's a pushwall.
            {
                dynamicMapObject newObject = new dynamicMapObject();
                newObject.type = mapObjectTypes.MAPOBJECT_PUSHWALL;
                newObject.spawnwidth = width;
                newObject.spawnheight = height;
                newObject.poswidth = width;
                newObject.posheight = height;
                dynamicMapObjects.Add(newObject);
            }
            else if (objNumber >= 23 && (objNumber <= 70 || (_isSoD && objNumber <= 74)))
            { // It's a static object.
                staticMapObject newObject = new staticMapObject();
                newObject.poswidth = width;
                newObject.posheight = height;
                newObject.objectID = objNumber;

                // Determine if it's blocking or obtainable.
                if (staticObjectClassification.ContainsKey((byte)objNumber))
                {
                    switch (staticObjectClassification[(byte)objNumber])
                    {
                        case staticObjectInteraction.OBJ_BLOCKING:
                            newObject.blocking = true;
                            break;
                        case staticObjectInteraction.OBJ_OBTAINABLE:
                            newObject.obtainable = true;
                            break;
                    }
                }

                staticMapObjects.Add(newObject);
            }

        }

        // [Dash|RD] This is not referring to walls. It is purely a check on whether the tile is blocked by a static object.
        //           [TODO]: This same segment makes sense to refer to any dynamic objects as well, so we'll update it when they're in the game.
        public bool isTileBlocked(int height, int width)
        {
            foreach (staticMapObject obj in staticMapObjects)
            {
                if (obj.poswidth == width && obj.posheight == height)
                {
                    if (obj.blocking)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public bool isTilePushable(int height, int width)
        {
            foreach (dynamicMapObject obj in dynamicMapObjects)
            {
                if (obj.poswidth == width && obj.posheight == height)
                {
                    if (obj.type == mapObjectTypes.MAPOBJECT_PUSHWALL && !obj.activated)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool isDoorOpenable(int height, int width, bool goldKey, bool silverKey)
        {
            foreach (dynamicMapObject obj in dynamicMapObjects)
            {
                if (obj.poswidth == width && obj.posheight == height)
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

        public maphandler(bool a_isSoD)
        {
            dynamicMapObjects = new List<dynamicMapObject>();
            staticMapObjects = new List<staticMapObject>();

            _isSoD = a_isSoD;
            _mapHeight = 0;
            _mapWidth = 0;

            // Manually add blocking map objects.
            // These are the common objects. We'll add the SoD/non-SoD specifics after.
            staticObjectClassification = new Dictionary<byte, staticObjectInteraction>
            {
                { 24, staticObjectInteraction.OBJ_BLOCKING }, // Green Barrel
                { 25, staticObjectInteraction.OBJ_BLOCKING }, // Table/chairs
                { 26, staticObjectInteraction.OBJ_BLOCKING }, // Floor Lamp
                { 28, staticObjectInteraction.OBJ_BLOCKING }, // Hanged Man
                { 29, staticObjectInteraction.OBJ_OBTAINABLE }, // Bad Food
                { 30, staticObjectInteraction.OBJ_BLOCKING }, // Red Pillar
                { 31, staticObjectInteraction.OBJ_BLOCKING }, // Tree
                { 33, staticObjectInteraction.OBJ_BLOCKING }, // Sink
                { 34, staticObjectInteraction.OBJ_BLOCKING }, // Potted Plant
                { 35, staticObjectInteraction.OBJ_BLOCKING }, // Urn
                { 36, staticObjectInteraction.OBJ_BLOCKING }, // Bare Table
                { 39, staticObjectInteraction.OBJ_BLOCKING }, // Suit of armor
                { 40, staticObjectInteraction.OBJ_BLOCKING }, // Hanging Cage
                { 41, staticObjectInteraction.OBJ_BLOCKING }, // Skeleton in Cage
                { 43, staticObjectInteraction.OBJ_OBTAINABLE }, // Gold Key
                { 44, staticObjectInteraction.OBJ_OBTAINABLE }, // Silver Key
                { 45, staticObjectInteraction.OBJ_BLOCKING }, // STUFF
                { 47, staticObjectInteraction.OBJ_OBTAINABLE }, // Good Food
                { 48, staticObjectInteraction.OBJ_OBTAINABLE }, // First Aid
                { 49, staticObjectInteraction.OBJ_OBTAINABLE }, // Clip
                { 50, staticObjectInteraction.OBJ_OBTAINABLE }, // Machine Gun
                { 51, staticObjectInteraction.OBJ_OBTAINABLE }, // Gatling Gun
                { 52, staticObjectInteraction.OBJ_OBTAINABLE }, // Cross
                { 53, staticObjectInteraction.OBJ_OBTAINABLE }, // Chalice
                { 54, staticObjectInteraction.OBJ_OBTAINABLE }, // Bible
                { 55, staticObjectInteraction.OBJ_OBTAINABLE }, // Crown
                { 56, staticObjectInteraction.OBJ_OBTAINABLE }, // One Up
                { 57, staticObjectInteraction.OBJ_OBTAINABLE }, // Gibs food
                { 58, staticObjectInteraction.OBJ_BLOCKING }, // Barrel
                { 59, staticObjectInteraction.OBJ_BLOCKING }, // Well
                { 60, staticObjectInteraction.OBJ_BLOCKING }, // Empty Well
                { 61, staticObjectInteraction.OBJ_OBTAINABLE }, // Edible Gibs 2
                { 62, staticObjectInteraction.OBJ_BLOCKING }, // Flag
                { 68, staticObjectInteraction.OBJ_BLOCKING }, // Stove
                { 69, staticObjectInteraction.OBJ_BLOCKING } // Spears
            };

            // Add SoD specific objects.
            if (_isSoD)
            {
                staticObjectClassification.Add(38, staticObjectInteraction.OBJ_BLOCKING); // Gibs!
                staticObjectClassification.Add(67, staticObjectInteraction.OBJ_BLOCKING); // Gibs!
                staticObjectClassification.Add(71, staticObjectInteraction.OBJ_BLOCKING); // Marble Pillar
                staticObjectClassification.Add(72, staticObjectInteraction.OBJ_OBTAINABLE); // Box of ammo
                staticObjectClassification.Add(73, staticObjectInteraction.OBJ_BLOCKING); // Truck
                staticObjectClassification.Add(74, staticObjectInteraction.OBJ_OBTAINABLE); // Spear of Destiny
            }
            else
            {
                staticObjectClassification.Add(63, staticObjectInteraction.OBJ_BLOCKING); // Aaaardwolf!
            }
        }
    }
}

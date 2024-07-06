using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aardwolf
{
    internal class maphandler
    {
        private byte[][] levelTileMap;
        private bool _isLoaded = false;
        private int _mapHeight;
        private int _mapWidth;

        public void importMapData(byte[] rawMapData, int mapHeight, int mapWidth)
        {
            _mapHeight = mapHeight;
            _mapWidth = mapWidth;

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
                }
            }

            _isLoaded = true;

            // Print the map to debug output.
            /*for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    // Make sure we write two three bytes for each tile, even if it's a single number.
                    if (levelTileMap[i][j] < 10)
                        Debug.Write(" " + levelTileMap[i][j].ToString() + " ");
                    else
                        Debug.Write(levelTileMap[i][j].ToString() + " ");
                }
                Debug.WriteLine("");
            }*/
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
            _mapHeight = 0;
            _mapWidth = 0;
        }
    }
}

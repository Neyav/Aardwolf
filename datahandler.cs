﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;

namespace Aardwolf
{
    struct mapDataHeader
    {
        public Int32 offPlane0;
        public Int32 offPlane1;
        public Int32 offPlane2;
        public UInt32 lenPlane0;
        public UInt32 lenPlane1;
        public UInt32 lenPlane2;
        public UInt32 width;
        public UInt32 height;
        // Define a 16 byte character array.
        public char[] name;

        public mapDataHeader()
        {
            offPlane0 = 0;
            offPlane1 = 0;
            offPlane2 = 0;
            lenPlane0 = 0;
            lenPlane1 = 0;
            lenPlane2 = 0;
            width = 0;
            height = 0;
            name = new char[16];
        }
    }

    internal class dataHandler
    {
        Byte[] _AUDIOHED;
        Byte[] _AUDIOT;
        Byte[] _GAMEMAPS;
        Byte[] _MAPHEAD;
        Byte[] _VGADICT;
        Byte[] _VGAHEAD;
        Byte[] _VGAGRAPH;
        Byte[] _VSWAP;

        Int32[] _mapOffsets;

        int _levels;

        List<mapDataHeader> _mapDataHeaders;
        List<byte[]> _mapData_offPlane0;

        bool _isLoaded = false;
        bool _isSOD = false;

        public void loadAllData(bool isSOD)
        {
            if (_isLoaded)
                return;

            _isSOD = isSOD;
            // Load all of our files into the byte arrays
            if (!_isSOD)
            {   // Grab the wolf3D data files.
                _AUDIOHED = System.IO.File.ReadAllBytes("AUDIOHED.WL6");
                _AUDIOT = System.IO.File.ReadAllBytes("AUDIOT.WL6");
                _GAMEMAPS = System.IO.File.ReadAllBytes("GAMEMAPS.WL6");
                _MAPHEAD = System.IO.File.ReadAllBytes("MAPHEAD.WL6");
                _VGADICT = System.IO.File.ReadAllBytes("VGADICT.WL6");
                _VGAHEAD = System.IO.File.ReadAllBytes("VGAHEAD.WL6");
                _VGAGRAPH = System.IO.File.ReadAllBytes("VGAGRAPH.WL6");
                _VSWAP = System.IO.File.ReadAllBytes("VSWAP.WL6");
            }
            
            _isLoaded = true;
        }

        public byte[] getLevelData(int level)
        {
            if (!_isLoaded)
                return null;

            if (level > _levels)
                return null;

            return _mapData_offPlane0[level];
        }
        public void parseLevelData()
        {
            int iterator = 0;
            IDdecompression decompressor = new IDdecompression(ref _MAPHEAD);
            // Load all the _mapOffsets from the MAPHEAD file.
            // Grab the mapheader from _mapOffsets as we go, if it is valid, and dump it into
            // _mapDataHeaders. Use _MAPHEAD and _GAMEMAPS to do this.

            for (int i = 2; i < 402; i = i + 4)
            {
                _mapOffsets[i / 4] = BitConverter.ToInt32(_MAPHEAD, i);

                Debug.WriteLine("Offset: {0} -> {1}", i / 4, _mapOffsets[i / 4]);
            }

            while (_mapOffsets[iterator] != 0)
            {
                mapDataHeader localHeader = new mapDataHeader();

                _levels++;
                // Find the data from _mapOffsets[iterator] in _GAMEMAPS and put it in a mapDataHeader

                // Get the offset for the first plane

                /*Offset Type    Name Description
                    0   INT32LE offPlane0   Offset in GAMEMAPS to beginning of compressed plane 0 data(or <= 0 if plane is not present)
                    4   INT32LE offPlane1   Offset in GAMEMAPS to beginning of compressed plane 1 data(or <= 0 if plane is not present)
                    8   INT32LE offPlane2   Offset in GAMEMAPS to beginning of compressed plane 2 data(or <= 0 if plane is not present)
                    12  UINT16LE lenPlane0   Length of compressed plane 0 data(in bytes)
                    14  UINT16LE lenPlane1   Length of compressed plane 1 data(in bytes)
                    16  UINT16LE lenPlane2   Length of compressed plane 2 data(in bytes)
                    18  UINT16LE width   Width of level(in tiles)
                    20  UINT16LE height  Height of level(in tiles)
                    22  char[16] name    Internal name for level(used only by editor, not displayed in -game. null - terminated)*/

                localHeader.offPlane0 = BitConverter.ToInt32(_GAMEMAPS, _mapOffsets[iterator]);
                localHeader.offPlane1 = BitConverter.ToInt32(_GAMEMAPS, _mapOffsets[iterator] + 4);
                localHeader.offPlane2 = BitConverter.ToInt32(_GAMEMAPS, _mapOffsets[iterator] + 8);
                localHeader.lenPlane0 = BitConverter.ToUInt16(_GAMEMAPS, _mapOffsets[iterator] + 12);
                localHeader.lenPlane1 = BitConverter.ToUInt16(_GAMEMAPS, _mapOffsets[iterator] + 14);
                localHeader.lenPlane2 = BitConverter.ToUInt16(_GAMEMAPS, _mapOffsets[iterator] + 16);
                localHeader.width = BitConverter.ToUInt16(_GAMEMAPS, _mapOffsets[iterator] + 18);
                localHeader.height = BitConverter.ToUInt16(_GAMEMAPS, _mapOffsets[iterator] + 20);
                // The name is the next 16 bytes in a character array.
                for (int i = 0; i < 16; i++)
                {
                    localHeader.name[i] = Convert.ToChar(_GAMEMAPS[_mapOffsets[iterator] + 22 + i]);
                }
                string levelName = new string(localHeader.name);

                Debug.WriteLine("Level Name: [{0}] -- ", levelName);

                Debug.WriteLine("Level {0}: Width: {1}, Height: {2}, LenPlane0: {3}", iterator, localHeader.width, localHeader.height, localHeader.lenPlane0);

                _mapDataHeaders.Add(localHeader);
                iterator++;
            }

            Debug.WriteLine("Levels detected: {0}", _levels);

            // Now that we have all the mapDataHeaders, we can decompress the data and put it into a byte array.
            // Start with plane 0.
            for (int i = 0; i < _levels; i++)
            {
                byte[] localPlane0 = new byte[_mapDataHeaders[i].lenPlane0];
                byte[] decarmackedPlane0;
                byte[] decompressedPlane0;
                

                localPlane0 = _GAMEMAPS.Skip(_mapDataHeaders[i].offPlane0).Take((int)_mapDataHeaders[i].lenPlane0).ToArray();
                decarmackedPlane0 = decompressor.CarmackDecompress(localPlane0);
                decompressedPlane0 = decompressor.RLEWDecompress(decarmackedPlane0);

                _mapData_offPlane0.Add(decompressedPlane0);

                Debug.WriteLine("Plane 0: {0} -> {1} -> {2}", localPlane0.Length, decarmackedPlane0.Length, decompressedPlane0.Length);
            }


            
        }

        public int getLevels()
        {
            return _levels;
        }   

        public string getLevelName(int level)
        {
            if (level > _levels)
                return null;

            string levelName = new string(_mapDataHeaders[level].name);

            return levelName;
        }

        public dataHandler()
        {
            _isLoaded = false;
            _isSOD = false;

            _mapOffsets = new Int32[100];

            _mapDataHeaders = new List<mapDataHeader>();
            _mapData_offPlane0 = new List<byte[]>();

            _levels = 0;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;
using System.Drawing.Imaging;

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

    struct VSWAPHeader
    {
        public UInt16 chunkCount;
        public UInt16 spriteStart;
        public UInt16 soundStart;

        public UInt32[] chunkOffsets;
        public UInt16[] chunkLengths;

        public VSWAPHeader()
        {
            chunkCount = 0;
            spriteStart = 0;
            soundStart = 0;

            chunkOffsets = new UInt32[1];
            chunkLengths = new UInt16[1];
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

        // Level data
        Int32[] _mapOffsets;
        int _levels;
        List<mapDataHeader> _mapDataHeaders;
        List<byte[]> _mapData_offPlane0;
        List<byte[]> _mapData_offPlane1;
        List<byte[]> _mapData_offPlane2;

        // VSWAP data
        VSWAPHeader _VSWAPHeader;

        bool _isLoaded = false;
        bool _isSOD = false;

        private void prefetchAndDecompressPlanes()
        {
            IDdecompression decompressor = new IDdecompression(ref _MAPHEAD);

            for (int i = 0; i < _levels; i++)
            {
                byte[] localPlane0 = new byte[_mapDataHeaders[i].lenPlane0];
                byte[] localPlane1 = new byte[_mapDataHeaders[i].lenPlane1];
                byte[] localPlane2 = new byte[_mapDataHeaders[i].lenPlane2];

                localPlane0 = _GAMEMAPS.Skip(_mapDataHeaders[i].offPlane0).Take((int)_mapDataHeaders[i].lenPlane0).ToArray();
                localPlane1 = _GAMEMAPS.Skip(_mapDataHeaders[i].offPlane1).Take((int)_mapDataHeaders[i].lenPlane1).ToArray();
                localPlane2 = _GAMEMAPS.Skip(_mapDataHeaders[i].offPlane2).Take((int)_mapDataHeaders[i].lenPlane2).ToArray();

                _mapData_offPlane0.Add(decompressor.RLEWDecompress(decompressor.CarmackDecompress(localPlane0)));
                _mapData_offPlane1.Add(decompressor.RLEWDecompress(decompressor.CarmackDecompress(localPlane1)));
                _mapData_offPlane2.Add(decompressor.RLEWDecompress(decompressor.CarmackDecompress(localPlane2)));
            }
  
        }

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
            else
            {
                _AUDIOHED = System.IO.File.ReadAllBytes("AUDIOHED.SOD");
                _AUDIOT = System.IO.File.ReadAllBytes("AUDIOT.SOD");
                _GAMEMAPS = System.IO.File.ReadAllBytes("GAMEMAPS.SOD");
                _MAPHEAD = System.IO.File.ReadAllBytes("MAPHEAD.SOD");
                _VGADICT = System.IO.File.ReadAllBytes("VGADICT.SOD");
                _VGAHEAD = System.IO.File.ReadAllBytes("VGAHEAD.SOD");
                _VGAGRAPH = System.IO.File.ReadAllBytes("VGAGRAPH.SOD");
                _VSWAP = System.IO.File.ReadAllBytes("VSWAP.SOD");
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

        public bool tileIsPushWall(int level, int x, int y)
        {            
            if (!_isLoaded)
                return false;

            if (level > _levels)
                return false;

            if (x > _mapDataHeaders[level].width)
                return false;

            if (y > _mapDataHeaders[level].height)
                return false;

            // The x2 is because a byte is two characters in the array thanks to unicode.
            return (_mapData_offPlane1[level][(y * _mapDataHeaders[level].width + x) * 2] == 98);            
        }
        public void parseLevelData()
        {
            int iterator = 0;

            // Load all the _mapOffsets from the MAPHEAD file.
            // Grab the mapheader from _mapOffsets as we go, if it is valid, and dump it into
            // _mapDataHeaders. Use _MAPHEAD and _GAMEMAPS to do this.

            // SOD doesn't have the padding that Wolf3d does, so we need to do this differently.
            for (int i = 2; i < _MAPHEAD.Length; i = i + 4)
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
            prefetchAndDecompressPlanes();
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

        public void prepareVSWAP()
        {
            _VSWAPHeader.chunkCount = BitConverter.ToUInt16(_VSWAP, 0);
            _VSWAPHeader.spriteStart = BitConverter.ToUInt16(_VSWAP, 2);
            _VSWAPHeader.soundStart = BitConverter.ToUInt16(_VSWAP, 4);

            Debug.WriteLine("VSWAP Header: ChunkCount: {0}, spriteStart: {1}, SoundStart: {2}", _VSWAPHeader.chunkCount, _VSWAPHeader.spriteStart, _VSWAPHeader.soundStart);
        
            _VSWAPHeader.chunkOffsets = new UInt32[_VSWAPHeader.chunkCount];
            _VSWAPHeader.chunkLengths = new UInt16[_VSWAPHeader.chunkCount];

            for (int i = 0; i < _VSWAPHeader.chunkCount; i++)
            {
                _VSWAPHeader.chunkOffsets[i] = BitConverter.ToUInt32(_VSWAP, 6 + (i * 4));
                _VSWAPHeader.chunkLengths[i] = BitConverter.ToUInt16(_VSWAP, 6 + (_VSWAPHeader.chunkCount * 4) + (i * 2));

                Debug.WriteLine("VSWAP Chunk {0}: Offset: {1}, Length: {2}", i, _VSWAPHeader.chunkOffsets[i], _VSWAPHeader.chunkLengths[i]);
            }
        }

        public VSWAPHeader getVSWAPHeader
        {
            get { return _VSWAPHeader; }
        }
        public byte[] getTexture(int chunk)
        {
            if (chunk >= _VSWAPHeader.spriteStart)
                return null;

            byte[] chunkData = new byte[_VSWAPHeader.chunkLengths[chunk]];

            chunkData = _VSWAP.Skip((int)_VSWAPHeader.chunkOffsets[chunk]).Take((int)_VSWAPHeader.chunkLengths[chunk]).ToArray();

            return chunkData;
        }

        public void getSprite(int sprite)
        {
            int spriteOffset = sprite + _VSWAPHeader.spriteStart;
            Bitmap bitmap = new Bitmap(64, 64, PixelFormat.Format32bppArgb);
            palettehandler palette = new palettehandler(_isSOD);

            byte[] rawSpriteData = new byte[_VSWAPHeader.chunkLengths[spriteOffset]];

            rawSpriteData = _VSWAP.Skip((int)_VSWAPHeader.chunkOffsets[spriteOffset]).Take((int)_VSWAPHeader.chunkLengths[spriteOffset]).ToArray();

            // Set the whole bitmap to be transparent.
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0, 0));
                }
            }

            // [Dash|RD] -- Man, this Wolf3D stuff is a pain. I can never find good guides or references for it. Half the source code references I can find.
            //              to try and understand it use double casting from a pointer to a value and back to a pointer while also iterating the pointer on the same line.
            //              Okay, granted that's an exageration, but it's how I feel at this point. Nobody has been clear about how it works, and everyone that writes an
            //              implementation seems to be trying to get awesome grades for being clever. Well, we're not clever here. This is the future. We have 128 CORE CPUs.
            //              and more RAM than I can shake a stick at. I'm coding this in C# for Gods sake. Let's aim for readability. If this segment helps ANYONE in the future.
            //              out I will be so happy, because this was painful for me.

            //              rawSpriteData here is a dump of the total sprite data from the VSWAP file. Everything I talk about in here is going to be in reference to that.
            //              Anything NOT described in the data for the sprite is considered transparent. Everyone says Wolf3D has a transparency colour, there is no transparency
            //              colour. The data for transparent pixels is SIMPLY not described here. Also, everyone refers to them as texels. I get that, but it's annoying. We're
            //              drawing pixels. This is a primitive form of texture mapping; I don't like the word texel, maybe I'm just an old man. COFFEE is COFFEE, keep your frappachino
            //              out of my coding politics.

            //          The first four bytes describes the X extents for the sprite. Let's deal with that first.
            UInt16 xStart = BitConverter.ToUInt16(rawSpriteData, 0);
            UInt16 xEnd = BitConverter.ToUInt16(rawSpriteData, 2);

            //          Next we have a series of 16 bit unsigned ints that describe the offset to drawing instructions. These are offsets from the beginning of the sprite data.
            //          Wolf3D sprites are drawn in columns because the whole renderer is based on columns. It's part of the magic sauce that made it so fast. So these drawing
            //          instructions describe how to draw the columns. We're just going to fetch the whole series of offsets here and store it in an array. The data inbetween the
            //          end of this offset list (There is one offset for every column from xStart to xEnd) and the first offset is the actual pixel data. We'll get there, don't worry.

            int numColumns = xEnd - xStart + 1;

            UInt16[] columnOffsets = new UInt16[numColumns];

            for (int i = 0; i < numColumns; i++)
            {
                columnOffsets[i] = BitConverter.ToUInt16(rawSpriteData, 4 + (i * 2));
            }

            // DEBUG PRINT THE COLUMN OFFSETS
            for (int i = 0; i < numColumns; i++)
            {
                Debug.WriteLine("Column {0}: {1}", i, columnOffsets[i]);
            }

            //         The pixel data starts at the end of the column offset list. So we know that the pixel data starts at 4 + ((xEnd - xStart + 1) * 2).
            //         We're going to keep track of this as an iterator so we always know which pixel colour we're on.
            int pixelDataIterator = 4 + (numColumns * 2);

            // DEBUG PRINT THE PIXELDATAITERATOR
            Debug.WriteLine("Pixel Data Iterator: {0}", pixelDataIterator);

            //          Now lets start processing the drawing instructions. They are stored in 6 byte segments, three groups of uint16s.
            //          If the first uint16 is 0, it indicates the end of the instruction set for this column. This is actually fairly clever, because it allows us to
            //          terminate a 6 byte code sequence with a 2 byte signal, and the next columns data begins immedately after. This means we save 4 bytes of padding.
            //          This would have saved a lot of space back in the day. There are 400+ sprites in Wolf3D, and if they averaged 32 columns of data each, you save
            //          51k+ by doing this. Don't look at me like that. I had a 40MB HD when I first got Wolf3D. 51K was almost 10% of our conventional RAM. I ONLY HAD 2MB of
            //          memory in total. Man... almost makes me want to store all the sprites in memory and preprocess them just so that data exists twice in memory here.
            //          32GB of RAM, so much room for activities in here.
            //          
            //          Anyhow, the actual 6 byte code sequence. This is an example of it:
            //          76-- 56
            //          77-- 0
            //          78-- 248
            //          79-- 255
            //          80-- 52
            //          81-- 0
            //
            //          So the first int is 56, which is the y offset from the top of the image, doubled. So 28, y: 14 - 1 = 13.
            //          The second int is... Carmack knows. I don't know. It's some voodoo magic the engine uses. We're not using it here.
            //          The third int is 52, which is the y offset from the top of the image, doubled again. So 26, y: 13 - 1 = 12.
            //          So our drawing instruction in this example has our first pixel colour occupying X: xStart, Y: 12 -> 13.

            //          Let's start processing the drawing instructions.

            for (int xDraw = xStart; xDraw <= xEnd; xDraw++)
            {
                UInt16 yStart, yEnd;
                // Grab an iterator to our drawing instruction offset.
                int instructionOffset = columnOffsets[xDraw - xStart];

                // This loop occurs for every instruction in the column.
                while (true)
                {
                    // Grab yEnd.
                    yEnd = BitConverter.ToUInt16(rawSpriteData, instructionOffset);
                    // If yStart is 0, we're done with this column.
                    if (yEnd == 0)
                        break;

                    // Grab yStart.
                    yStart = BitConverter.ToUInt16(rawSpriteData, instructionOffset + 4);

                    // Move the instruction offset to the next instruction.
                    instructionOffset += 6;

                    // Cut them in half for... whatever reason.
                    yStart = (UInt16)(yStart / 2);
                    yEnd = (UInt16)(yEnd / 2);

                    for (int yDraw = yStart; yDraw < yEnd; yDraw++)
                    {
                        byte colour = rawSpriteData[pixelDataIterator];

                        // DEBUG PRINT THE COLOUR
                        Debug.WriteLine("Colour: {0}", colour);

                        pixelDataIterator++;

                        // We need to translate the colour from a 256 palette reference to a RGB value.
                        RGBA colourRGBA = palette.getPaletteColor(colour);
                        colourRGBA.a = 255; // The bitmap was preset to be transparent, so we're going to make every rendered pixel opaque.

                        bitmap.SetPixel(xDraw, yDraw, System.Drawing.Color.FromArgb(colourRGBA.a, colourRGBA.r, colourRGBA.g, colourRGBA.b));
                    }
                }
            }

            // Save the bitmap to a file for debugging.
            bitmap.Save("sprite" + sprite + ".png", ImageFormat.Png);
        }

        public dataHandler()
        {
            _isLoaded = false;
            _isSOD = false;

            _mapOffsets = new Int32[100];

            _mapDataHeaders = new List<mapDataHeader>();
            _mapData_offPlane0 = new List<byte[]>();
            _mapData_offPlane1 = new List<byte[]>();
            _mapData_offPlane2 = new List<byte[]>();

            _VSWAPHeader = new VSWAPHeader();

            _levels = 0;
        }


    }
}

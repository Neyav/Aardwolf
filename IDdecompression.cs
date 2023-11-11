using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// IDdecompression class --
//
// Handles the decompression of data using ID Software's LZ compression and RLEW compression schemes that were popular during this time frame.

namespace Aardwolf
{
    // [Dash|RD] - This keeps being a problem so I'm just going to lay it out concisely here.
    //              x86 machines of the present and past are little endian, which means the least significant byte is stored first.
    //              x64 follows suit. Wolf3D was originally written for 16 bit systems, so a "WORD" is 16 bits, or 2 bytes.
    //              or as we're going to reference here, topend, bottomend, as bytes.
    //              This gets confusing rather fast because the original source is good old fashioned C with pointers and obtuse
    //              variable names, iterators during conditionals, and other such fun making it difficult to follow.
    //              We don't need to aim for performance here, so we're going to aim for readability and accuracy.
    //              Unfortunately, because the data is double encoded without checksums, an error at any point breaks everything,
    //              so this is... a bit of a pain for me. More than it should be.
    internal class IDdecompression
    {
        byte topRLEWtag;
        byte bottomRLEWtag;

        public byte[] RLEWDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // Same thing with Carmack compression, we're starting at byte 2 because the first two bytes are the uncompressed size of the data.
            // Which, oddly enough isn't used in wolf3d either, as the map size is fixed at 64x64. So they do 64x64x2 to get the
            // final uncompressed size. Since we're here in C# fancy pants land, we can just grab the length of the input array.
            int inputIterator = 2;

            while (inputIterator < input.Length)
            {
                byte topinput = input[inputIterator];
                byte bottominput = input[inputIterator + 1];
                inputIterator += 2;

                if (topinput == topRLEWtag && bottominput == bottomRLEWtag)
                {
                    byte topcount = input[inputIterator];
                    byte bottomcount = input[inputIterator + 1];
                    inputIterator += 2;

                    int count = bottomcount * 256 + topcount;

                    byte topvalue = input[inputIterator];
                    byte bottomvalue = input[inputIterator + 1];
                    inputIterator += 2;

                    while (count > 0)
                    {
                        result.Add(topvalue);
                        result.Add(bottomvalue);
                        count--;
                    }
                }
                else
                {
                    result.Add(topinput);
                    result.Add(bottominput);
                }
            }

            Byte[] output = result.ToArray();

            return output; 
        }

        public byte[] CarmackDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // We're starting at byte 2 because the first two bytes are the uncompressed size of the data.
            int inputIterator = 2;

            // Original source uses length/2 to do this, and subtracts from length everytime a WORD (2 bytes)
            // is passed. We're going to use the length of the input array instead as we easily have access to it.
            while (inputIterator < input.Length)
            {
                // Grab two bytes, topend and bottomend in sequence.
                byte topend = input[inputIterator];
                byte bottomend = input[inputIterator + 1];
                inputIterator += 2;

                // The bottom end tags whether or not this section is compressed, and if so with which form.
                if (bottomend == 0xA7) // One Byte "Near pointer"
                {
                    if (topend == 0x00) // Signals that the original trigger is actually part of the data.
                    {                   // So we grab one more byte, as it is the true topend of this sequence.
                        topend = input[inputIterator];
                        inputIterator++;
                        result.Add(topend);
                        result.Add(bottomend);
                        continue;
                    }
                    else
                    {
                        byte count = topend; // The number of words to copy.
                        int offset = input[inputIterator]; // The offset (in words ) to copy from.
                        inputIterator++;
                        offset *= 2; // We multiply by two because we're dealing with bytes, not words.

                        while (count > 0)
                        {   // We copy the words, count times, from the offset.
                            int outOffset = result.Count() - offset;

                            topend = result[outOffset];
                            bottomend = result[outOffset + 1];
                            result.Add(topend);
                            result.Add(bottomend);
                            count--;
                        }
                        continue;
                    }
                }
                else if (bottomend == 0xA8) // One WORD "Far Pointer"
                {
                    if (topend == 0x00)
                    {
                        topend = input[inputIterator];
                        inputIterator++;
                        result.Add(topend);
                        result.Add(bottomend);
                        continue;
                    }
                    else
                    {
                        byte count = topend;
                        byte offsettop = input[inputIterator];
                        byte offsetbottom = input[inputIterator + 1];
                        inputIterator += 2;

                        Int16 offset = (Int16)(offsetbottom * 256 + offsettop);

                        offset *= 2;

                        while (count > 0)
                        {
                            count--;
                            topend = result[offset];
                            bottomend = result[offset + 1];
                            offset += 2;
                            result.Add(topend);
                            result.Add(bottomend);
                        }
                        continue;
                    }
                    
                }
                else
                {   // There is no compression, just add these bytes to the output.
                    result.Add(topend);
                    result.Add(bottomend);
                }

            }

            Byte[] output = result.ToArray();

            return output;
        }

        public IDdecompression(ref byte[] aMapHead)
        {   // Grab the RWLEtag from the map header.
            topRLEWtag = aMapHead[0];
            bottomRLEWtag = aMapHead[1];
        }
    }


}

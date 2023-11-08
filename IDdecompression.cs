using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// IDdecompression class --
//
// Handles the decompression of data using ID Software's LZ compression and RLEW compression schemes that were popular during this time frame.

namespace Aardwolf
{

    internal class IDdecompression
    {
        private byte lowRWLEtag;
        private byte highRWLEtag;

        public byte[] RLEWDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // Initialize the input index and read the first word
            int inputIndex = 0;
            byte highByte = input[inputIndex];
            inputIndex++;
            byte lowByte = input[inputIndex];
            inputIndex++;

            do
            {
                // Read the next word.
                highByte = input[inputIndex];
                inputIndex++;
                lowByte = input[inputIndex];
                inputIndex++;

                if (highByte == highRWLEtag && lowByte == lowRWLEtag)
                {
                    // This is a compressed block.  Read the next word.
                    highByte = input[inputIndex];
                    inputIndex++;
                    lowByte = input[inputIndex];
                    inputIndex++;

                    // The high byte is the count, the low byte is the value.
                    for (int i = 0; i < highByte; i++)
                    {
                        result.Add(lowByte);
                    }
                }
                else
                {
                    // This is an uncompressed block.  Add the bytes to the output.
                    result.Add(highByte);
                    result.Add(lowByte);
                }
            } while (inputIndex < input.Length);

            byte[] output = new byte[result.Count];

            return output;
        }

        // A C# function that decompresses a byte array using Carmack compression algorithm
        // Reference: https://moddingwiki.shikadi.net/wiki/Carmack_compression
        public byte[] CarmackDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // Initialize the input index and read the first word
            int inputIndex = 0;


            // Loop until the end of the input is reached
            while (inputIndex < input.Length)
            {
                byte lowByte = input[inputIndex];
                inputIndex++;
                byte highByte = input[inputIndex];
                inputIndex++;

                if (highByte == 0xa7)
                {   // This is the high byte trigger for a near pointer.
                    if (lowByte == 0x00)
                    {   // There is no value in the low byte, which means 0xa7 is part of the source.
                        lowByte = input[inputIndex];
                        inputIndex++;
                        result.Add(highByte);
                        result.Add(lowByte);
                    }
                    else
                    {
                        byte offset = input[inputIndex];
                        inputIndex++;
                        int cpyptr = result.Count - offset - 1;
                        while (lowByte > 0)
                        {
                            lowByte--;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                        }
                    }
                }
                else if (highByte == 0xa8)
                {   // This is the high byte trigger for a far pointer.
                    if (lowByte == 0x00)
                    {   // There is no value in the low byte, which means 0xa7 is part of the source.
                        lowByte = input[inputIndex];
                        inputIndex++;
                        result.Add(highByte);
                        result.Add(lowByte);
                    }
                    else
                    {
                        UInt16 offset = (UInt16)(input[inputIndex] | input[inputIndex + 1] << 8);
                        inputIndex += 2;

                        UInt16 cpyptr = offset;
                        while (lowByte > 0)
                        {
                            lowByte--;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                        }
                    }
                }
                else
                {   // There is no compression.  Just add the bytes to the output.
                    result.Add(highByte);
                    result.Add(lowByte);
                }
                
            }

            byte[] output = new byte[result.Count];

            output = result.ToArray();

            // Return the output buffer
            return output;
        }

        public IDdecompression(ref byte[] aMapHead)
        {   // Grab the RWLEtag from the map header.
            highRWLEtag = aMapHead[0];
            lowRWLEtag = aMapHead[1];

            Debug.WriteLine("RWLEtag: {0:X2}{1:X2}", lowRWLEtag, highRWLEtag);
        }
    }


}

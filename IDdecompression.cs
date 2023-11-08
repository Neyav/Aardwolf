using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// IDdecompression class --
//
// Handles the decompression of data using ID Software's LZ compression and RLEW compression schemes that were popular during this time frame.

namespace Aardwolf
{
    internal class WORD16BIT
    {
        public byte lowByte;
        public byte highByte;

        public void setWORD16BIT(byte[] input, ref int inputIndex)
        {
            lowByte = input[inputIndex];
            inputIndex++;
            highByte = input[inputIndex];
            inputIndex++;
        }
        public UInt16 getWORD16BIT()
        {
            UInt16 output = (UInt16)(lowByte | highByte << 8);

            return output;
        }

        public WORD16BIT()
        {
            lowByte = 0;
            highByte = 0;
        }
    }

    internal class IDdecompression
    {
        private byte lowRWLEtag;
        private byte highRWLEtag;

        public byte[] RLEWDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // Initialize the input index and read the first word
            int inputIndex = 2;
            byte highByte = 0;
            byte lowByte = 0;

            do
            {
                byte inputByte = input[inputIndex];
                inputIndex++;
                if (inputByte == highRWLEtag)
                {
                    inputByte = input[inputIndex];
                    inputIndex++;
                    if (inputByte == lowRWLEtag)
                    {
                        // This is a compressed word.  Grab the next byte and repeat it the number of times specified by the next byte.
                        inputByte = input[inputIndex];
                        inputIndex++;
                        byte repeatCount = input[inputIndex];
                        inputIndex++;
                        for (int i = 0; i < repeatCount; i++)
                        {
                            result.Add(inputByte);
                        }
                    }
                    else
                    {
                        // This is not a compressed word.  Add the two bytes to the output.
                        result.Add(highRWLEtag);
                        result.Add(inputByte);
                    }
                }
                else
                {
                    // This is not a compressed word.  Add the byte to the output.
                    result.Add(inputByte);
                }


            } while (inputIndex < input.Length);

            byte[] output = new byte[result.Count];

            output = result.ToArray();

            return output;
        }

        // A C# function that decompresses a byte array using Carmack compression algorithm
        // Reference: https://moddingwiki.shikadi.net/wiki/Carmack_compression
        public byte[] CarmackDecompress(byte[] input)
        {
            List<byte> result = new List<byte>();

            // Initialize the input index and read the first word
            int inputIndex = 0;
            WORD16BIT lenWORD = new WORD16BIT();
            lenWORD.setWORD16BIT(input, ref inputIndex);
            UInt16 len = lenWORD.getWORD16BIT();

            Debug.WriteLine("CarmackDecompress: len: {0}", len);

            // Loop until the end of the input is reached
            while (inputIndex < input.Length)
            {
                WORD16BIT word = new WORD16BIT();
                
                word.setWORD16BIT(input, ref inputIndex);

                if (word.highByte == 0xA7)
                {   // This is the high byte trigger for a near pointer.
                    if (word.lowByte == 0x00)
                    {   // There is no value in the low byte, which means 0xA7 is part of the source.
                        word.lowByte = input[inputIndex];
                        inputIndex++;
                        result.Add(word.highByte);
                        result.Add(word.lowByte);
                    }
                    else
                    {
                        byte offset = input[inputIndex];
                        inputIndex++;
                        int cpyptr = result.Count - 1 - (offset * 2); // We're moving in 16 bit words.
                        while (word.lowByte > 0)
                        {
                            word.lowByte--;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                        }
                    }
                }
                else if (word.highByte == 0xA8)
                {   // This is the high byte trigger for a far pointer.
                    if (word.lowByte == 0x00)
                    {   // There is no value in the low byte, which means 0xa7 is part of the source.
                        word.lowByte = input[inputIndex];
                        inputIndex++;
                        result.Add(word.highByte);
                        result.Add(word.lowByte);
                    }
                    else
                    {
                        WORD16BIT offsetWORD = new WORD16BIT();
                        offsetWORD.setWORD16BIT(input, ref inputIndex);
                        UInt16 offset = offsetWORD.getWORD16BIT();

                        UInt16 cpyptr = (UInt16) (offset * 2); // We're moving in 16 bit words.
                        while (word.lowByte > 0)
                        {
                            word.lowByte--;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                            result.Add(result[cpyptr]);
                            cpyptr++;
                        }
                    }
                }
                else
                {   // There is no compression.  Just add the bytes to the output.
                    result.Add(word.highByte);
                    result.Add(word.lowByte);
                }
                
            }

            byte[] output = new byte[result.Count];

            output = result.ToArray();

            // Return the output buffer
            return output;
        }

        public IDdecompression(ref byte[] aMapHead)
        {   // Grab the RWLEtag from the map header.
            lowRWLEtag = aMapHead[0];
            highRWLEtag = aMapHead[1];

            Debug.WriteLine("RWLEtag: {0:X2}{1:X2}", lowRWLEtag, highRWLEtag);
        }
    }


}

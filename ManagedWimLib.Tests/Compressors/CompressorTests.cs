/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2020 Hajin Jang

    This file is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free
    Software Foundation; either version 3 of the License, or (at your option) any
    later version.

    This file is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
    details.

    You should have received a copy of the GNU Lesser General Public License
    along with this file; if not, see http://www.gnu.org/licenses/.
*/

// #define DEBUG_BLOCKSIZE

using ManagedWimLib.Compressors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ManagedWimLib.Tests.Compressors
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class CompressorTests
    {
        [TestMethod]
        public void CompressDecompress()
        {
            string srcFilePath = Path.Combine(TestSetup.SampleDir, "Compressors", "Source.txt");
            long srcFileSize = new FileInfo(srcFilePath).Length;
            Console.WriteLine($"Source : {srcFileSize / 1024.0:#.0}KB");

            void Template(CompressionType cType, int maxBlockSizeExp, bool useSpan)
            {
                int maxBlockSize = 1 << maxBlockSizeExp;
                byte[] srcBuffer = new byte[maxBlockSize];
                byte[] destBuffer = new byte[maxBlockSize];

                byte[] rawDigest;
                byte[] compDigest;

                int totalCompSize = 0;
                int totalDecompSize = 0;
                List<(int RawSize, int CompSize)> blockSizes = new List<(int, int)>((int)(srcFileSize / maxBlockSize) + 1);
                using (MemoryStream compMemStream = new MemoryStream((int)srcFileSize))
                {
                    // Compress first
                    using (Compressor compressor = Compressor.Create(cType, maxBlockSize, 50, CompressorFlags.None))
                    using (FileStream srcFileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        HashAlgorithm hash = SHA256.Create();

                        int readBytes = 0;
                        do
                        {
                            readBytes = srcFileStream.Read(srcBuffer, 0, srcBuffer.Length);
                            if (readBytes == 0)
                                break;

                            int compBytes;
                            if (useSpan)
                            {
                                ReadOnlySpan<byte> srcSpan = srcBuffer.AsSpan(0, readBytes);
                                Span<byte> destSpan = destBuffer.AsSpan(0, destBuffer.Length);
                                compBytes = compressor.Compress(srcSpan, destSpan);
                            }
                            else
                            {
                                compBytes = compressor.Compress(srcBuffer, 0, readBytes, destBuffer, 0, destBuffer.Length);
                            }

                            Assert.IsTrue(0 < compBytes);
                            compMemStream.Write(destBuffer, 0, compBytes);
                            totalCompSize += compBytes;

                            hash.TransformBlock(srcBuffer, 0, readBytes, srcBuffer, 0);
                            blockSizes.Add((readBytes, compBytes));

#if DEBUG_BLOCKSIZE
                            Console.WriteLine($"Compressed [{readBytes}B] into [{compBytes}B]");
#endif
                        }
                        while (0 < readBytes);

                        rawDigest = hash.TransformFinalBlock(srcBuffer, 0, 0);
                    }

                    Console.WriteLine($"{cType}, {maxBlockSizeExp} : {totalCompSize / 1024.0:#.0}KB");
                    Assert.IsTrue(totalCompSize < srcFileSize);

                    // Try decompressing
                    compMemStream.Position = 0;
                    using (Decompressor decompressor = Decompressor.Create(cType, maxBlockSize))
                    using (HashAlgorithm hash = SHA256.Create())
                    {
                        for (int i = 0; i < blockSizes.Count; i++)
                        {
                            int rawBytes = blockSizes[i].RawSize;
                            int compBytes = blockSizes[i].CompSize;

                            int readBytes = compMemStream.Read(srcBuffer, 0, compBytes);
                            if (readBytes == 0)
                                break;
                            Assert.AreEqual(compBytes, readBytes);

                            bool ret;
                            if (useSpan)
                            {
                                ReadOnlySpan<byte> srcSpan = srcBuffer.AsSpan(0, compBytes);
                                Span<byte> destSpan = destBuffer.AsSpan(0, rawBytes);
                                ret = decompressor.Decompress(srcSpan, destSpan, rawBytes);
                            }
                            else
                            {
                                ret = decompressor.Decompress(srcBuffer, 0, compBytes, destBuffer, 0, rawBytes);
                            }
                            Assert.IsTrue(ret);

                            hash.TransformBlock(destBuffer, 0, rawBytes, destBuffer, 0);
                            totalDecompSize += rawBytes;

#if DEBUG_BLOCKSIZE
                            Console.WriteLine($"Decompressed [{compBytes}B] into [{rawBytes}B]");
#endif
                        }
                        compDigest = hash.TransformFinalBlock(destBuffer, 0, 0);
                    }

                    Assert.IsTrue(totalCompSize < totalDecompSize);
                    Assert.AreEqual(srcFileSize, totalDecompSize);
                    Assert.IsTrue(rawDigest.SequenceEqual(compDigest));
#if DEBUG_BLOCKSIZE
                    Console.WriteLine();
#endif
                }
            }

            foreach (bool useSpan in new bool[] { false, true })
            {
                Console.WriteLine();
                Console.WriteLine($"UseSpan : {useSpan}");

                Template(CompressionType.XPRESS, 12, useSpan);
                Template(CompressionType.XPRESS, 16, useSpan);

                Template(CompressionType.LZX, 15, useSpan);
                Template(CompressionType.LZX, 21, useSpan);

                Template(CompressionType.LZMS, 15, useSpan);
                Template(CompressionType.LZMS, 24, useSpan);
            }
        }
    }
}

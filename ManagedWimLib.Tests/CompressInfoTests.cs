/*
    Licensed under LGPLv3

    Derived from wimlib's original header files
    Copyright (C) 2012-2018 Eric Biggers

    C# Wrapper written by Hajin Jang
    Copyright (C) 2017-2020 Hajin Jang

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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ManagedWimLib.Tests
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class CompressInfoTests
    {
        #region SetDefaultCompressionLevel
        [TestMethod]
        public void SetDefaultCompressionLevel()
        {
            void ShouldFailTemplate(Action action)
            {
                bool success = false;
                try
                {
                    action.Invoke();
                }
                catch (WimLibException e) when (e.ErrorCode == ErrorCode.InvalidCompressionType)
                {
                    success = true;
                }
                Assert.IsTrue(success);
            }

            try
            {
                ShouldFailTemplate(() => Wim.SetDefaultCompressionLevel(CompressionType.None, Wim.DefaultCompressionLevel, CompressorFlags.None));
                ShouldFailTemplate(() => Wim.SetDefaultCompressionLevel(CompressionType.None, Wim.DefaultCompressionLevel, CompressorFlags.Destructive));

                Assert.AreEqual(0u, Wim.DefaultCompressionLevel);
                foreach (CompressorFlags flag in (CompressorFlags[])Enum.GetValues(typeof(CompressorFlags)))
                {
                    for (uint level = Wim.DefaultCompressionLevel; level <= 120; level += 10)
                    {
                        Wim.SetDefaultCompressionLevel(CompressionType.XPRESS, level, flag);
                        Wim.SetDefaultCompressionLevel(CompressionType.LZX, level, flag);
                        Wim.SetDefaultCompressionLevel(CompressionType.LZMS, level, flag);
                        Wim.SetEveryDefaultCompressionLevel(level, flag);
                    }
                }
            }
            finally
            {
                Wim.SetEveryDefaultCompressionLevel(Wim.DefaultCompressionLevel, CompressorFlags.None);
            }
        }

        [TestMethod]
        public void GetCompressorNeededMemory()
        {
            void Template(CompressionType ctype, ulong maxBlockSize, uint compLevel, CompressorFlags flags, bool success, bool expectExcept = false)
            {
                string msg = $"({ctype}, {maxBlockSize}, {success}, {flags})";

                ulong requiredMemory = 0;
                try
                {
                    requiredMemory = Wim.GetCompressorNeededMemory(ctype, maxBlockSize, compLevel, flags);
                }
                catch (ArgumentException e)
                {
                    if (!success && expectExcept)
                    {
                        Console.WriteLine($"{msg} = (Intended) {e.Message}");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"{msg} = (Unintended) {e.Message}");
                        Assert.Fail();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{msg} = (Unintended) {e.Message}");
                    Assert.Fail();
                }

                if (expectExcept)
                {
                    Console.WriteLine($"{msg} = (Unintended) No Exception!");
                    Assert.Fail();
                }

                if (success)
                {
                    Console.WriteLine($"{msg} = {requiredMemory}");
                    Assert.AreNotEqual(0uL, requiredMemory);
                }
                else
                {
                    Console.WriteLine($"{msg} = (Intended) Error");
                    Assert.AreEqual(0uL, requiredMemory);
                }                
            }

            switch (TestSetup.PlatformBitness)
            {
                case 32:
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 36), Wim.DefaultCompressionLevel, CompressorFlags.None, false, true);
                    break;
                case 64:
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 48), Wim.DefaultCompressionLevel, CompressorFlags.None, false, false);
                    break;
            }

            Assert.AreEqual(0u, Wim.DefaultCompressionLevel);
            foreach (CompressorFlags flag in (CompressorFlags[])Enum.GetValues(typeof(CompressorFlags)))
            {
                Console.WriteLine($"CompressorFlags = {flag}");

                for (uint level = Wim.DefaultCompressionLevel; level <= 120; level += 10)
                {
                    Console.WriteLine($"Level = {level}");

                    Template(CompressionType.None, (ulong)Math.Pow(2, 15), level, flag, false);

                    Template(CompressionType.XPRESS, 0, level, flag, false);
                    Template(CompressionType.XPRESS, (ulong)Math.Pow(2, 8), level, flag, true);
                    Template(CompressionType.XPRESS, (ulong)Math.Pow(2, 14), level, flag, true);
                    Template(CompressionType.XPRESS, (ulong)Math.Pow(2, 15), level, flag, true);
                    Template(CompressionType.XPRESS, (ulong)Math.Pow(2, 18), level, flag, false);

                    Template(CompressionType.LZX, 0, level, flag, false);
                    Template(CompressionType.LZX, (ulong)Math.Pow(2, 12), level, flag, true);
                    Template(CompressionType.LZX, (ulong)Math.Pow(2, 16), level, flag, true);
                    Template(CompressionType.LZX, (ulong)Math.Pow(2, 20), level, flag, true);
                    Template(CompressionType.LZX, (ulong)Math.Pow(2, 24), level, flag, false);

                    Template(CompressionType.LZMS, 0, level, flag, false);
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 12), level, flag, true);
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 16), level, flag, true);
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 24), level, flag, true);
                    Template(CompressionType.LZMS, (ulong)Math.Pow(2, 31), level, flag, false);

                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
        #endregion
    }
}

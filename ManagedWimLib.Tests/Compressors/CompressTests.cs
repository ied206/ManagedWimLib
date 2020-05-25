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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ManagedWimLib.Compressors;

namespace ManagedWimLib.Tests.Compressors
{
    [TestClass]
    [TestCategory(TestSetup.WimLib)]
    public class CompressTests
    {
        [TestMethod]
        public void Compress()
        {
            /*
            int maxBlockSize = 1 << 16;
            Compressor compressor = Compressor.CreateCompressor(CompressionType.XPRESS, maxBlockSize, 50, CompressorFlags.None);
            compressor.Compress();
            */
        }
    }
}
